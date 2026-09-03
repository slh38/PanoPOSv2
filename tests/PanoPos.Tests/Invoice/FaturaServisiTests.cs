using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Order;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Invoice;

public sealed class FaturaServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly SiparisServisi _siparisServisi;
    private readonly FaturaServisi _faturaServisi;

    public FaturaServisiTests()
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
        _faturaServisi = new FaturaServisi(_dbContext);
    }

    [Fact]
    public async Task Siparisten_fatura_olusturulur()
    {
        var siparis = await HazirSiparisAsync();

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto
        {
            SiparisId = siparis.Id
        });

        Assert.Equal(siparis.Id, fatura.SiparisId);
        Assert.Equal(FaturaDurumu.Acik, fatura.Durum);
        Assert.Equal(0m, fatura.OdenenTutar);
        Assert.Equal(fatura.NetToplam, fatura.KalanTutar);
    }

    [Fact]
    public async Task Detaylar_snapshot_olarak_gelir()
    {
        var siparis = await HazirSiparisAsync();

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Single(fatura.Detaylar);
        Assert.Equal("Latte", fatura.Detaylar[0].StokKartAd);
        Assert.Equal(120m, fatura.Detaylar[0].SatirToplam);
    }

    [Fact]
    public async Task Siparisteki_para_birimi_faturaya_kopyalanir()
    {
        var siparis = await HazirSiparisAsync(paraBirimKodu: "USD", kur: 38.25m);

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Equal("USD", fatura.ParaBirimKodu);
    }

    [Fact]
    public async Task Siparisteki_kur_faturaya_kopyalanir()
    {
        var siparis = await HazirSiparisAsync(paraBirimKodu: "EUR", kur: 41.75m);

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Equal(41.75m, fatura.Kur);
    }

    [Fact]
    public async Task Satir_indirimleri_faturaya_snapshot_gelir()
    {
        var siparis = await HazirSiparisAsync(satirIndirimOrani: 10m);

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        var detay = Assert.Single(fatura.Detaylar);
        Assert.Equal(120m, detay.SatirAraToplam);
        Assert.Equal(10m, detay.IndirimOrani);
        Assert.Equal(12m, detay.IndirimTutari);
        Assert.Equal(108m, detay.SatirNetToplam);
    }

    [Fact]
    public async Task Genel_indirim_faturaya_snapshot_gelir()
    {
        var siparis = await HazirSiparisAsync(genelIndirimOrani: 5m);

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Equal(120m, fatura.AraToplam);
        Assert.Equal(5m, fatura.GenelIndirimOrani);
        Assert.Equal(6m, fatura.GenelIndirimTutari);
    }

    [Fact]
    public async Task NetToplam_dogru_kopyalanir()
    {
        var siparis = await HazirSiparisAsync(satirIndirimOrani: 10m, genelIndirimOrani: 5m);

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Equal(102.60m, fatura.NetToplam);
        Assert.Equal(fatura.NetToplam, fatura.ToplamTutar);
    }

    [Fact]
    public async Task Siparis_durumu_guncellenir()
    {
        var siparis = await HazirSiparisAsync();

        await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        var guncelSiparis = await _dbContext.Siparisler.SingleAsync(x => x.Id == siparis.Id);
        Assert.Equal(SiparisDurumu.Tamamlandi, guncelSiparis.Durum);
    }

    [Fact]
    public async Task Fatura_kapatilir()
    {
        var siparis = await HazirSiparisAsync();
        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        var kapali = await _faturaServisi.FaturaKapatAsync(fatura.Id, new FaturaKapatRequestDto { KapatanKullaniciId = 1 });

        Assert.Equal(FaturaDurumu.Kapali, kapali.Durum);
        Assert.NotNull(kapali.KapanisTarihi);
        Assert.Equal(1, kapali.KapatanKullaniciId);
    }

    [Fact]
    public async Task Fatura_iptal_edilir()
    {
        var siparis = await HazirSiparisAsync();
        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        var iptal = await _faturaServisi.FaturaIptalAsync(fatura.Id, new FaturaIptalRequestDto { Aciklama = "Iptal" });

        Assert.Equal(FaturaDurumu.Iptal, iptal.Durum);
        Assert.False(iptal.AktifMi);
    }

    [Fact]
    public async Task Fatura_listelemede_yeni_alanlar_doner()
    {
        var siparis = await HazirSiparisAsync(paraBirimKodu: "USD", kur: 38.25m, satirIndirimOrani: 10m, genelIndirimOrani: 5m);
        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        var liste = await _faturaServisi.FaturaListeleAsync(1, (int)FaturaDurumu.Acik, 1, 10);

        var kayit = Assert.Single(liste.Kayitlar);
        Assert.Equal(fatura.Id, kayit.Id);
        Assert.Equal("USD", kayit.ParaBirimKodu);
        Assert.Equal(38.25m, kayit.Kur);
        Assert.Equal(120m, kayit.AraToplam);
        Assert.Equal(5.40m, kayit.GenelIndirimTutari);
        Assert.Equal(102.60m, kayit.NetToplam);
        Assert.Equal(0m, kayit.OdenenTutar);
        Assert.Equal(102.60m, kayit.KalanTutar);
    }

    [Fact]
    public async Task Sayfali_liste_calisir()
    {
        for (var i = 0; i < 3; i++)
        {
            var siparis = await HazirSiparisAsync($"Latte-{i}");
            await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });
        }

        var liste = await _faturaServisi.FaturaListeleAsync(1, (int)FaturaDurumu.Acik, 1, 2);

        Assert.Equal(3, liste.ToplamKayit);
        Assert.Equal(2, liste.Kayitlar.Count);
    }

    private async Task<Siparis> HazirSiparisAsync(
        string urunAd = "Latte",
        string paraBirimKodu = "TRY",
        decimal kur = 1m,
        decimal? satirIndirimOrani = null,
        decimal? satirIndirimTutari = null,
        decimal? genelIndirimOrani = null,
        decimal? genelIndirimTutari = null)
    {
        var urun = new StokKart
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = urunAd,
            StokKartTipi = StokKartTipi.Mamul,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKartler.Add(urun);
        await _dbContext.SaveChangesAsync();

        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen,
            ParaBirimKodu = paraBirimKodu,
            Kur = kur,
            GenelIndirimOrani = genelIndirimOrani,
            GenelIndirimTutari = genelIndirimTutari
        });

        await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            StokKartId = urun.Id,
            Miktar = 2,
            BirimFiyat = 60m,
            IndirimOrani = satirIndirimOrani,
            IndirimTutari = satirIndirimTutari
        });

        return await _dbContext.Siparisler.SingleAsync(x => x.Id == siparis.Id);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}


