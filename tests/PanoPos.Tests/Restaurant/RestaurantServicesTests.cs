using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Restaurant;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Restaurant;

namespace PanoPos.Tests.Restaurant;

public sealed class RestaurantServicesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly MasaServisi _masaServisi;
    private readonly MasaGrupServisi _masaGrupServisi;
    private readonly AdisyonServisi _adisyonServisi;

    public RestaurantServicesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _masaServisi = new MasaServisi(_dbContext);
        _masaGrupServisi = new MasaGrupServisi(_dbContext);
        _adisyonServisi = new AdisyonServisi(_dbContext);
    }

    [Fact]
    public async Task Masa_olusturulur()
    {
        var masa = await _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto
        {
            SubeId = 1,
            Kod = "M-01",
            Ad = "Masa 1"
        });

        Assert.Equal("Masa 1", masa.Ad);
        Assert.Equal(SystemSeedData.MasaDurumBosId, masa.MasaDurumId);
    }

    [Fact]
    public async Task Masa_grubu_olusturulur()
    {
        var grup = await _masaGrupServisi.OlusturAsync(new MasaGrupOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Salon",
            Kod = "SALON"
        });

        Assert.Equal("Salon", grup.Ad);
        Assert.Equal("SALON", grup.Kod);
    }

    [Fact]
    public async Task Masa_grup_ile_kaydedilir()
    {
        var grup = await _masaGrupServisi.OlusturAsync(new MasaGrupOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Bahce",
            Kod = "BAHCE"
        });

        var masa = await _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto
        {
            SubeId = 1,
            Kod = "M-B1",
            Ad = "Bahce Masa",
            MasaGrupId = grup.Id,
            Kapasite = 6
        });

        Assert.Equal(grup.Id, masa.MasaGrupId);
        Assert.Equal("Bahce", masa.MasaGrupAdi);
        Assert.Equal(6, masa.Kapasite);
    }

    [Fact]
    public async Task Adisyon_acilir()
    {
        var masa = await MasaOlusturAsync();

        var adisyon = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1
        });

        Assert.Equal(masa.Id, adisyon.MasaId);
        Assert.Equal(AdisyonDurumu.Acik, adisyon.Durum);
    }

    [Fact]
    public async Task Adisyon_acilirken_kisi_sayisi_kaydedilir()
    {
        var masa = await MasaOlusturAsync();

        var adisyon = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1,
            KisiSayisi = 4
        });

        Assert.Equal(4, adisyon.KisiSayisi);
    }

    [Fact]
    public async Task Negatif_veya_sifir_kisi_sayisi_hata_verir()
    {
        var masa = await MasaOlusturAsync();

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1,
            KisiSayisi = 0
        }));

        Assert.Equal("adisyon_guest_count_invalid", ex.ErrorCode);
    }

    [Fact]
    public async Task Ayni_masada_ikinci_acik_adisyon_engellenir()
    {
        var masa = await MasaOlusturAsync();
        await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1
        });

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1
        }));

        Assert.Equal("masa_open_check_exists", ex.ErrorCode);
    }

    [Fact]
    public async Task Kapatinca_masa_durumu_degisir()
    {
        var masa = await MasaOlusturAsync();
        var acik = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1
        });

        var masaDolu = await _dbContext.Masalar.AsNoTracking().SingleAsync(x => x.Id == masa.Id);
        Assert.Equal(SystemSeedData.MasaDurumDoluId, masaDolu.MasaDurumId);

        await _adisyonServisi.AdisyonKapatAsync(new AdisyonKapatRequestDto { AdisyonId = acik.Id });

        var masaBos = await _dbContext.Masalar.AsNoTracking().SingleAsync(x => x.Id == masa.Id);
        Assert.Equal(SystemSeedData.MasaDurumBosId, masaBos.MasaDurumId);
    }

    [Fact]
    public async Task Acik_adisyon_getirilebilir()
    {
        var masa = await MasaOlusturAsync();
        var acik = await _adisyonServisi.AdisyonAcAsync(new AdisyonAcRequestDto
        {
            MasaId = masa.Id,
            AcanKullaniciId = 1,
            AcanCihazId = 1,
            KisiSayisi = 3
        });

        var bulunan = await _adisyonServisi.AcikAdisyonGetirAsync(masa.Id);

        Assert.NotNull(bulunan);
        Assert.Equal(acik.Id, bulunan!.Id);
        Assert.Equal(3, bulunan.KisiSayisi);
    }

    private Task<MasaDto> MasaOlusturAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return _masaServisi.MasaOlusturAsync(new MasaOlusturRequestDto
        {
            SubeId = 1,
            Kod = $"M-{suffix}",
            Ad = $"Masa-{suffix}"
        });
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
