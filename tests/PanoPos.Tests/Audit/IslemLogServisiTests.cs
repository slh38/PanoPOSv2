using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Audit;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Audit;

public sealed class IslemLogServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly IslemLogServisi _islemLogServisi;

    public IslemLogServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _islemLogServisi = new IslemLogServisi(_dbContext);
    }

    [Fact]
    public async Task Log_eklenir()
    {
        var log = await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            KullaniciId = 1,
            ModulAdi = "Order",
            EkranAdi = "Siparis",
            ButonAdi = "Kaydet",
            IslemTipi = "Create",
            HedefTablo = "Siparis",
            HedefId = 99,
            Aciklama = "Siparis olusturuldu",
            BasariliMi = true,
            CorrelationId = "corr-1"
        });

        Assert.True(log.Id > 0);
        Assert.Equal("Order", log.ModulAdi);
    }

    [Fact]
    public async Task Log_filtreli_listelenir()
    {
        await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            KullaniciId = 1,
            ModulAdi = "Order",
            IslemTipi = "Create",
            BasariliMi = true
        });

        await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            KullaniciId = null,
            ModulAdi = "Invoice",
            IslemTipi = "Create",
            BasariliMi = false,
            HataKodu = "fail"
        });

        var liste = await _islemLogServisi.ListeleAsync(new IslemLogListeRequestDto
        {
            SubeId = 1,
            KullaniciId = 1,
            BasariliMi = true,
            IslemTipi = "Create",
            Page = 1,
            PageSize = 10
        });

        var kayit = Assert.Single(liste.Kayitlar);
        Assert.Equal((long)1, kayit.KullaniciId);
        Assert.True(kayit.BasariliMi);
    }

    [Fact]
    public async Task Log_detay_getirilir()
    {
        var eklenen = await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            KullaniciId = 1,
            ModulAdi = "Payment",
            EkranAdi = "Tahsilat",
            ButonAdi = "TahsilatAl",
            IslemTipi = "Create",
            HedefTablo = "Tahsilat",
            HedefId = 15,
            BasariliMi = true,
            Aciklama = "Tahsilat tamamlandi",
            SureMs = 45
        });

        var detay = await _islemLogServisi.DetayGetirAsync(eklenen.Id);

        Assert.Equal(eklenen.Id, detay.Id);
        Assert.Equal("Payment", detay.ModulAdi);
        Assert.Equal(45, detay.SureMs);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}

