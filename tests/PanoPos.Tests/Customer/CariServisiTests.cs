using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Customer;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Customer;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests.Customer;

public sealed class CariServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly CariServisi _cariServisi;

    public CariServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _cariServisi = new CariServisi(_dbContext);
    }

    [Fact]
    public async Task Cari_eklenir()
    {
        var cari = await _cariServisi.CariOlusturAsync(new CariOlusturRequestDto
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
        await CariEkleAsync("CR-002", "Ilk Cari");

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => CariEkleAsync("CR-002", "Ikinci Cari"));

        Assert.Equal("cari_kodu_duplicate", ex.ErrorCode);
    }

    [Fact]
    public async Task Liste_sayfali_doner()
    {
        await CariEkleAsync("CR-101", "Zeta");
        await CariEkleAsync("CR-102", "Beta");
        await CariEkleAsync("CR-103", "Alfa");

        var sayfa = await _cariServisi.CariListeleAsync(1, null, 2, 2);

        Assert.Equal(3, sayfa.ToplamKayit);
        Assert.Equal(2, sayfa.Sayfa);
        Assert.Equal(2, sayfa.SayfaBoyutu);
        Assert.Single(sayfa.Kayitlar);
    }

    [Fact]
    public async Task Soft_delete_filtre_calisir()
    {
        var cari = await CariEkleAsync("CR-201", "Silinecek Cari");
        var entity = await _dbContext.Cariler.IgnoreQueryFilters().SingleAsync(x => x.Id == cari.Id);
        entity.SoftDelete(null, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync();

        var sayfa = await _cariServisi.CariListeleAsync(1, null, 1, 20);

        Assert.DoesNotContain(sayfa.Kayitlar, x => x.Id == cari.Id);
    }

    private Task<CariDto> CariEkleAsync(string kod, string ad)
    {
        return _cariServisi.CariOlusturAsync(new CariOlusturRequestDto
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
