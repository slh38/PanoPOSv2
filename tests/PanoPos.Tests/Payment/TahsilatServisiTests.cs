using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Payment;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Payment;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Payment;

public sealed class TahsilatServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly TahsilatServisi _tahsilatServisi;
    private readonly BankaServisi _bankaServisi;

    public TahsilatServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _tahsilatServisi = new TahsilatServisi(_dbContext);
        _bankaServisi = new BankaServisi(_dbContext);
    }

    [Fact]
    public async Task Nakit_tahsilat_basarili_ve_kasa_hareketi_olusur()
    {
        var kasa = await KasaEkleAsync();
        var fatura = await FaturaEkleAsync();

        var tahsilat = await _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
        {
            SubeId = 1,
            FaturaId = fatura.Id,
            OdemeTipi = OdemeTipi.Nakit,
            KasaId = kasa.Id,
            KullaniciId = 1,
            CihazId = 1,
            Tutar = 150m,
            ParaBirimKodu = "TRY",
            Kur = 1m,
            Aciklama = "Nakit tahsilat"
        });

        Assert.Equal(OdemeTipi.Nakit, tahsilat.OdemeTipi);
        Assert.Equal(150m, tahsilat.YerelTutar);

        var hareket = await _dbContext.KasaHareketleri.SingleAsync(x => x.ReferansId == tahsilat.Id);
        Assert.Equal(KasaIslemTipi.SatisTahsilat, hareket.IslemTipi);
        Assert.Equal(150m, hareket.Tutar);

        var guncelFatura = await _dbContext.Faturalar.SingleAsync(x => x.Id == fatura.Id);
        Assert.Equal(FaturaDurumu.Kapali, guncelFatura.Durum);
    }

    [Fact]
    public async Task Kredi_karti_tahsilat_basarili_ve_banka_hareketi_olusur()
    {
        var banka = await BankaEkleAsync();
        var fatura = await FaturaEkleAsync(paraBirimKodu: "USD", kur: 38.25m, netToplam: 100m);

        var tahsilat = await _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
        {
            SubeId = 1,
            FaturaId = fatura.Id,
            OdemeTipi = OdemeTipi.KrediKarti,
            BankaId = banka.Id,
            KullaniciId = 1,
            CihazId = 1,
            Tutar = 100m,
            ParaBirimKodu = "USD",
            Kur = 38.25m,
            Aciklama = "Kart tahsilat"
        });

        var hareket = await _dbContext.BankaHareketleri.SingleAsync(x => x.TahsilatId == tahsilat.Id);
        Assert.Equal(banka.Id, hareket.BankaId);
        Assert.Equal(3825m, hareket.YerelTutar);
    }

    [Fact]
    public async Task Veresiye_tahsilat_basarili_ve_cari_hareket_olusur()
    {
        var cari = await CariEkleAsync();
        var fatura = await FaturaEkleAsync(cariId: cari.Id, netToplam: 250m);

        var tahsilat = await _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
        {
            SubeId = 1,
            FaturaId = fatura.Id,
            OdemeTipi = OdemeTipi.Veresiye,
            KullaniciId = 1,
            CihazId = 1,
            Tutar = 250m,
            ParaBirimKodu = "TRY",
            Kur = 1m,
            Aciklama = "Veresiye satis"
        });

        var hareket = await _dbContext.CariHareketleri.SingleAsync(x => x.TahsilatId == tahsilat.Id);
        Assert.Equal(cari.Id, hareket.CariId);
        Assert.Equal(CariHareketTipi.Borc, hareket.HareketTipi);
        Assert.Equal(250m, hareket.YerelTutar);
    }

    [Fact]
    public async Task Yerel_tutar_dogru_hesaplanir()
    {
        var banka = await BankaEkleAsync();
        var fatura = await FaturaEkleAsync(paraBirimKodu: "EUR", kur: 41.755m, netToplam: 10m);

        var tahsilat = await _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
        {
            SubeId = 1,
            FaturaId = fatura.Id,
            OdemeTipi = OdemeTipi.KrediKarti,
            BankaId = banka.Id,
            KullaniciId = 1,
            CihazId = 1,
            Tutar = 10m,
            ParaBirimKodu = "EUR",
            Kur = 41.755m
        });

        Assert.Equal(417.55m, tahsilat.YerelTutar);
    }

    [Fact]
    public async Task Hareket_olusturma_hatasi_transaction_ile_geri_alinir()
    {
        var fatura = await FaturaEkleAsync();

        await Assert.ThrowsAsync<UygulamaHatasi>(() => _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
        {
            SubeId = 1,
            FaturaId = fatura.Id,
            OdemeTipi = OdemeTipi.Nakit,
            KasaId = 999,
            KullaniciId = 1,
            CihazId = 1,
            Tutar = 150m,
            ParaBirimKodu = "TRY",
            Kur = 1m
        }));

        Assert.False(await _dbContext.Tahsilatlar.AnyAsync(x => x.FaturaId == fatura.Id));
        var guncelFatura = await _dbContext.Faturalar.SingleAsync(x => x.Id == fatura.Id);
        Assert.Equal(FaturaDurumu.Acik, guncelFatura.Durum);
    }

    [Fact]
    public async Task Tahsilat_listeleme_calisir()
    {
        var kasa = await KasaEkleAsync();

        for (var i = 0; i < 3; i++)
        {
            var fatura = await FaturaEkleAsync(netToplam: 100m + i, faturaNo: $"FTR-TEST-{i:000}");
            await _tahsilatServisi.TahsilatOlusturAsync(new TahsilatOlusturRequestDto
            {
                SubeId = 1,
                FaturaId = fatura.Id,
                OdemeTipi = OdemeTipi.Nakit,
                KasaId = kasa.Id,
                KullaniciId = 1,
                CihazId = 1,
                Tutar = 100m + i,
                ParaBirimKodu = "TRY",
                Kur = 1m
            });
        }

        var liste = await _tahsilatServisi.TahsilatListeleAsync(1, 1, 2);

        Assert.Equal(3, liste.ToplamKayit);
        Assert.Equal(2, liste.Kayitlar.Count);
        Assert.All(liste.Kayitlar, x => Assert.True(x.YerelTutar > 0));
    }

    [Fact]
    public async Task Banka_olusturulup_listelenir()
    {
        await _bankaServisi.BankaOlusturAsync(new BankaOlusturRequestDto
        {
            SubeId = 1,
            Ad = "Ziraat",
            Kod = "ZR001"
        });

        var liste = await _bankaServisi.BankaListeleAsync(1);

        var banka = Assert.Single(liste);
        Assert.Equal("Ziraat", banka.Ad);
    }

    private async Task<Fatura> FaturaEkleAsync(
        string paraBirimKodu = "TRY",
        decimal kur = 1m,
        decimal netToplam = 150m,
        long? cariId = null,
        string? faturaNo = null)
    {
        var fatura = new Fatura
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            FaturaNo = faturaNo ?? $"FTR-{Guid.NewGuid():N}"[..20],
            CariId = cariId,
            ParaBirimKodu = paraBirimKodu,
            Kur = kur,
            AraToplam = netToplam,
            GenelIndirimTutari = 0,
            NetToplam = netToplam,
            ToplamTutar = netToplam,
            Durum = FaturaDurumu.Acik,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Faturalar.Add(fatura);
        await _dbContext.SaveChangesAsync();
        return fatura;
    }

    private async Task<Kasa> KasaEkleAsync()
    {
        var kasa = new Kasa
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = $"Kasa-{Guid.NewGuid():N}"[..12],
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Kasalar.Add(kasa);
        await _dbContext.SaveChangesAsync();
        return kasa;
    }

    private async Task<Banka> BankaEkleAsync()
    {
        var banka = new Banka
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            Ad = $"Banka-{Guid.NewGuid():N}"[..12],
            Kod = $"BNK-{Guid.NewGuid():N}"[..12],
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Bankalar.Add(banka);
        await _dbContext.SaveChangesAsync();
        return banka;
    }

    private async Task<Cari> CariEkleAsync()
    {
        var cari = new Cari
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CariKodu = $"CRI-{Guid.NewGuid():N}"[..12],
            Ad = "Cari Test",
            Tip = CariTipi.Alici,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Cariler.Add(cari);
        await _dbContext.SaveChangesAsync();
        return cari;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
