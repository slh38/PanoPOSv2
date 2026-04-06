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
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen
        });

        Assert.Equal(SiparisDurumu.Bekliyor, siparis.Durum);
        Assert.StartsWith("SIP-", siparis.SiparisNo);
    }

    [Fact]
    public async Task Masa_siparisinde_adisyon_zorunlu()
    {
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.Masa
        }));

        Assert.Equal("adisyon_required_for_table_order", ex.ErrorCode);
    }

    [Fact]
    public async Task Hizli_satis_siparisinde_adisyon_zorunlu_degildir()
    {
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen
        });

        Assert.Null(siparis.AdisyonId);
    }

    [Fact]
    public async Task Satir_eklenince_toplam_guncellenir()
    {
        var urun = await UrunEkleAsync("Kahve");
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen
        });

        var guncel = await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 75m
        });

        Assert.Equal(150m, guncel.ToplamTutar);
        Assert.Single(guncel.Detaylar);
    }

    [Fact]
    public async Task Siparis_iptal_edilir()
    {
        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen
        });

        var iptal = await _siparisServisi.SiparisIptalAsync(siparis.Id);

        Assert.Equal(SiparisDurumu.Iptal, iptal.Durum);
        Assert.False(iptal.AktifMi);
    }

    [Fact]
    public async Task Listeleme_calisir()
    {
        await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto { SubeId = 1, SiparisTipi = SiparisTipi.HizliSatisBekleyen });
        await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto { SubeId = 1, SiparisTipi = SiparisTipi.HizliSatisBekleyen });
        var masa = await _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto { SubeId = 1, Kod = "M-01", Ad = "Masa 1" });
        var adisyon = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto { MasaId = masa.Id, AcanKullaniciId = 1, AcanCihazId = 1 });
        await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto { SubeId = 1, SiparisTipi = SiparisTipi.Masa, AdisyonId = adisyon.Id });

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
            AdisyonId = adisyon.Id
        });

        Assert.Equal(adisyon.Id, siparis.AdisyonId);
        Assert.Equal(SiparisTipi.Masa, siparis.SiparisTipi);
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
