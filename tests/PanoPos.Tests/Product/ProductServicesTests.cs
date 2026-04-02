using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Product;

namespace PanoPos.Tests.Product;

public sealed class ProductServicesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly UrunServisi _urunServisi;
    private readonly BarkodServisi _barkodServisi;
    private readonly RenkServisi _renkServisi;
    private readonly BedenServisi _bedenServisi;

    public ProductServicesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _urunServisi = new UrunServisi(_dbContext);
        _barkodServisi = new BarkodServisi(_dbContext);
        _renkServisi = new RenkServisi(_dbContext);
        _bedenServisi = new BedenServisi(_dbContext);
    }

    [Fact]
    public async Task Urun_olusturulabilir()
    {
        var urun = await _urunServisi.UrunOlusturAsync(new UrunOlusturRequestDto
        {
            SubeId = 1,
            UrunKodu = "URN-001",
            Ad = "Kola",
            UrunTipi = UrunTipi.Mamul
        });

        Assert.Equal("Kola", urun.Ad);
        Assert.Equal("URN-001", urun.UrunKodu);
    }

    [Fact]
    public async Task Ayni_barkod_iki_kez_eklenemez()
    {
        var urun = await UrunEkleAsync("URN-002", "Soda");
        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            UrunId = urun.Id,
            BarkodNo = "8690000000011",
            BarkodTipi = BarkodTipi.Ean
        });

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            UrunId = urun.Id,
            BarkodNo = "8690000000011",
            BarkodTipi = BarkodTipi.Ean
        }));

        Assert.Equal("barcode_duplicate", ex.ErrorCode);
    }

    [Fact]
    public async Task Barkod_ile_urun_bulunur()
    {
        var urun = await UrunEkleAsync("URN-003", "Ayran");
        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            UrunId = urun.Id,
            BarkodNo = "8690000000022",
            BarkodTipi = BarkodTipi.Ean
        });

        var barkod = await _barkodServisi.BarkodIleBulAsync("8690000000022");

        Assert.NotNull(barkod);
        Assert.Equal(urun.Id, barkod!.UrunId);
        Assert.Equal("Ayran", barkod.UrunAd);
    }

    [Fact]
    public async Task Barkod_ile_varyant_bulunur()
    {
        var urun = await UrunEkleAsync("URN-004", "Tisort");
        var renk = await _renkServisi.OlusturAsync(new RenkOlusturRequestDto { SubeId = 1, Ad = "Kirmizi", Kod = "KRMZ" });
        var varyant = await _urunServisi.UrunVaryantOlusturAsync(urun.Id, new UrunVaryantOlusturRequestDto
        {
            RenkId = renk.Id,
            VaryantKodu = "TS-KRMZ",
            BarkodluMu = true
        });

        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            UrunVaryantId = varyant.Id,
            BarkodNo = "8690000000033",
            BarkodTipi = BarkodTipi.Ean
        });

        var barkod = await _barkodServisi.BarkodIleBulAsync("8690000000033");

        Assert.NotNull(barkod);
        Assert.Equal(varyant.Id, barkod!.UrunVaryantId);
        Assert.Equal("TS-KRMZ", barkod.VaryantKodu);
    }

    [Fact]
    public async Task Ayni_urun_altinda_ayni_varyant_tekrar_eklenemez()
    {
        var urun = await UrunEkleAsync("URN-005", "Gomlek");
        var renk = await _renkServisi.OlusturAsync(new RenkOlusturRequestDto { SubeId = 1, Ad = "Mavi", Kod = "MAVI" });
        var beden = await _bedenServisi.OlusturAsync(new BedenOlusturRequestDto { SubeId = 1, Ad = "L", Kod = "L" });

        await _urunServisi.UrunVaryantOlusturAsync(urun.Id, new UrunVaryantOlusturRequestDto
        {
            RenkId = renk.Id,
            BedenId = beden.Id,
            VaryantKodu = "GMLK-MAVI-L",
            BarkodluMu = true
        });

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _urunServisi.UrunVaryantOlusturAsync(urun.Id, new UrunVaryantOlusturRequestDto
        {
            RenkId = renk.Id,
            BedenId = beden.Id,
            VaryantKodu = "GMLK-MAVI-L-2",
            BarkodluMu = true
        }));

        Assert.Equal("variant_duplicate", ex.ErrorCode);
    }

    [Fact]
    public async Task Varyantta_renk_ve_beden_ikisi_bos_olamaz()
    {
        var urun = await UrunEkleAsync("URN-006", "Pantolon");

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _urunServisi.UrunVaryantOlusturAsync(urun.Id, new UrunVaryantOlusturRequestDto
        {
            VaryantKodu = "PNT-NULL",
            BarkodluMu = false
        }));

        Assert.Equal("variant_empty", ex.ErrorCode);
    }

    [Fact]
    public async Task Urun_listesi_pagination_ile_doner()
    {
        await UrunEkleAsync("URN-101", "Elma");
        await UrunEkleAsync("URN-102", "Armut");
        await UrunEkleAsync("URN-103", "Muz");

        var sayfa = await _urunServisi.UrunListeleAsync(null, 2, 2);

        Assert.Equal(3, sayfa.ToplamKayit);
        Assert.Equal(2, sayfa.Sayfa);
        Assert.Equal(2, sayfa.SayfaBoyutu);
        Assert.Single(sayfa.Kayitlar);
    }

    private Task<UrunDto> UrunEkleAsync(string kod, string ad)
    {
        return _urunServisi.UrunOlusturAsync(new UrunOlusturRequestDto
        {
            SubeId = 1,
            UrunKodu = kod,
            Ad = ad,
            UrunTipi = UrunTipi.Mamul
        });
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
