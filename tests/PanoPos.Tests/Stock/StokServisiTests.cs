using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Stock;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Stock;

namespace PanoPos.Tests.Stock;

public sealed class StokServisiTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly StokServisi service;
    private readonly StokKart stock;
    private readonly StokKartSatisBirimi unit;
    private readonly Depo target;

    public StokServisiTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        service = new(db, new IslemLogServisi(db));
        stock = new() { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Ad = "Su", KdvId = 1 };
        target = new() { TenantId = stock.TenantId, SubeId = 1, DepoKodu = "HEDEF", Ad = "Hedef" };
        db.AddRange(stock, target);
        db.SaveChanges();
        unit = new() { TenantId = stock.TenantId, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "ADET", BirimAdi = "Adet", Katsayi = 1 };
        db.Add(unit);
        db.SaveChanges();
    }

    private StokFisOlusturRequest Request(decimal amount = 100, bool transfer = false) => new() {
        SubeId = 1, DepoId = transfer ? null : 1, KaynakDepoId = transfer ? 1 : null,
        HedefDepoId = transfer ? target.Id : null,
        Detaylar = new() { new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = amount } }
    };
    private async Task<decimal> Quantity(long depo = 1, long? variant = null) =>
        (await service.GetMiktarAsync(1, depo, stock.Id, variant)).Miktar;

    [Fact]
    public async Task Devir_yuz_adet_stok_yuz()
    {
        var fis = await service.DevirAsync(Request());
        Assert.Equal(100m, await Quantity());
        Assert.Equal(StokFisTipi.Devir, fis.StokFisTipi);
        Assert.Equal(100m, Assert.Single(await db.StokHareketleri.ToListAsync()).Miktar);
        Assert.Single(await db.IslemLoglari.Where(x => x.HedefId == fis.Id && x.ModulAdi == "Stok").ToListAsync());
    }

    [Fact]
    public async Task Devir_on_koli_yirmidort_katsayi_ikiyuzkirk()
    {
        unit.Katsayi = 24; unit.BirimKodu = "KOLI"; unit.BirimAdi = "Koli";
        await db.SaveChangesAsync();
        var fis = await service.DevirAsync(Request(10));
        Assert.Equal(240m, await Quantity());
        Assert.Equal(24m, fis.Detaylar[0].Katsayi);
        unit.Katsayi = 30; unit.BirimAdi = "Degisti";
        await db.SaveChangesAsync();
        var stored = await service.GetByIdAsync(fis.Id, 1);
        Assert.Equal("Koli", stored.Detaylar[0].BirimAdi);
        Assert.Equal(24m, stored.Detaylar[0].Katsayi);
        Assert.Equal(240m, await Quantity());
    }

    [Theory]
    [InlineData(93, -7)]
    [InlineData(105, 5)]
    [InlineData(100, 0)]
    [InlineData(0, -100)]
    public async Task Sayim_farki_ve_snapshot(decimal counted, decimal difference)
    {
        await service.DevirAsync(Request());
        var fis = await service.SayimAsync(Request(counted));
        var line = Assert.Single(fis.Detaylar);
        Assert.Equal(100m, line.SistemMiktari);
        Assert.Equal(counted, line.SayilanMiktar);
        Assert.Equal(difference, line.FarkMiktari);
        var moves = await db.StokHareketleri.Where(x => x.StokFisId == fis.Id).ToListAsync();
        if (difference == 0) Assert.Empty(moves);
        else Assert.Equal(difference, Assert.Single(moves).Miktar);
        Assert.Equal(counted, await Quantity());
    }

    [Fact]
    public async Task Sayim_sayilan_birim_temel_miktara_cevrilir()
    {
        await service.DevirAsync(Request());
        unit.Katsayi = 24; await db.SaveChangesAsync();
        var fis = await service.SayimAsync(Request(4));
        Assert.Equal(96m, fis.Detaylar[0].SayilanMiktar);
        Assert.Equal(-4m, fis.Detaylar[0].FarkMiktari);
        Assert.Equal(96m, await Quantity());
    }

    [Theory]
    [InlineData(10, 1, 10)]
    [InlineData(2, 24, 48)]
    public async Task Transfer_tek_fis_iki_hareket_ve_eksi_stok_serbest(decimal amount, decimal coefficient, decimal expected)
    {
        unit.Katsayi = coefficient; await db.SaveChangesAsync();
        var fis = await service.TransferAsync(Request(amount, true));
        Assert.Equal(-expected, await Quantity());
        Assert.Equal(expected, await Quantity(target.Id));
        var moves = await db.StokHareketleri.Where(x => x.StokFisId == fis.Id).ToListAsync();
        Assert.Equal(2, moves.Count);
        Assert.Equal(0, moves.Sum(x => x.Miktar));
        Assert.Contains(moves, x => x.DepoId == 1 && x.StokHareketTipi == StokHareketTipi.DepoTransferCikis);
        Assert.Contains(moves, x => x.DepoId == target.Id && x.StokHareketTipi == StokHareketTipi.DepoTransferGiris);
        Assert.Single(await db.StokFisleri.ToListAsync());
    }

    [Fact]
    public async Task Ayni_depo_transferi_reddedilir()
    {
        var r = Request(10, true); r.HedefDepoId = r.KaynakDepoId;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.TransferAsync(r));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.00001)]
    public async Task Devir_ve_transfer_gecersiz_miktar(decimal amount)
    {
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(Request(amount)));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.TransferAsync(Request(amount, true)));
        Assert.Empty(await db.StokFisleri.ToListAsync());
    }

    [Fact]
    public async Task Negatif_sayim_reddedilir() =>
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.SayimAsync(Request(-1)));

    [Fact]
    public async Task Tekrarli_sayim_satiri_reddedilir()
    {
        var r = Request();
        r.Detaylar.Add(r.Detaylar[0]);
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.SayimAsync(r));
        Assert.Empty(await db.StokFisleri.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Depo_tenant_ve_sube_izolasyonu(bool otherTenant)
    {
        var tenant = otherTenant ? Guid.NewGuid() : stock.TenantId;
        var branch = await AddBranch(tenant);
        target.TenantId = tenant; target.SubeId = branch.Id; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.TransferAsync(Request(10, true)));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.GetMiktarAsync(1, target.Id, stock.Id, null));
    }

    [Fact]
    public async Task Baska_tenant_stok_karti_kullanilamaz()
    {
        var branch = await AddBranch(Guid.NewGuid());
        var other = new StokKart { TenantId = branch.TenantId, SubeId = branch.Id, KdvId = 1, Ad = "Diger tenant" };
        db.Add(other); await db.SaveChangesAsync();
        var r = Request(); r.Detaylar[0].StokKartId = other.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(r));
    }

    [Fact]
    public async Task Baska_stok_birimi_kullanilamaz()
    {
        var other = new StokKart { TenantId = stock.TenantId, SubeId = 1, KdvId = 1, Ad = "Diger" };
        db.Add(other); await db.SaveChangesAsync();
        unit.StokKartId = other.Id; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(Request()));
    }

    [Fact]
    public async Task Baska_stok_varyanti_kullanilamaz()
    {
        var other = new StokKart { TenantId = stock.TenantId, SubeId = 1, KdvId = 1, Ad = "Diger" };
        db.Add(other); await db.SaveChangesAsync();
        var variant = await AddVariant(other.Id, "V1");
        var r = Request(); r.Detaylar[0].StokKartVaryantId = variant.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(r));
    }

    [Theory]
    [InlineData("Depo", false)]
    [InlineData("Depo", true)]
    [InlineData("Stok", false)]
    [InlineData("Stok", true)]
    [InlineData("Birim", false)]
    [InlineData("Birim", true)]
    [InlineData("Varyant", false)]
    [InlineData("Varyant", true)]
    public async Task Pasif_silinmis_kaynaklar_kullanilamaz(string kind, bool deleted)
    {
        var r = Request(10, true);
        PanoPos.Domain.Common.BaseEntity entity = kind switch {
            "Depo" => target, "Stok" => stock, "Birim" => unit, _ => await AddVariant(stock.Id, "V1")
        };
        if (entity is StokKartVaryant variant) r.Detaylar[0].StokKartVaryantId = variant.Id;
        if (deleted) entity.SoftDelete(null, DateTime.UtcNow); else entity.AktifMi = false;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.TransferAsync(r));
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Fact]
    public async Task Varyantli_ve_varyantsiz_stok_birbirinden_ayri()
    {
        var first = await AddVariant(stock.Id, "V1");
        var second = await AddVariant(stock.Id, "V2");
        await service.DevirAsync(Request(7));
        var r = Request(10); r.Detaylar[0].StokKartVaryantId = first.Id; await service.DevirAsync(r);
        r = Request(20); r.Detaylar[0].StokKartVaryantId = second.Id; await service.DevirAsync(r);
        r = Request(3); r.Detaylar[0].StokKartVaryantId = first.Id; await service.SayimAsync(r);
        Assert.Equal(7m, await Quantity());
        Assert.Equal(3m, await Quantity(1, first.Id));
        Assert.Equal(20m, await Quantity(1, second.Id));
    }

    [Fact]
    public async Task Transfer_hedef_hatasi_kaynak_fis_ve_detaylari_rollback_yapar()
    {
        await db.Database.ExecuteSqlRawAsync(@"CREATE TRIGGER FailTransfer BEFORE INSERT ON StokHareket
WHEN NEW.StokHareketTipi = 4 BEGIN SELECT RAISE(ABORT, 'test target failure'); END;");
        await Assert.ThrowsAsync<DbUpdateException>(() => service.TransferAsync(Request(10, true)));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokFisDetaylari.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Empty(await db.IslemLoglari.ToListAsync());
    }

    [Fact]
    public async Task Audit_hatasi_tum_stok_islemini_rollback_yapar()
    {
        await db.Database.ExecuteSqlRawAsync(@"CREATE TRIGGER FailAudit BEFORE INSERT ON IslemLog
BEGIN SELECT RAISE(ABORT, 'test audit failure'); END;");
        await Assert.ThrowsAsync<DbUpdateException>(() => service.DevirAsync(Request()));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokFisDetaylari.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Fact]
    public async Task Ayni_detay_ayni_hareket_tipi_ikinci_kez_yazilamaz()
    {
        await service.TransferAsync(Request(10, true));
        var source = await db.StokHareketleri.FirstAsync(x => x.StokHareketTipi == StokHareketTipi.DepoTransferCikis);
        db.StokHareketleri.Add(new() { TenantId = source.TenantId, SubeId = source.SubeId, DepoId = source.DepoId,
            StokKartId = source.StokKartId, StokFisId = source.StokFisId, StokFisDetayId = source.StokFisDetayId,
            StokHareketTipi = source.StokHareketTipi, Miktar = source.Miktar, HareketTarihi = source.HareketTarihi });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal(2, await db.StokHareketleri.CountAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ledger_guncellenemez_silinemez(bool delete)
    {
        await service.DevirAsync(Request());
        var move = await db.StokHareketleri.SingleAsync();
        if (delete) db.Remove(move); else move.Miktar = 500;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal(100m, await Quantity());
    }

    [Fact]
    public async Task Fis_numarasi_tekrari_hareket_uretmez()
    {
        var r = Request(); r.FisNo = "STK-TEST";
        await service.DevirAsync(r);
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(r));
        Assert.Equal(100m, await Quantity());
        Assert.Single(await db.StokFisleri.ToListAsync());
    }

    [Fact]
    public async Task Stok_hareket_toplamidir_ve_decimal_hassasiyeti_korunur()
    {
        await service.DevirAsync(Request(10.1234m));
        await service.DevirAsync(Request(0.0001m));
        await service.TransferAsync(Request(3.1234m, true));
        Assert.Equal(7.0001m, await Quantity());
        var moves = await db.StokHareketleri.Where(x => x.DepoId == 1).ToListAsync();
        Assert.Equal(moves.Sum(x => x.Miktar), await Quantity());
    }

    [Fact]
    public async Task Fis_liste_pagination_arama_tip_depo_ve_scope()
    {
        var first = Request(); first.FisNo = "DEV-1";
        await service.DevirAsync(first);
        var second = Request(); second.FisNo = "DEV-2";
        await service.DevirAsync(second);
        await service.TransferAsync(Request(1, true));
        var page = await service.GetPagedAsync(new() { SubeId = 1, Page = 2, PageSize = 1, StokFisTipi = StokFisTipi.Devir });
        Assert.Equal(2, page.ToplamKayit); Assert.Single(page.Kayitlar);
        var search = await service.GetPagedAsync(new() { SubeId = 1, Arama = "DEV-1" });
        Assert.Equal("DEV-1", Assert.Single(search.Kayitlar).FisNo);
        Assert.Single((await service.GetPagedAsync(new() { SubeId = 1, DepoId = target.Id })).Kayitlar);
        var branch = await AddBranch(Guid.NewGuid());
        Assert.Empty((await service.GetPagedAsync(new() { SubeId = branch.Id })).Kayitlar);
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.GetByIdAsync(search.Kayitlar[0].Id, branch.Id));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 501)]
    public async Task Gecersiz_pagination_reddedilir(int page, int size) =>
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.GetPagedAsync(new() { SubeId = 1, Page = page, PageSize = size }));

    [Fact]
    public async Task Yanlis_depo_alanlari_reddedilir()
    {
        var r = Request(); r.KaynakDepoId = 1;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(r));
        r = Request(1, true); r.DepoId = 1;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.TransferAsync(r));
    }

    [Fact]
    public async Task Ondalik_temel_miktar_kaybi_sessiz_yuvarlanmaz()
    {
        unit.Katsayi = 0.0001m; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DevirAsync(Request(0.0001m)));
    }

    private async Task<StokKartVaryant> AddVariant(long stockId, string code)
    {
        var color = new Renk { TenantId = stock.TenantId, SubeId = 1, Ad = code, Kod = code };
        db.Add(color); await db.SaveChangesAsync();
        var variant = new StokKartVaryant { TenantId = stock.TenantId, SubeId = 1, StokKartId = stockId, VaryantKodu = code, RenkId = color.Id };
        db.Add(variant); await db.SaveChangesAsync(); return variant;
    }

    private async Task<Sube> AddBranch(Guid tenant)
    {
        if (!await db.Tenantler.AnyAsync(x => x.TenantId == tenant))
        {
            db.Add(new Tenant { TenantId = tenant, SubeId = 1, Kod = tenant.ToString("N")[..10], Ad = "Diger tenant" });
            await db.SaveChangesAsync();
        }
        var branch = new Sube { TenantId = tenant, SubeId = 1, Kod = "SUBE2", Ad = "Diger sube" };
        db.Add(branch); await db.SaveChangesAsync(); return branch;
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
