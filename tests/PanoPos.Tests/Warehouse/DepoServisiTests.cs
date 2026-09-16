using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Warehouse;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Warehouse;

namespace PanoPos.Tests.Warehouse;

public sealed class DepoServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly DepoServisi _depoServisi;

    public DepoServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _dbContext = new PanoPosDbContext(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(_connection).Options);
        _dbContext.Database.EnsureCreated();
        _depoServisi = new DepoServisi(_dbContext);
    }

    [Fact]
    public async Task Seed_subesi_icin_varsayilan_merkez_depo_olusturulur()
    {
        var depo = await _depoServisi.GetByIdAsync(1, 1);
        Assert.Equal("MERKEZ", depo.DepoKodu);
        Assert.True(depo.VarsayilanMi);
    }

    [Fact]
    public async Task Ensure_default_iki_kez_cagrildiginda_duplicate_uretmez()
    {
        await _depoServisi.EnsureDefaultDepoAsync(SystemSeedData.TenantGuid, 1);
        await _depoServisi.EnsureDefaultDepoAsync(SystemSeedData.TenantGuid, 1);
        Assert.Single(await _dbContext.Depolar.Where(x => x.SubeId == 1 && x.VarsayilanMi).ToListAsync());
    }

    [Fact]
    public async Task Ayni_sube_ve_kod_icin_duplicate_engellenir()
    {
        await CreateAsync("A-01", "Ana Depo");
        var exception = await Assert.ThrowsAsync<UygulamaHatasi>(() => CreateAsync(" a-01 ", "Ikinci Depo"));
        Assert.Equal("depo_duplicate", exception.ErrorCode);
    }

    [Fact]
    public async Task Depo_kodu_uppercase_normalize_edilir()
    {
        var depo = await CreateAsync("  raf-1  ", "Raf Deposu");
        Assert.Equal("RAF-1", depo.DepoKodu);
    }

    [Fact]
    public async Task Ayni_kod_farkli_subede_kullanilabilir()
    {
        var ikinciSubeId = await AddSubeAsync(SystemSeedData.TenantGuid, "SUBE-2");
        await CreateAsync("ORTAK", "Merkez Depo");
        var depo = await _depoServisi.CreateAsync(new DepoKaydetRequestDto { SubeId = ikinciSubeId, DepoKodu = "ortak", Ad = "Ikinci Sube Deposu" });
        Assert.Equal(ikinciSubeId, depo.SubeId);
    }

    [Fact]
    public async Task Yeni_varsayilan_eski_varsayilani_kaldirir()
    {
        var depo = await CreateAsync("YENI", "Yeni Depo");
        var varsayilan = await _depoServisi.SetDefaultAsync(depo.Id, 1);
        Assert.True(varsayilan.VarsayilanMi);
        Assert.False((await _depoServisi.GetByIdAsync(1, 1)).VarsayilanMi);
    }

    [Fact]
    public async Task Varsayilan_depo_silinemez()
    {
        var exception = await Assert.ThrowsAsync<UygulamaHatasi>(() => _depoServisi.DeleteAsync(1, 1));
        Assert.Equal("default_depo_cannot_be_deleted", exception.ErrorCode);
    }

    [Fact]
    public async Task Normal_depo_soft_delete_edilir_ve_listelenmez()
    {
        var depo = await CreateAsync("SIL", "Silinecek Depo");
        await _depoServisi.DeleteAsync(depo.Id, 1);
        Assert.True(await _dbContext.Depolar.IgnoreQueryFilters().Where(x => x.Id == depo.Id).Select(x => x.SilindiMi).SingleAsync());
        var page = await _depoServisi.GetPagedAsync(1, null, null, 1, 20);
        Assert.DoesNotContain(page.Kayitlar, x => x.Id == depo.Id);
    }

    [Fact]
    public async Task Pasif_depo_varsayilan_yapilamaz()
    {
        var depo = await CreateAsync("PASIF", "Pasif Depo");
        await _depoServisi.UpdateAsync(depo.Id, new DepoKaydetRequestDto { SubeId = 1, DepoKodu = depo.DepoKodu, Ad = depo.Ad, AktifMi = false });
        var exception = await Assert.ThrowsAsync<UygulamaHatasi>(() => _depoServisi.SetDefaultAsync(depo.Id, 1));
        Assert.Equal("inactive_depo_cannot_be_default", exception.ErrorCode);
    }

    [Fact]
    public async Task Tenant_izolasyonu_uygulanir()
    {
        var tenantId = Guid.NewGuid();
        var ikinciSubeId = await AddSubeAsync(tenantId, "TENANT-2");
        await CreateAsync("A-01", "Ilk Tenant Deposu");
        await _depoServisi.CreateAsync(new DepoKaydetRequestDto { SubeId = ikinciSubeId, DepoKodu = "A-01", Ad = "Ikinci Tenant Deposu" });
        var page = await _depoServisi.GetPagedAsync(1, null, null, 1, 20);
        Assert.DoesNotContain(page.Kayitlar, x => x.SubeId == ikinciSubeId);
    }

    [Fact]
    public async Task Liste_pagination_ve_arama_uygular()
    {
        await CreateAsync("Z-01", "Zeta Deposu");
        await CreateAsync("A-01", "Alfa Deposu");
        var search = await _depoServisi.GetPagedAsync(1, "Alfa", true, 1, 10);
        var page = await _depoServisi.GetPagedAsync(1, null, true, 2, 2);
        Assert.Single(search.Kayitlar);
        Assert.Equal("Alfa Deposu", search.Kayitlar[0].Ad);
        Assert.Equal(3, page.ToplamKayit);
        Assert.Single(page.Kayitlar);
    }

    private Task<DepoDto> CreateAsync(string kod, string ad)
    {
        return _depoServisi.CreateAsync(new DepoKaydetRequestDto { SubeId = 1, DepoKodu = kod, Ad = ad });
    }

    private async Task<long> AddSubeAsync(Guid tenantId, string kod)
    {
        if (!await _dbContext.Tenantler.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId))
        {
            _dbContext.Tenantler.Add(new Tenant { TenantId = tenantId, SubeId = 1, Ad = $"Tenant {kod}", Kod = $"T-{kod}" });
            await _dbContext.SaveChangesAsync();
        }

        var sube = new Sube { TenantId = tenantId, SubeId = 1, Ad = $"Sube {kod}", Kod = kod };
        _dbContext.Subeler.Add(sube);
        await _dbContext.SaveChangesAsync();
        return sube.Id;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
