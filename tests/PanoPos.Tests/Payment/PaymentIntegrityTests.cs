using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Payment;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Payment;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests.Payment;

public sealed partial class TahsilatServisiTests
{
    private static TahsilatOlusturRequestDto Odeme(long faturaId, long? kasaId, decimal tutar = 100m)
        => new()
        {
            IslemAnahtari = Guid.NewGuid(),
            SubeId = 1,
            FaturaId = faturaId,
            KullaniciId = 1,
            CihazId = 1,
            OdemeTipi = OdemeTipi.Nakit,
            KasaId = kasaId,
            Tutar = tutar,
            ParaBirimKodu = "TRY",
            Kur = 1m
        };

    [Fact]
    public async Task Altiyuz_nakit_dortyuz_kart_gercek_toplamla_kapanir_ve_filtrelenir()
    {
        var kasa = await KasaEkleAsync();
        var banka = await BankaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 1000m);
        var first = await _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id, 600));
        Assert.Equal(600, first.FaturaOdenenTutar);
        Assert.Equal(400, first.FaturaKalanTutar);
        Assert.Equal(FaturaDurumu.Acik, first.FaturaDurumu);
        var card = Odeme(fatura.Id, null, 400);
        card.OdemeTipi = OdemeTipi.KrediKarti; card.BankaId = banka.Id;
        var last = await _tahsilatServisi.TahsilatOlusturAsync(card);
        Assert.Equal(1000, last.FaturaOdenenTutar);
        Assert.Equal(0, last.FaturaKalanTutar);
        Assert.Equal(FaturaDurumu.Kapali, last.FaturaDurumu);
        var other = await FaturaEkleAsync(netToplam: 100);
        await _tahsilatServisi.TahsilatOlusturAsync(Odeme(other.Id, kasa.Id));
        var list = await _tahsilatServisi.TahsilatListeleAsync(1, 1, 1, faturaId: fatura.Id);
        Assert.Equal(2, list.ToplamKayit);
        Assert.Equal(last.Id, Assert.Single(list.Kayitlar).Id);
        var second = await _tahsilatServisi.TahsilatListeleAsync(1, 2, 1, faturaId: fatura.Id);
        Assert.Equal(first.Id, Assert.Single(second.Kayitlar).Id);
        Assert.Single(await _dbContext.BankaHareketleri.ToListAsync());
        Assert.Empty(await _dbContext.StokHareketleri.ToListAsync());
        Assert.Empty(await _dbContext.StokFisleri.ToListAsync());
    }

    [Fact]
    public async Task Sekizyuz_sonrasi_ucyuz_reddedilir_gercek_odeme_korunur()
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 1000);
        await _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id, 800));
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() =>
            _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id, 300)));
        Assert.Equal("payment_total_exceeds_invoice", error.ErrorCode);
        var stored = await _dbContext.Faturalar.SingleAsync(x => x.Id == fatura.Id);
        Assert.Equal(800, stored.OdenenTutar); Assert.Equal(200, stored.KalanTutar);
        Assert.Single(await _dbContext.Tahsilatlar.ToListAsync());
        Assert.Single(await _dbContext.KasaHareketleri.ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    public async Task Eksik_gercek_tahsilatla_kapatma_reddedilir(int paid)
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 100);
        if (paid > 0) await _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id, paid));
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() => new FaturaServisi(_dbContext)
            .FaturaKapatAsync(fatura.Id, new FaturaKapatRequestDto { KapatanKullaniciId = 1 }));
        Assert.Equal("invoice_not_fully_paid", error.ErrorCode);
        var stored = await _dbContext.Faturalar.SingleAsync(x => x.Id == fatura.Id);
        Assert.Equal(paid, stored.OdenenTutar); Assert.Equal(100 - paid, stored.KalanTutar);
        Assert.Equal(FaturaDurumu.Acik, stored.Durum);
    }

    [Fact]
    public async Task Header_tutari_yerine_gercek_tahsilat_toplanir()
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 1000);
        fatura.OdenenTutar = 900; fatura.KalanTutar = 100;
        await _dbContext.SaveChangesAsync();
        var result = await _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id, 600));
        Assert.Equal(600, result.FaturaOdenenTutar); Assert.Equal(400, result.FaturaKalanTutar);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Ayni_anahtar_tekrari_hareket_ve_outbox_uretmez(int amount)
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 100);
        var service = new TahsilatServisi(_dbContext, new OutboxServisi(_dbContext));
        var request = Odeme(fatura.Id, kasa.Id, amount);
        var first = await service.TahsilatOlusturAsync(request);
        request.Tutar = amount == 100 ? 100.00m : 50.00m;
        request.Kur = 1.000000m; request.ParaBirimKodu = " try ";
        var second = await service.TahsilatOlusturAsync(request);
        Assert.Equal(first.Id, second.Id);
        Assert.Single(await _dbContext.Tahsilatlar.ToListAsync());
        Assert.Single(await _dbContext.KasaHareketleri.ToListAsync());
        Assert.Single(await _dbContext.OutboxOlaylari.ToListAsync());
        if (amount == 100)
        {
            await new FaturaServisi(_dbContext).FaturaKapatAsync(fatura.Id,
                new FaturaKapatRequestDto { KapatanKullaniciId = 1 });
            Assert.Single(await _dbContext.Tahsilatlar.ToListAsync());
        }
    }

    [Theory]
    [InlineData("tutar")]
    [InlineData("fatura")]
    [InlineData("tip")]
    [InlineData("kur")]
    [InlineData("para")]
    [InlineData("kasa")]
    [InlineData("banka")]
    [InlineData("aciklama")]
    [InlineData("tarih")]
    public async Task Ayni_anahtar_farkli_payload_conflict(string field)
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 1000);
        var request = Odeme(fatura.Id, kasa.Id);
        await _tahsilatServisi.TahsilatOlusturAsync(request);
        switch (field)
        {
            case "tutar": request.Tutar = 101; break;
            case "fatura": request.FaturaId = (await FaturaEkleAsync()).Id; break;
            case "tip": request.OdemeTipi = OdemeTipi.KrediKarti; break;
            case "kur": request.Kur = 2; break;
            case "para": request.ParaBirimKodu = "USD"; break;
            case "kasa": request.KasaId = (await KasaEkleAsync()).Id; break;
            case "banka": request.BankaId = (await BankaEkleAsync()).Id; break;
            case "aciklama": request.Aciklama = "farkli"; break;
            case "tarih": request.TahsilatTarihi = DateTime.UtcNow; break;
        }
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() => _tahsilatServisi.TahsilatOlusturAsync(request));
        Assert.Equal(409, error.StatusCode); Assert.Equal("payment_key_conflict", error.ErrorCode);
        Assert.Single(await _dbContext.Tahsilatlar.ToListAsync());
        Assert.Single(await _dbContext.KasaHareketleri.ToListAsync());
    }

    [Theory]
    [InlineData("bos")]
    [InlineData("tutar")]
    [InlineData("kur")]
    public async Task Anahtar_ve_parasal_hassasiyet_dogrulanir(string field)
    {
        var request = Odeme(1, 1);
        if (field == "bos") request.IslemAnahtari = Guid.Empty;
        if (field == "tutar") request.Tutar = 0.001m;
        if (field == "kur") request.Kur = 1.0000001m;
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() => _tahsilatServisi.TahsilatOlusturAsync(request));
        Assert.Equal(400, error.StatusCode);
        Assert.Empty(await _dbContext.Tahsilatlar.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Gecersiz_eski_tahsilat_anahtari_yeniden_kullanilamaz(bool deleted)
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync();
        var request = Odeme(fatura.Id, kasa.Id);
        await _tahsilatServisi.TahsilatOlusturAsync(request);
        var stored = await _dbContext.Tahsilatlar.SingleAsync();
        stored.SilindiMi = deleted; stored.AktifMi = false;
        await _dbContext.SaveChangesAsync();
        Assert.Equal("payment_key_conflict", (await Assert.ThrowsAsync<UygulamaHatasi>(
            () => _tahsilatServisi.TahsilatOlusturAsync(request))).ErrorCode);
        Assert.Equal(0, (await _tahsilatServisi.TahsilatListeleAsync(1, 1, 10, faturaId: fatura.Id)).ToplamKayit);
        Assert.Single(await _dbContext.Tahsilatlar.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task Anahtar_unique_korumasi_veritabaninda_da_calisir()
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync();
        await _tahsilatServisi.TahsilatOlusturAsync(Odeme(fatura.Id, kasa.Id));
        var original = await _dbContext.Tahsilatlar.AsNoTracking().SingleAsync();
        original.Id = 0; original.TahsilatFisNo = "FARKLI";
        _dbContext.Tahsilatlar.Add(original);
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Theory]
    [InlineData(OdemeTipi.Nakit)]
    [InlineData(OdemeTipi.KrediKarti)]
    [InlineData(OdemeTipi.Veresiye)]
    public async Task Outbox_hatasi_tahsilat_hareket_ve_fatura_guncellemesini_geri_alir(OdemeTipi type)
    {
        var kasa = await KasaEkleAsync();
        var banka = await BankaEkleAsync();
        var cari = await CariKartEkleAsync();
        var fatura = await FaturaEkleAsync(netToplam: 100, cariId: cari.Id);
        var request = Odeme(fatura.Id, kasa.Id);
        request.OdemeTipi = type; request.BankaId = banka.Id;
        await using var db = new PanoPosDbContext(new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection).AddInterceptors(new OutboxFailure()).Options);
        var service = new TahsilatServisi(db, new OutboxServisi(db));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TahsilatOlusturAsync(request));
        _dbContext.ChangeTracker.Clear();
        Assert.Empty(await _dbContext.Tahsilatlar.ToListAsync());
        Assert.Empty(await _dbContext.KasaHareketleri.ToListAsync());
        Assert.Empty(await _dbContext.BankaHareketleri.ToListAsync());
        Assert.Empty(await _dbContext.CariHareketleri.ToListAsync());
        Assert.Empty(await _dbContext.OutboxOlaylari.ToListAsync());
        var stored = await _dbContext.Faturalar.SingleAsync(x => x.Id == fatura.Id);
        Assert.Equal(0, stored.OdenenTutar); Assert.Equal(100, stored.KalanTutar);
        Assert.Equal(FaturaDurumu.Acik, stored.Durum);
    }

    private sealed class OutboxFailure : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<OutboxOlay>().Any(x => x.State == EntityState.Added))
                throw new InvalidOperationException("Injected outbox write failure");
            return ValueTask.FromResult(result);
        }
    }
}

