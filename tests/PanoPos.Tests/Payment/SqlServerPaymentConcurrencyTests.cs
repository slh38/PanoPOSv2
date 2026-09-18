using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PanoPos.Application.Common;
using PanoPos.Application.Payment;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Payment;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Payment;

public sealed class SqlServerPaymentConcurrencyTests
{
    public sealed class SqlServerTheoryAttribute : TheoryAttribute
    {
        public SqlServerTheoryAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PANOPOS_TEST_SQLSERVER")))
                Skip = "Set PANOPOS_TEST_SQLSERVER to the migrated development PanoPosDb.";
        }
    }

    [SqlServerTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Concurrent_payment_is_atomic_and_idempotent(bool sameKey, bool differentInvoice)
    {
        var connection = Environment.GetEnvironmentVariable("PANOPOS_TEST_SQLSERVER")!;
        Assert.Equal("PanoPosDb", new SqlConnectionStringBuilder(connection).InitialCatalog);
        var options = new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlServer(connection).Options;
        await using var setup = new PanoPosDbContext(options);
        var tenant = SystemSeedData.TenantGuid;
        var kasa = new Kasa { TenantId = tenant, SubeId = 1, Ad = "Payment test " + Guid.NewGuid().ToString("N") };
        Fatura Invoice() => new()
        {
            TenantId = tenant,
            SubeId = 1,
            DepoId = 1,
            FaturaNo = "TEST-" + Guid.NewGuid().ToString("N"),
            ParaBirimKodu = "TRY",
            Kur = 1,
            AraToplam = 100,
            NetToplam = 100,
            ToplamTutar = 100,
            KalanTutar = 100,
            Durum = FaturaDurumu.Acik
        };
        var firstInvoice = Invoice(); var secondInvoice = Invoice();
        setup.AddRange(kasa, firstInvoice, secondInvoice);
        await setup.SaveChangesAsync();
        var firstId = firstInvoice.Id;
        var secondId = secondInvoice.Id;
        try
        {
            var gate = new LockGate();
            var concurrentOptions = new DbContextOptionsBuilder<PanoPosDbContext>()
                .UseSqlServer(connection).AddInterceptors(gate).Options;
            var key = Guid.NewGuid();
            async Task<(TahsilatDto? Payment, UygulamaHatasi? Error)> Pay(bool second)
            {
                await using var db = new PanoPosDbContext(concurrentOptions);
                try
                {
                    var result = await new TahsilatServisi(db).TahsilatOlusturAsync(new TahsilatOlusturRequestDto
                    {
                        IslemAnahtari = second && !sameKey ? Guid.NewGuid() : key,
                        FaturaId = second && differentInvoice ? secondInvoice.Id : firstInvoice.Id,
                        SubeId = 1,
                        KullaniciId = 1,
                        CihazId = 1,
                        KasaId = kasa.Id,
                        OdemeTipi = OdemeTipi.Nakit,
                        Tutar = 100,
                        ParaBirimKodu = "TRY",
                        Kur = 1
                    });
                    return (result, null);
                }
                catch (UygulamaHatasi error) { return (null, error); }
            }
            var results = await Task.WhenAll(Pay(false), Pay(true)).WaitAsync(TimeSpan.FromSeconds(60));
            var payments = await setup.Tahsilatlar.AsNoTracking().Where(x => x.FaturaId == firstId || x.FaturaId == secondId).ToListAsync();
            var payment = Assert.Single(payments);
            Assert.Equal(100, payment.Tutar);
            Assert.Equal(1, await setup.KasaHareketleri.CountAsync(x => x.KasaId == kasa.Id));
            if (sameKey && !differentInvoice)
            {
                Assert.All(results, x => { Assert.Null(x.Error); Assert.Equal(payment.Id, x.Payment!.Id); });
            }
            else
            {
                Assert.Single(results.Where(x => x.Payment != null));
                var error = Assert.Single(results.Where(x => x.Error != null)).Error!;
                Assert.Equal(409, error.StatusCode);
                if (differentInvoice) Assert.Equal("payment_key_conflict", error.ErrorCode);
            }
            var invoices = await setup.Faturalar.AsNoTracking().Where(x => x.Id == firstId || x.Id == secondId).ToListAsync();
            Assert.Equal(100, invoices.Sum(x => x.OdenenTutar));
            var receipt = await new PanoPos.Infrastructure.Invoice.FaturaServisi(setup).FaturaGetirAsync(payment.FaturaId);
            Assert.Equal(payment.Id, Assert.Single(receipt.Odemeler).TahsilatId);
            Assert.Equal(kasa.Id, receipt.Odemeler[0].KasaId);
            Assert.Equal(kasa.Ad, receipt.Odemeler[0].KasaAdi);
            Assert.Equal(100, receipt.NakitToplam);
            Assert.Equal(receipt.Odemeler.Sum(x => x.Tutar), receipt.OdenenTutar);
            Assert.All(invoices, x => Assert.Equal(x.NetToplam - x.OdenenTutar, x.KalanTutar));
        }
        finally
        {
            // Delete only records allocated by this test; never reset the development database.
            await setup.KasaHareketleri.Where(x => x.TenantId == tenant && x.KasaId == kasa.Id).ExecuteDeleteAsync();
            await setup.Tahsilatlar.Where(x => x.TenantId == tenant && (x.FaturaId == firstId || x.FaturaId == secondId)).ExecuteDeleteAsync();
            await setup.Faturalar.Where(x => x.TenantId == tenant && (x.Id == firstId || x.Id == secondId)).ExecuteDeleteAsync();
            await setup.Kasalar.Where(x => x.TenantId == tenant && x.Id == kasa.Id).ExecuteDeleteAsync();
        }
    }

    private sealed class LockGate : DbCommandInterceptor
    {
        private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrived;
        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("FROM Fatura WITH (UPDLOCK,HOLDLOCK)", StringComparison.Ordinal))
            {
                if (Interlocked.Increment(ref arrived) == 2) ready.TrySetResult();
                await ready.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
            }
            return result;
        }
    }
}
