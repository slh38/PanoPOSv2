using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Customer;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Customer;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests.Customer;

public sealed class CariKartServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly CariKartServisi _cariKartServisi;

    public CariKartServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _cariKartServisi = new CariKartServisi(_dbContext);
    }

    [Fact]
    public async Task CariKart_eklenir()
    {
        var cari = await _cariKartServisi.CariKartOlusturAsync(new CariKartOlusturRequestDto
        {
            SubeId = 1,
            CariKodu = "CR-001",
            Ad = "ABC Tedarik",
            Tip = CariTipi.Satici,
            Telefon = "5551112233"
        });

        Assert.Equal("ABC Tedarik", cari.Ad);
        Assert.Equal("CR-001", cari.CariKodu);
        Assert.Equal(CariTipi.Satici, cari.Tip);
    }

    [Fact]
    public async Task Duplicate_kontrolu_calisir()
    {
        await CariKartEkleAsync("CR-002", "Ilk Cari");

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => CariKartEkleAsync("CR-002", "Ikinci Cari"));

        Assert.Equal("cari_kart_kodu_duplicate", ex.ErrorCode);
    }

    [Fact]
    public async Task Liste_sayfali_doner()
    {
        await CariKartEkleAsync("CR-101", "Zeta");
        await CariKartEkleAsync("CR-102", "Beta");
        await CariKartEkleAsync("CR-103", "Alfa");

        var sayfa = await _cariKartServisi.CariKartListeleAsync(1, null, 2, 2);

        Assert.Equal(3, sayfa.ToplamKayit);
        Assert.Equal(2, sayfa.Sayfa);
        Assert.Equal(2, sayfa.SayfaBoyutu);
        Assert.Single(sayfa.Kayitlar);
    }

    [Fact]
    public async Task Soft_delete_filtre_calisir()
    {
        var cari = await CariKartEkleAsync("CR-201", "Silinecek Cari");
        var entity = await _dbContext.CariKartlar.IgnoreQueryFilters().SingleAsync(x => x.Id == cari.Id);
        entity.SoftDelete(null, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync();

        var sayfa = await _cariKartServisi.CariKartListeleAsync(1, null, 1, 20);

        Assert.DoesNotContain(sayfa.Kayitlar, x => x.Id == cari.Id);
    }

    private Task<CariKartDto> CariKartEkleAsync(string kod, string ad)
    {
        return _cariKartServisi.CariKartOlusturAsync(new CariKartOlusturRequestDto
        {
            SubeId = 1,
            CariKodu = kod,
            Ad = ad,
            Tip = CariTipi.Alici
        });
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
