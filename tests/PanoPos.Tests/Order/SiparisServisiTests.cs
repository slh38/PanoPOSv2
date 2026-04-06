using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Application.Restaurant;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Restaurant;

namespace PanoPos.Tests.Order;

public sealed class SiparisServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly SiparisServisi _siparisServisi;
    private readonly MasaServisi _masaServisi;
    private readonly AdisyonServisi _adisyonServisi;

    public SiparisServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _siparisServisi = new SiparisServisi(_dbContext);
        _masaServisi = new MasaServisi(_dbContext);
        _adisyonServisi = new AdisyonServisi(_dbContext);
    }

    [Fact]
    public async Task Siparis_olusturulur()
    {
        var siparis = await YeniSiparisAsync();

        Assert.Equal(SiparisDurumu.Bekliyor, siparis.Durum);
        Assert.StartsWith("SIP-", siparis.SiparisNo);
    }

    [Fact]
    public async Task Masa_siparisinde_adisyon_zorunlu()
    {
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.Masa,
            ParaBirimKodu = "TRY",
            Kur = 1
        }));

        Assert.Equal("adisyon_required_for_table_order", ex.ErrorCode);
    }

    [Fact]
    public async Task Hizli_satis_siparisinde_adisyon_zorunlu_degildir()
    {
        var siparis = await YeniSiparisAsync();

        Assert.Null(siparis.AdisyonId);
    }

    [Fact]
    public async Task Satir_eklenince_toplam_guncellenir()
    {
        var urun = await UrunEkleAsync("Kahve");
        var siparis = await YeniSiparisAsync();

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 75m
        });

        Assert.Equal(150m, guncel.AraToplam);
        Assert.Equal(150m, guncel.NetToplam);
        Assert.Equal(150m, guncel.ToplamTutar);
        Assert.Single(guncel.Detaylar);
    }

    [Fact]
    public async Task Siparis_iptal_edilir()
    {
        var siparis = await YeniSiparisAsync();

        var iptal = await _siparisServisi.SiparisIptalAsync(siparis.Id);

        Assert.Equal(SiparisDurumu.Iptal, iptal.Durum);
        Assert.False(iptal.AktifMi);
    }

    [Fact]
    public async Task Listeleme_calisir()
    {
        await YeniSiparisAsync();
        await YeniSiparisAsync();
        var masa = await _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto { SubeId = 1, Kod = "M-01", Ad = "Masa 1" });
        var adisyon = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto { MasaId = masa.Id, AcanKullaniciId = 1, AcanCihazId = 1 });
        await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto { SubeId = 1, SiparisTipi = SiparisTipi.Masa, AdisyonId = adisyon.Id, ParaBirimKodu = "TRY", Kur = 1 });

        var liste = await _siparisServisi.SiparisListeleAsync(1, (int)SiparisDurumu.Bekliyor, 1, 2);

        Assert.Equal(3, liste.ToplamKayit);
        Assert.Equal(2, liste.Kayitlar.Count);
    }

    [Fact]
    public async Task Adisyona_bagli_siparis_acilir()
    {
        var masa = await _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto { SubeId = 1, Kod = "M-02", Ad = "Masa 2" });
        var adisyon = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto { MasaId = masa.Id, AcanKullaniciId = 1, AcanCihazId = 1 });

        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.Masa,
            AdisyonId = adisyon.Id,
            ParaBirimKodu = "TRY",
            Kur = 1
        });

        Assert.Equal(adisyon.Id, siparis.AdisyonId);
        Assert.Equal(SiparisTipi.Masa, siparis.SiparisTipi);
    }

    [Fact]
    public async Task Satir_indirimi_oranla_hesaplanir()
    {
        var urun = await UrunEkleAsync("Pasta");
        var siparis = await YeniSiparisAsync();

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 50m,
            IndirimOrani = 10
        });

        Assert.Equal(100m, guncel.Detaylar[0].SatirAraToplam);
        Assert.Equal(10m, guncel.Detaylar[0].IndirimTutari);
        Assert.Equal(90m, guncel.Detaylar[0].SatirNetToplam);
    }

    [Fact]
    public async Task Satir_indirimi_tutarla_hesaplanir()
    {
        var urun = await UrunEkleAsync("Cheesecake");
        var siparis = await YeniSiparisAsync();

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 50m,
            IndirimTutari = 15m
        });

        Assert.Equal(15m, guncel.Detaylar[0].IndirimTutari);
        Assert.Equal(85m, guncel.Detaylar[0].SatirNetToplam);
    }

    [Fact]
    public async Task Ayni_satirda_oran_ve_tutar_hata_verir()
    {
        var urun = await UrunEkleAsync("Cookie");
        var siparis = await YeniSiparisAsync();

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 1,
            BirimFiyat = 40m,
            IndirimOrani = 10m,
            IndirimTutari = 5m
        }));

        Assert.Equal("line_discount_conflict", ex.ErrorCode);
    }

    [Fact]
    public async Task Siparis_genel_indirim_orani_calisir()
    {
        var urun = await UrunEkleAsync("Filtre Kahve");
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "TRY",
            Kur = 1,
            GenelIndirimOrani = 10m
        });

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 50m
        });

        Assert.Equal(100m, guncel.AraToplam);
        Assert.Equal(10m, guncel.GenelIndirimTutari);
        Assert.Equal(90m, guncel.NetToplam);
    }

    [Fact]
    public async Task Siparis_genel_indirim_tutari_calisir()
    {
        var urun = await UrunEkleAsync("Mocha");
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "TRY",
            Kur = 1,
            GenelIndirimTutari = 20m
        });

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 50m
        });

        Assert.Equal(20m, guncel.GenelIndirimTutari);
        Assert.Equal(80m, guncel.NetToplam);
    }

    [Fact]
    public async Task Ayni_sipariste_oran_ve_tutar_hata_verir()
    {
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "TRY",
            Kur = 1,
            GenelIndirimOrani = 5m,
            GenelIndirimTutari = 10m
        }));

        Assert.Equal("order_discount_conflict", ex.ErrorCode);
    }

    [Fact]
    public async Task Para_birimi_ve_kur_kaydedilir()
    {
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "USD",
            Kur = 38.25m
        });

        Assert.Equal("USD", siparis.ParaBirimKodu);
        Assert.Equal(38.25m, siparis.Kur);
    }

    [Fact]
    public async Task NetToplam_dogru_hesaplanir()
    {
        var urun1 = await UrunEkleAsync("Espresso");
        var urun2 = await UrunEkleAsync("Sandvic");
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "TRY",
            Kur = 1,
            GenelIndirimTutari = 5m
        });

        await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun1.Id,
            Miktar = 2,
            BirimFiyat = 30m,
            IndirimTutari = 10m
        });

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun2.Id,
            Miktar = 1,
            BirimFiyat = 40m
        });

        Assert.Equal(100m, guncel.AraToplam);
        Assert.Equal(5m, guncel.GenelIndirimTutari);
        Assert.Equal(85m, guncel.NetToplam);
        Assert.Equal(85m, guncel.ToplamTutar);
    }

    private Task<SiparisDto> YeniSiparisAsync()
    {
        return _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = "TRY",
            Kur = 1
        });
    }

    private async Task<Urun> UrunEkleAsync(string ad)
    {
        var urun = new Urun
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = ad,
            UrunTipi = UrunTipi.Mamul,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Urunler.Add(urun);
        await _dbContext.SaveChangesAsync();
        return urun;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
