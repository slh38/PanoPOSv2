using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Purchase;
using PanoPos.Application.Tax;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Purchase;
using PanoPos.Infrastructure.Stock;

namespace PanoPos.Tests.Purchase;

public sealed class PurchaseStockTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly AlisFaturaServisi service;
    private readonly StokServisi stockService;
    private readonly StokKart stock;
    private readonly StokKartSatisBirimi unit;
    private readonly Cari customer;
    private readonly Depo target;

    public PurchaseStockTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        service = new(db, new VergiHesaplamaServisi());
        stockService = new(db, new IslemLogServisi(db));
        stock = new() { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Ad = "Su", KdvId = 4 };
        customer = new() { TenantId = stock.TenantId, SubeId = 1, Ad = "Tedarikci", CariKodu = "C1", Tip = CariTipi.Satici };
        target = new() { TenantId = stock.TenantId, SubeId = 1, DepoKodu = "ALIS", Ad = "Alis Deposu" };
        db.AddRange(stock, customer, target); db.SaveChanges();
        unit = new() { TenantId = stock.TenantId, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1 };
        db.Add(unit); db.SaveChanges();
    }

    private AlisFaturaKaydetRequest Request(decimal quantity = 10) => new() {
        SubeId = 1, DepoId = target.Id, CariId = customer.Id, FaturaNo = "AL-TEST", FaturaTarihi = new(2026, 9, 17),
        Detaylar = new() { new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = quantity, BirimFiyat = 120 } }
    };
    private async Task<decimal> Quantity(long? variant = null) =>
        (await stockService.GetMiktarAsync(1, target.Id, stock.Id, variant)).Miktar;

    [Fact]
    public async Task Taslak_create_update_stok_uretmez()
    {
        var f = await service.CreateAsync(Request());
        var r = Request(20); r.DepoId = 1;
        f = await service.UpdateAsync(f.Id, r);
        Assert.Equal(1, f.DepoId);
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokFisDetaylari.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Equal(1, (await service.GetPagedAsync(new() { SubeId = 1 })).Kayitlar[0].DepoId);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(24, 240)]
    public async Task Kesinlestirme_dogru_depo_baglanti_ve_temel_miktar(decimal coefficient, decimal expected)
    {
        unit.Katsayi = coefficient; await db.SaveChangesAsync();
        var f = await service.CreateAsync(Request());
        var finished = await service.KesinlestirAsync(f.Id, 1);
        var fis = await db.StokFisleri.Include(x => x.Detaylar).SingleAsync();
        var move = await db.StokHareketleri.SingleAsync();
        Assert.Equal(AlisFaturaDurumu.Kesinlesti, finished.Durum);
        Assert.Equal(StokFisTipi.Alis, fis.StokFisTipi);
        Assert.Equal(f.Id, fis.AlisFaturaId);
        Assert.Equal(target.Id, fis.DepoId);
        Assert.Equal(f.FaturaTarihi, fis.FisTarihi);
        Assert.Null(fis.KaynakDepoId); Assert.Null(fis.HedefDepoId);
        Assert.Equal(expected, move.Miktar);
        Assert.Equal(StokHareketTipi.Alis, move.StokHareketTipi);
        Assert.Equal(fis.Id, move.StokFisId);
        Assert.Equal(fis.Detaylar.Single().Id, move.StokFisDetayId);
        Assert.Equal(target.Id, move.DepoId);
        Assert.Equal(expected, await Quantity());
        Assert.Equal(0, (await stockService.GetMiktarAsync(1, 1, stock.Id, null)).Miktar);
        Assert.Equal(f.Id, (await stockService.GetByIdAsync(fis.Id, 1)).AlisFaturaId);
        Assert.Equal(f.Id, (await stockService.GetPagedAsync(new() { SubeId = 1 })).Kayitlar[0].AlisFaturaId);
        Assert.Single(await db.IslemLoglari.Where(x => x.HedefId == f.Id && x.IslemTipi == "KesinlestirStokGirisi").ToListAsync());
    }

    [Fact]
    public async Task Tekrar_kesinlestirme_ikinci_stok_ve_audit_uretmez()
    {
        var f = await service.CreateAsync(Request());
        await service.KesinlestirAsync(f.Id, 1);
        db.ChangeTracker.Clear();
        await service.KesinlestirAsync(f.Id, 1);
        Assert.Single(await db.StokFisleri.ToListAsync());
        Assert.Single(await db.StokFisDetaylari.ToListAsync());
        Assert.Single(await db.StokHareketleri.ToListAsync());
        Assert.Single(await db.IslemLoglari.ToListAsync());
        Assert.Equal(10, await Quantity());
    }

    [Fact]
    public async Task Unique_index_ikinci_alis_fisini_engeller()
    {
        var f = await service.CreateAsync(Request());
        await service.KesinlestirAsync(f.Id, 1);
        db.StokFisleri.Add(new() { TenantId = stock.TenantId, SubeId = 1, DepoId = target.Id,
            AlisFaturaId = f.Id, StokFisTipi = StokFisTipi.Alis, FisNo = "FARKLI", FisTarihi = DateTime.UtcNow });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Single(await db.StokFisleri.ToListAsync());
        Assert.Equal(10, await Quantity());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999999)]
    public async Task Depo_zorunlu_ve_mevcut_olmali(long depoId)
    {
        var r = Request(); r.DepoId = depoId;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.CreateAsync(r));
        Assert.Empty(await db.AlisFaturalar.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Baska_sube_veya_tenant_deposu_create_update_kesinlestirme_reddeder(bool otherTenant)
    {
        var f = await service.CreateAsync(Request());
        var tenant = otherTenant ? Guid.NewGuid() : stock.TenantId;
        if (otherTenant) {
            db.Add(new Tenant { TenantId = tenant, SubeId = 1, Kod = "T2", Ad = "Tenant2" });
            await db.SaveChangesAsync();
        }
        var branch = new Sube { TenantId = tenant, SubeId = 1, Kod = "S2", Ad = "Sube2" };
        db.Add(branch); await db.SaveChangesAsync();
        target.TenantId = tenant; target.SubeId = branch.Id; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.CreateAsync(Request()));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.UpdateAsync(f.Id, Request()));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.KesinlestirAsync(f.Id, 1));
        Assert.Empty(await db.StokFisleri.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Pasif_silinmis_depo_create_ve_kesinlestirme_reddeder(bool deleted)
    {
        var f = await service.CreateAsync(Request());
        if (deleted) target.SoftDelete(null, DateTime.UtcNow); else target.AktifMi = false;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.CreateAsync(Request()));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.KesinlestirAsync(f.Id, 1));
        Assert.Empty(await db.StokFisleri.ToListAsync());
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, -1)]
    [InlineData(false, 0)]
    [InlineData(false, -1)]
    public async Task Gecersiz_snapshot_miktar_katsayi_kesinlesmez(bool quantity, decimal value)
    {
        var f = await service.CreateAsync(Request());
        var line = await db.AlisFaturaDetaylari.SingleAsync();
        if (quantity) line.Miktar = value; else line.Katsayi = value;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.KesinlestirAsync(f.Id, 1));
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Equal(AlisFaturaDurumu.Taslak, (await service.GetByIdAsync(f.Id, 1)).Durum);
    }

    [Fact]
    public async Task Snapshot_master_degisse_de_korunur()
    {
        unit.Katsayi = 24; unit.BirimKodu = "KOLI"; unit.BirimAdi = "Koli"; await db.SaveChangesAsync();
        var f = await service.CreateAsync(Request());
        unit.Katsayi = 30; unit.BirimKodu = "YENI"; unit.BirimAdi = "Yeni birim"; await db.SaveChangesAsync();
        await service.KesinlestirAsync(f.Id, 1);
        var detail = await db.StokFisDetaylari.SingleAsync();
        Assert.Equal(24, detail.Katsayi);
        Assert.Equal("KOLI", detail.BirimKodu); Assert.Equal("Koli", detail.BirimAdi);
        Assert.Equal(240, await Quantity());
    }

    [Fact]
    public async Task Cok_satir_varyant_ve_null_varyant_ayri_stoklanir()
    {
        var color = new Renk { TenantId = stock.TenantId, SubeId = 1, Kod = "M", Ad = "Mavi" };
        db.Add(color); await db.SaveChangesAsync();
        var variant = new StokKartVaryant { TenantId = stock.TenantId, SubeId = 1, StokKartId = stock.Id, RenkId = color.Id, VaryantKodu = "V1" };
        db.Add(variant); await db.SaveChangesAsync();
        var r = Request();
        r.Detaylar.Add(new() { StokKartId = stock.Id, StokKartVaryantId = variant.Id, StokKartSatisBirimiId = unit.Id, Miktar = 7, BirimFiyat = 10 });
        var f = await service.CreateAsync(r); await service.KesinlestirAsync(f.Id, 1);
        Assert.Equal(2, await db.StokFisDetaylari.CountAsync());
        Assert.Equal(2, await db.StokHareketleri.CountAsync());
        Assert.Equal(10, await Quantity()); Assert.Equal(7, await Quantity(variant.Id));
        Assert.Contains(await db.StokFisDetaylari.ToListAsync(), x => x.StokKartVaryantId == variant.Id);
    }

    [Theory]
    [InlineData("movement")]
    [InlineData("audit")]
    [InlineData("status")]
    public async Task Son_asamada_hata_tum_islemi_geri_alir(string stage)
    {
        var f = await service.CreateAsync(Request());
        var trigger = stage switch {
            "movement" => "CREATE TRIGGER Fail BEFORE INSERT ON StokHareket BEGIN SELECT RAISE(ABORT, 'movement'); END;",
            "audit" => "CREATE TRIGGER Fail BEFORE INSERT ON IslemLog BEGIN SELECT RAISE(ABORT, 'audit'); END;",
            _ => "CREATE TRIGGER Fail BEFORE UPDATE ON AlisFatura WHEN NEW.Durum = 1 BEGIN SELECT RAISE(ABORT, 'status'); END;"
        };
        await db.Database.ExecuteSqlRawAsync(trigger);
        await Assert.ThrowsAsync<DbUpdateException>(() => service.KesinlestirAsync(f.Id, 1));
        db.ChangeTracker.Clear();
        Assert.Equal(AlisFaturaDurumu.Taslak, (await service.GetByIdAsync(f.Id, 1)).Durum);
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokFisDetaylari.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Empty(await db.IslemLoglari.ToListAsync());
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER Fail;");
        await service.KesinlestirAsync(f.Id, 1);
        Assert.Equal(10, await Quantity());
    }

    [Fact]
    public async Task Kesinlesmis_fatura_depo_detay_update_delete_iptal_engelli()
    {
        var f = await service.CreateAsync(Request());
        await service.KesinlestirAsync(f.Id, 1);
        var r = Request(50); r.DepoId = 1;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.UpdateAsync(f.Id, r));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.DeleteAsync(f.Id, 1));
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.IptalAsync(f.Id, 1));
        Assert.Equal("purchase_stock_reversal_required", error.ErrorCode);
        var stored = await service.GetByIdAsync(f.Id, 1);
        Assert.Equal(target.Id, stored.DepoId); Assert.Equal(10, stored.Detaylar[0].Miktar);
        Assert.Equal(AlisFaturaDurumu.Kesinlesti, stored.Durum);
        Assert.Equal(10, await Quantity());
    }

    [Fact]
    public async Task Taslak_iptal_stok_uretmez()
    {
        var f = await service.CreateAsync(Request());
        await service.IptalAsync(f.Id, 1);
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.KesinlestirAsync(f.Id, 1));
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Kdv_hesaplari_degismeden_stok_uretilir(bool included)
    {
        var setting = await db.TenantAyarlari.SingleAsync();
        setting.AlisFiyatlariKdvDahilMi = included; await db.SaveChangesAsync();
        var r = Request(); r.GenelIndirimTutari = 100;
        var f = await service.CreateAsync(r);
        var completed = await service.KesinlestirAsync(f.Id, 1);
        Assert.Equal(f.NetToplam, completed.NetToplam);
        Assert.Equal(f.ToplamKdv, completed.ToplamKdv);
        Assert.Equal(f.ToplamMatrah, completed.ToplamMatrah);
        Assert.Equal(included ? 1100m : 1320m, completed.NetToplam);
        Assert.Equal(10, await Quantity());
        Assert.Empty(await db.CariHareketleri.ToListAsync());
        Assert.Empty(await db.KasaHareketleri.ToListAsync());
        Assert.Empty(await db.BankaHareketleri.ToListAsync());
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
