using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Cash;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Cash;

public sealed class VardiyaServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly VardiyaServisi _vardiyaServisi;

    public VardiyaServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _vardiyaServisi = new VardiyaServisi(_dbContext);
    }

    [Fact]
    public async Task Vardiya_acilabilir()
    {
        var kasa = await HazirKasaVeOturumAsync();

        var sonuc = await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);

        Assert.True(sonuc.AktifMi);
        Assert.Equal(100m, sonuc.AcilisNakit);
    }

    [Fact]
    public async Task Ayni_cihazda_ikinci_vardiya_acilamaz()
    {
        var kasa1 = await HazirKasaVeOturumAsync();
        var kasa2 = await KasaEkleAsync("Kasa 2");
        await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa1.Id, 100m);

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _vardiyaServisi.VardiyaAcAsync(1, 1, kasa2.Id, 50m));

        Assert.Equal("aktif_vardiya_device_exists", ex.ErrorCode);
    }

    [Fact]
    public async Task Ayni_kasada_ikinci_vardiya_acilamaz()
    {
        var kasa = await HazirKasaVeOturumAsync();
        await CihazEkleAsync(2, "CIHAZ-002");
        await AktifOturumEkleAsync(1, 2);
        await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _vardiyaServisi.VardiyaAcAsync(1, 2, kasa.Id, 50m));

        Assert.Equal("aktif_vardiya_cash_exists", ex.ErrorCode);
    }

    [Fact]
    public async Task Vardiya_kapanabilir()
    {
        var kasa = await HazirKasaVeOturumAsync();
        var vardiya = await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);

        var sonuc = await _vardiyaServisi.VardiyaKapatAsync(vardiya.VardiyaId, 100m, "Kapanis deneme");

        Assert.Equal(vardiya.VardiyaId, sonuc.VardiyaId);
        Assert.Equal(0m, sonuc.FarkTutar);
    }

    [Fact]
    public async Task Beklenen_nakit_dogru_hesaplanir()
    {
        var kasa = await HazirKasaVeOturumAsync();
        var vardiya = await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);
        await HareketEkleAsync(vardiya.VardiyaId, kasa.Id, KasaIslemTipi.NakitGiris, 50m);
        await HareketEkleAsync(vardiya.VardiyaId, kasa.Id, KasaIslemTipi.Masraf, 20m);

        var sonuc = await _vardiyaServisi.VardiyaKapatAsync(vardiya.VardiyaId, 130m, null);

        Assert.Equal(130m, sonuc.BeklenenNakit);
        Assert.Equal(0m, sonuc.FarkTutar);
    }

    [Fact]
    public async Task Kasa_hareket_kaydi_olusur()
    {
        var kasa = await HazirKasaVeOturumAsync();
        var vardiya = await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);

        var acilisHareketi = await _dbContext.KasaHareketleri.SingleAsync(x => x.VardiyaId == vardiya.VardiyaId && x.IslemTipi == KasaIslemTipi.Acilis);
        Assert.Equal(100m, acilisHareketi.Tutar);

        await _vardiyaServisi.VardiyaKapatAsync(vardiya.VardiyaId, 100m, null);

        var kapanisHareketi = await _dbContext.KasaHareketleri.SingleAsync(x => x.VardiyaId == vardiya.VardiyaId && x.IslemTipi == KasaIslemTipi.VardiyaKapanis);
        Assert.Equal(100m, kapanisHareketi.Tutar);
    }

    [Fact]
    public async Task Aktif_vardiya_dogru_doner()
    {
        var kasa = await HazirKasaVeOturumAsync();
        var vardiya = await _vardiyaServisi.VardiyaAcAsync(1, 1, kasa.Id, 100m);

        var aktif = await _vardiyaServisi.AktifVardiyaGetirAsync(1);

        Assert.NotNull(aktif);
        Assert.Equal(vardiya.VardiyaId, aktif!.VardiyaId);
    }

    private async Task<Kasa> HazirKasaVeOturumAsync()
    {
        var kasa = await KasaEkleAsync("Ana Kasa");
        var cihaz = await _dbContext.Cihazlar.SingleAsync(x => x.Id == 1);
        cihaz.VarsayilanKasaId = kasa.Id;
        await _dbContext.SaveChangesAsync();
        await AktifOturumEkleAsync(1, 1);
        return kasa;
    }

    private async Task<Kasa> KasaEkleAsync(string ad)
    {
        var kasa = new Kasa
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = ad,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Kasalar.Add(kasa);
        await _dbContext.SaveChangesAsync();
        return kasa;
    }

    private async Task CihazEkleAsync(long id, string kod)
    {
        var cihaz = new Cihaz
        {
            Id = id,
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = kod,
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Cihazlar.Add(cihaz);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AktifOturumEkleAsync(long kullaniciId, long cihazId)
    {
        var mevcut = await _dbContext.KullaniciOturumlari
            .Where(x => x.KullaniciId == kullaniciId && x.CihazId == cihazId && x.AktifMi)
            .ToListAsync();

        if (mevcut.Count > 0)
        {
            return;
        }

        _dbContext.KullaniciOturumlari.Add(new KullaniciOturum
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            KullaniciId = kullaniciId,
            CihazId = cihazId,
            GirisTarihi = DateTime.UtcNow,
            AktifMi = true,
            SilindiMi = false
        });

        await _dbContext.SaveChangesAsync();
    }

    private async Task HareketEkleAsync(long vardiyaId, long kasaId, KasaIslemTipi islemTipi, decimal tutar)
    {
        _dbContext.KasaHareketleri.Add(new KasaHareket
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            KasaId = kasaId,
            VardiyaId = vardiyaId,
            KullaniciId = 1,
            CihazId = 1,
            IslemTipi = islemTipi,
            Tutar = tutar,
            Tarih = DateTime.UtcNow,
            AktifMi = true,
            SilindiMi = false
        });

        await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
