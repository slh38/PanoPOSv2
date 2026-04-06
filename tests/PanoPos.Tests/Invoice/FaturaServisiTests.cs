using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
    }

    [Fact]
    public async Task Detaylar_snapshot_olarak_gelir()
    {
        var siparis = await HazirSiparisAsync();

        var fatura = await _faturaServisi.SiparistenFaturaOlusturAsync(new SiparistenFaturaOlusturRequestDto { SiparisId = siparis.Id });

        Assert.Single(fatura.Detaylar);
        Assert.Equal("Latte", fatura.Detaylar[0].UrunAd);
        Assert.Equal(120m, fatura.Detaylar[0].SatirToplam);
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

    private async Task<Siparis> HazirSiparisAsync(string urunAd = "Latte")
    {
        var urun = new Urun
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = urunAd,
            UrunTipi = UrunTipi.Mamul,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Urunler.Add(urun);
        await _dbContext.SaveChangesAsync();

        var siparis = await _siparisServisi.SiparisOlusturAsync(new SiparisOlusturRequestDto
        {
            SubeId = 1,
            SiparisTipi = SiparisTipi.HizliSatisBekleyen
        });

        await _siparisServisi.SiparisSatirEkleAsync(siparis.Id, new SiparisSatirEkleRequestDto
        {
            UrunId = urun.Id,
            Miktar = 2,
            BirimFiyat = 60m
        });

        return await _dbContext.Siparisler.SingleAsync(x => x.Id == siparis.Id);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
