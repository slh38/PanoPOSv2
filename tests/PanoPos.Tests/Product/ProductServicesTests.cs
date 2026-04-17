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
    private readonly UrunKategoriServisi _urunKategoriServisi;
    private readonly UrunGrupServisi _urunGrupServisi;

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
        _urunKategoriServisi = new UrunKategoriServisi(_dbContext);
        _urunGrupServisi = new UrunGrupServisi(_dbContext);
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
    public async Task Kategori_olusturulur()
    {
        var kategori = await _urunKategoriServisi.OlusturAsync(new UrunKategoriOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Icecek",
            Kod = "ICECEK"
        });

        Assert.Equal("Icecek", kategori.Ad);
        Assert.Equal("ICECEK", kategori.Kod);
    }

    [Fact]
    public async Task Grup_olusturulur()
    {
        var grup = await _urunGrupServisi.OlusturAsync(new UrunGrupOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Hizli Tuketim",
            Kod = "HT"
        });

        Assert.Equal("Hizli Tuketim", grup.Ad);
        Assert.Equal("HT", grup.Kod);
    }

    [Fact]
    public async Task Urun_kategori_ve_grup_ile_kaydedilir()
    {
        var kategori = await _urunKategoriServisi.OlusturAsync(new UrunKategoriOlusturRequestDto { SubeId = 1, Ad = "Icecek", Kod = "ICECEK" });
        var grup = await _urunGrupServisi.OlusturAsync(new UrunGrupOlusturRequestDto { SubeId = 1, Ad = "Soguk", Kod = "SOGUK" });

        var urun = await _urunServisi.UrunOlusturAsync(new UrunOlusturRequestDto
        {
            SubeId = 1,
            UrunKodu = "URN-KTG-001",
            Ad = "Kola",
            UrunTipi = UrunTipi.Mamul,
            UrunKategoriId = kategori.Id,
            UrunGrupId = grup.Id
        });

        Assert.Equal(kategori.Id, urun.UrunKategoriId);
        Assert.Equal("Icecek", urun.UrunKategoriAd);
        Assert.Equal(grup.Id, urun.UrunGrupId);
        Assert.Equal("Soguk", urun.UrunGrupAd);
    }

    [Fact]
    public async Task Urun_listelemede_kategori_ve_grup_gorunur()
    {
        var kategori = await _urunKategoriServisi.OlusturAsync(new UrunKategoriOlusturRequestDto { SubeId = 1, Ad = "Atistirmalik", Kod = "ATS" });
        var grup = await _urunGrupServisi.OlusturAsync(new UrunGrupOlusturRequestDto { SubeId = 1, Ad = "Market", Kod = "MRK" });

        await _urunServisi.UrunOlusturAsync(new UrunOlusturRequestDto
        {
            SubeId = 1,
            UrunKodu = "URN-LIST-001",
            Ad = "Cips",
            UrunTipi = UrunTipi.Mamul,
            UrunKategoriId = kategori.Id,
            UrunGrupId = grup.Id
        });

        var liste = await _urunServisi.UrunListeleAsync("Cips", 1, 10);

        var kayit = Assert.Single(liste.Kayitlar);
        Assert.Equal(kategori.Id, kayit.UrunKategoriId);
        Assert.Equal("Atistirmalik", kayit.UrunKategoriAd);
        Assert.Equal(grup.Id, kayit.UrunGrupId);
        Assert.Equal("Market", kayit.UrunGrupAd);
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
