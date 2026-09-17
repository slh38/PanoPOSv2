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
    private readonly StokKartServisi _stokKartServisi;
    private readonly BarkodServisi _barkodServisi;
    private readonly RenkServisi _renkServisi;
    private readonly BedenServisi _bedenServisi;
    private readonly StokKategoriServisi _stokKategoriServisi;
    private readonly StokGrupServisi _stokGrupServisi;

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

        _stokKartServisi = new StokKartServisi(_dbContext);
        _barkodServisi = new BarkodServisi(_dbContext);
        _renkServisi = new RenkServisi(_dbContext);
        _bedenServisi = new BedenServisi(_dbContext);
        _stokKategoriServisi = new StokKategoriServisi(_dbContext);
        _stokGrupServisi = new StokGrupServisi(_dbContext);
    }

    [Fact]
    public async Task StokKart_olusturulabilir()
    {
        var stokKart = await _stokKartServisi.StokKartOlusturAsync(new StokKartOlusturRequestDto
        {
            KdvId = 1,
            SubeId = 1,
            StokKartKodu = "URN-001",
            Ad = "Kola",
            StokKartTipi = StokKartTipi.Mamul
        });

        Assert.Equal("Kola", stokKart.Ad);
        Assert.Equal("URN-001", stokKart.StokKartKodu);
    }

    [Fact]
    public async Task Kategori_olusturulur()
    {
        var kategori = await _stokKategoriServisi.OlusturAsync(new StokKategoriOlusturRequestDto
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
        var grup = await _stokGrupServisi.OlusturAsync(new StokGrupOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Hizli Tuketim",
            Kod = "HT"
        });

        Assert.Equal("Hizli Tuketim", grup.Ad);
        Assert.Equal("HT", grup.Kod);
    }

    [Fact]
    public async Task StokKart_kategori_ve_grup_ile_kaydedilir()
    {
        var kategori = await _stokKategoriServisi.OlusturAsync(new StokKategoriOlusturRequestDto { SubeId = 1, Ad = "Icecek", Kod = "ICECEK" });
        var grup = await _stokGrupServisi.OlusturAsync(new StokGrupOlusturRequestDto { SubeId = 1, Ad = "Soguk", Kod = "SOGUK" });

        var stokKart = await _stokKartServisi.StokKartOlusturAsync(new StokKartOlusturRequestDto
        {
            KdvId = 1,
            SubeId = 1,
            StokKartKodu = "URN-KTG-001",
            Ad = "Kola",
            StokKartTipi = StokKartTipi.Mamul,
            StokKategoriId = kategori.Id,
            StokGrupId = grup.Id
        });

        Assert.Equal(kategori.Id, stokKart.StokKategoriId);
        Assert.Equal("Icecek", stokKart.StokKategoriAd);
        Assert.Equal(grup.Id, stokKart.StokGrupId);
        Assert.Equal("Soguk", stokKart.StokGrupAd);
    }

    [Fact]
    public async Task StokKart_listelemede_kategori_ve_grup_gorunur()
    {
        var kategori = await _stokKategoriServisi.OlusturAsync(new StokKategoriOlusturRequestDto { SubeId = 1, Ad = "Atistirmalik", Kod = "ATS" });
        var grup = await _stokGrupServisi.OlusturAsync(new StokGrupOlusturRequestDto { SubeId = 1, Ad = "Market", Kod = "MRK" });

        await _stokKartServisi.StokKartOlusturAsync(new StokKartOlusturRequestDto
        {
            KdvId = 1,
            SubeId = 1,
            StokKartKodu = "URN-LIST-001",
            Ad = "Cips",
            StokKartTipi = StokKartTipi.Mamul,
            StokKategoriId = kategori.Id,
            StokGrupId = grup.Id
        });

        var liste = await _stokKartServisi.StokKartListeleAsync("Cips", 1, 10);

        var kayit = Assert.Single(liste.Kayitlar);
        Assert.Equal(kategori.Id, kayit.StokKategoriId);
        Assert.Equal("Atistirmalik", kayit.StokKategoriAd);
        Assert.Equal(grup.Id, kayit.StokGrupId);
        Assert.Equal("Market", kayit.StokGrupAd);
    }

    [Fact]
    public async Task Ayni_barkod_iki_kez_eklenemez()
    {
        var stokKart = await StokKartEkleAsync("URN-002", "Soda");
        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            StokKartId = stokKart.Id,
            BarkodNo = "8690000000011",
            BarkodTipi = BarkodTipi.Ean
        });

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            StokKartId = stokKart.Id,
            BarkodNo = "8690000000011",
            BarkodTipi = BarkodTipi.Ean
        }));

        Assert.Equal("barcode_duplicate", ex.ErrorCode);
    }

    [Fact]
    public async Task Barkod_ile_stok_kart_bulunur()
    {
        var stokKart = await StokKartEkleAsync("URN-003", "Ayran");
        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            StokKartId = stokKart.Id,
            BarkodNo = "8690000000022",
            BarkodTipi = BarkodTipi.Ean
        });

        var barkod = await _barkodServisi.BarkodIleBulAsync("8690000000022");

        Assert.NotNull(barkod);
        Assert.Equal(stokKart.Id, barkod!.StokKartId);
        Assert.Equal("Ayran", barkod.StokKartAd);
    }

    [Fact]
    public async Task Barkod_ile_varyant_bulunur()
    {
        var stokKart = await StokKartEkleAsync("URN-004", "Tisort");
        var renk = await _renkServisi.OlusturAsync(new RenkOlusturRequestDto { SubeId = 1, Ad = "Kirmizi", Kod = "KRMZ" });
        var varyant = await _stokKartServisi.StokKartVaryantOlusturAsync(stokKart.Id, new StokKartVaryantOlusturRequestDto
        {
            RenkId = renk.Id,
            VaryantKodu = "TS-KRMZ",
            BarkodluMu = true
        });

        await _barkodServisi.BarkodOlusturAsync(new BarkodOlusturRequestDto
        {
            StokKartVaryantId = varyant.Id,
            BarkodNo = "8690000000033",
            BarkodTipi = BarkodTipi.Ean
        });

        var barkod = await _barkodServisi.BarkodIleBulAsync("8690000000033");

        Assert.NotNull(barkod);
        Assert.Equal(varyant.Id, barkod!.StokKartVaryantId);
        Assert.Equal("TS-KRMZ", barkod.VaryantKodu);
    }

    [Fact]
    public async Task Ayni_stok_kart_altinda_ayni_varyant_tekrar_eklenemez()
    {
        var stokKart = await StokKartEkleAsync("URN-005", "Gomlek");
        var renk = await _renkServisi.OlusturAsync(new RenkOlusturRequestDto { SubeId = 1, Ad = "Mavi", Kod = "MAVI" });
        var beden = await _bedenServisi.OlusturAsync(new BedenOlusturRequestDto { SubeId = 1, Ad = "L", Kod = "L" });

        await _stokKartServisi.StokKartVaryantOlusturAsync(stokKart.Id, new StokKartVaryantOlusturRequestDto
        {
            RenkId = renk.Id,
            BedenId = beden.Id,
            VaryantKodu = "GMLK-MAVI-L",
            BarkodluMu = true
        });

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _stokKartServisi.StokKartVaryantOlusturAsync(stokKart.Id, new StokKartVaryantOlusturRequestDto
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
        var stokKart = await StokKartEkleAsync("URN-006", "Pantolon");

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _stokKartServisi.StokKartVaryantOlusturAsync(stokKart.Id, new StokKartVaryantOlusturRequestDto
        {
            VaryantKodu = "PNT-NULL",
            BarkodluMu = false
        }));

        Assert.Equal("variant_empty", ex.ErrorCode);
    }

    [Fact]
    public async Task StokKart_listesi_pagination_ile_doner()
    {
        await StokKartEkleAsync("URN-101", "Elma");
        await StokKartEkleAsync("URN-102", "Armut");
        await StokKartEkleAsync("URN-103", "Muz");

        var sayfa = await _stokKartServisi.StokKartListeleAsync(null, 2, 2);

        Assert.Equal(3, sayfa.ToplamKayit);
        Assert.Equal(2, sayfa.Sayfa);
        Assert.Equal(2, sayfa.SayfaBoyutu);
        Assert.Single(sayfa.Kayitlar);
    }

    private Task<StokKartDto> StokKartEkleAsync(string kod, string ad)
    {
        return _stokKartServisi.StokKartOlusturAsync(new StokKartOlusturRequestDto
        {
            KdvId = 1,
            SubeId = 1,
            StokKartKodu = kod,
            Ad = ad,
            StokKartTipi = StokKartTipi.Mamul
        });
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
