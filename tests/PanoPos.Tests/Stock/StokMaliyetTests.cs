using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Purchase;
using PanoPos.Application.Stock;
using PanoPos.Application.Tax;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Purchase;
using PanoPos.Infrastructure.Stock;

namespace PanoPos.Tests.Stock;

public sealed class StokMaliyetTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly AlisFaturaServisi purchases;
    private readonly StokMaliyetServisi costs;
    private readonly StokKart stock;
    private readonly StokKartSatisBirimi unit;
    private readonly CariKart supplier;
    private readonly Guid tenant = SystemSeedData.TenantGuid;

    public StokMaliyetTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        costs = new(db); purchases = new(db, new VergiHesaplamaServisi(), costs);
        stock = new() { TenantId = tenant, SubeId = 1, Ad = "Stok", KdvId = 4 };
        supplier = new() { TenantId = tenant, SubeId = 1, Ad = "Tedarikci", CariKodu = "C1", Tip = CariTipi.Satici };
        db.AddRange(stock, supplier); db.SaveChanges();
        unit = new() { TenantId = tenant, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true };
        db.Add(unit); db.SaveChanges();
    }

    private AlisFaturaKaydetRequest Request(decimal qty = 100, decimal price = 10, long depo = 1, long? variant = null) => new() {
        SubeId = 1, DepoId = depo, CariId = supplier.Id, FaturaNo = Guid.NewGuid().ToString("N"), FaturaTarihi = new(2026, 9, 17),
        Detaylar = new() { new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id,
            StokKartVaryantId = variant, Miktar = qty, BirimFiyat = price } }
    };
    private async Task<AlisFaturaDto> Buy(AlisFaturaKaydetRequest? request = null)
    {
        var f = await purchases.CreateAsync(request ?? Request());
        return await purchases.KesinlestirAsync(f.Id, 1);
    }
    private Task<StokMaliyetDto> Cost(long depo = 1, long? variant = null) => costs.GetAsync(1, depo, stock.Id, variant);
    private async Task<FaturaDto> Sell(decimal qty = 1, long depo = 1, long? variant = null)
    {
        var orders = new SiparisServisi(db);
        var order = await orders.SiparisOlusturAsync(new() { SubeId = 1, SiparisTipi = SiparisTipi.HizliSatisBekleyen, ParaBirimKodu = "TRY", Kur = 1 });
        await orders.KayitliFiyatlaSatirEkleAsync(db, order.Id, new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id,
            StokKartVaryantId = variant, Miktar = qty, BirimFiyat = 100 });
        return await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new() { SiparisId = order.Id, DepoId = depo });
    }

    [Fact] public async Task Varsayilan_yontem_ortalama_ve_okuma_kayit_uretmez()
    {
        Assert.Equal(MaliyetYontemi.AgirlikliOrtalama, (await db.TenantAyarlari.SingleAsync()).MaliyetYontemi);
        var c = await Cost();
        Assert.Equal(MaliyetYontemi.AgirlikliOrtalama, c.MaliyetYontemi);
        Assert.Equal(0, c.SeciliMaliyet); Assert.Equal("TRY", c.ParaBirimKodu);
        Assert.Empty(await db.StokMaliyetleri.ToListAsync());
    }

    [Fact] public async Task Ilk_alis_iki_maliyeti_birlikte_gunceller()
    {
        var f = await Buy();
        var c = await Cost();
        Assert.Equal(10, c.SonAlisMaliyeti); Assert.Equal(10, c.AgirlikliOrtalamaMaliyet);
        Assert.Equal(f.FaturaTarihi, (await db.StokMaliyetleri.SingleAsync()).SonAlisTarihi);
    }

    [Fact] public async Task Ikinci_alis_eski_ledger_miktariyla_ortalama_alir()
    {
        await Buy(); await Buy(Request(100, 14));
        var c = await Cost();
        Assert.Equal(14, c.SonAlisMaliyeti); Assert.Equal(12, c.AgirlikliOrtalamaMaliyet);
        Assert.Single(await db.StokMaliyetleri.ToListAsync());
    }

    [Fact] public async Task Satis_ortalama_ve_son_alis_degerini_degistirmez()
    {
        await Buy();
        var before = await db.StokMaliyetleri.AsNoTracking().SingleAsync();
        await Sell(20);
        var after = await db.StokMaliyetleri.AsNoTracking().SingleAsync();
        Assert.Equal(before.SonGuncellemeTarihi, after.SonGuncellemeTarihi);
        Assert.Equal(10, after.SonAlisMaliyeti); Assert.Equal(10, after.AgirlikliOrtalamaMaliyet);
    }

    [Theory] [InlineData(100)] [InlineData(110)]
    public async Task Sifir_ve_negatif_stokta_ortalama_yeni_alisa_resetlenir(decimal sale)
    {
        await Buy(); await Sell(sale); await Buy(Request(100, 20));
        Assert.Equal(20, (await Cost()).AgirlikliOrtalamaMaliyet);
    }

    [Fact] public async Task Dovizli_matrah_snapshot_kurla_yalniz_bir_kez_cevrilir()
    {
        var r = Request(10, 10);
        r.ParaBirimKodu = r.Detaylar[0].FiyatParaBirimKodu = "USD";
        r.Kur = r.Detaylar[0].FiyatKur = 42.5m;
        var f = await Buy(r);
        Assert.Equal(100, f.Detaylar[0].Matrah);
        Assert.Equal(425, (await Cost()).SonAlisMaliyeti);
        Assert.Equal(4250, (await Cost()).SonAlisMaliyeti * 10);
    }

    [Theory] [InlineData(true, 12)] [InlineData(false, 10)]
    public async Task Kdv_maliyete_dahil_edilmez(bool included, decimal price)
    {
        (await db.TenantAyarlari.SingleAsync()).AlisFiyatlariKdvDahilMi = included; await db.SaveChangesAsync();
        var f = await Buy(Request(100, price));
        Assert.Equal(1000, f.ToplamMatrah); Assert.Equal(200, f.ToplamKdv);
        Assert.Equal(10, (await Cost()).SonAlisMaliyeti);
    }

    [Theory] [InlineData(false, 9)] [InlineData(true, 8.1)]
    public async Task Satir_ve_genel_iskonto_sonrasi_matrah_kullanilir(bool general, decimal expected)
    {
        var r = Request(); r.Detaylar[0].IndirimOrani = 10;
        if (general) r.GenelIndirimOrani = 10;
        await Buy(r);
        Assert.Equal(expected, (await Cost()).SonAlisMaliyeti);
    }

    [Fact] public async Task Koli_alisi_temel_birim_maliyetine_donusturulur()
    {
        unit.Katsayi = 24; await db.SaveChangesAsync();
        await Buy(Request(10, 240));
        Assert.Equal(10, (await Cost()).SonAlisMaliyeti);
        Assert.Equal(240, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Tekrarlanan_satirlar_siradan_bagimsiz_efektif_maliyet_uretir(bool reverse)
    {
        await Buy(Request(100, 8));
        var r = Request(100, 10);
        r.Detaylar.Add(new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = 50, BirimFiyat = 12 });
        if (reverse) r.Detaylar.Reverse();
        await Buy(r);
        var c = await Cost();
        Assert.Equal(10.666667m, c.SonAlisMaliyeti);
        Assert.Equal(9.6m, c.AgirlikliOrtalamaMaliyet);
    }

    [Theory] [InlineData(MaliyetYontemi.SonAlis, 14)] [InlineData(MaliyetYontemi.AgirlikliOrtalama, 12)]
    public async Task Secilen_yontem_yeni_satisin_snapshotini_belirler(MaliyetYontemi method, decimal expected)
    {
        await Buy(); await Buy(Request(100, 14));
        await costs.YontemDegistirAsync(1, method);
        var line = Assert.Single((await Sell()).Detaylar);
        Assert.Equal(expected, line.BirimMaliyet); Assert.Equal(method, line.MaliyetYontemi);
        Assert.Equal(expected, (await Cost()).SeciliMaliyet);
    }

    [Fact] public async Task Yontem_ve_yeni_alis_gecmis_faturayi_degistirmez()
    {
        await Buy(); await Buy(Request(100, 14));
        await costs.YontemDegistirAsync(1, MaliyetYontemi.SonAlis);
        var old = await Sell();
        await costs.YontemDegistirAsync(1, MaliyetYontemi.AgirlikliOrtalama);
        Assert.Equal(12, Assert.Single((await Sell()).Detaylar).BirimMaliyet);
        await Buy(Request(100, 30));
        db.ChangeTracker.Clear();
        var line = Assert.Single((await new FaturaServisi(db).FaturaGetirAsync(old.Id)).Detaylar);
        Assert.Equal(14, line.BirimMaliyet); Assert.Equal(MaliyetYontemi.SonAlis, line.MaliyetYontemi);
    }

    [Fact] public async Task Alissiz_satis_sifir_snapshot_alir_sonraki_alis_bunu_degistirmez()
    {
        var f = await Sell();
        Assert.Equal(0, Assert.Single(f.Detaylar).BirimMaliyet);
        await Buy();
        db.ChangeTracker.Clear();
        Assert.Equal(0, Assert.Single((await new FaturaServisi(db).FaturaGetirAsync(f.Id)).Detaylar).BirimMaliyet);
    }

    [Fact] public async Task Iki_koli_satis_birim_maliyeti_240_satir_maliyeti_480()
    {
        await Buy();
        unit.Katsayi = 24; await db.SaveChangesAsync();
        var line = Assert.Single((await Sell(2)).Detaylar);
        Assert.Equal(240, line.BirimMaliyet); Assert.Equal(480, line.Miktar * line.BirimMaliyet);
        Assert.Equal(10, (await Cost()).AgirlikliOrtalamaMaliyet);
    }

    private async Task<long> Variant(string code)
    {
        var color = new Renk { TenantId = tenant, SubeId = 1, Ad = code, Kod = code };
        db.Add(color); await db.SaveChangesAsync();
        var v = new StokKartVaryant { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, VaryantKodu = code, RenkId = color.Id };
        db.Add(v); await db.SaveChangesAsync(); return v.Id;
    }

    [Fact] public async Task Iki_varyant_ve_varyantsiz_maliyetler_ayridir()
    {
        var a = await Variant("A"); var b = await Variant("B");
        await Buy(Request(100, 10, variant: a)); await Buy(Request(100, 20, variant: b)); await Buy(Request(100, 30));
        Assert.Equal(10, (await Cost(variant: a)).SeciliMaliyet);
        Assert.Equal(20, (await Cost(variant: b)).SeciliMaliyet);
        Assert.Equal(30, (await Cost()).SeciliMaliyet);
        Assert.Equal(3, await db.StokMaliyetleri.CountAsync());
        Assert.Equal(20, Assert.Single((await Sell(variant: b)).Detaylar).BirimMaliyet);
    }

    [Fact] public async Task Ayni_stok_iki_depoda_farkli_maliyet_tasir()
    {
        var depo = new Depo { TenantId = tenant, SubeId = 1, DepoKodu = "D2", Ad = "D2" };
        db.Add(depo); await db.SaveChangesAsync();
        await Buy(); await Buy(Request(100, 12, depo.Id));
        Assert.Equal(10, (await Cost()).SeciliMaliyet);
        Assert.Equal(12, (await Cost(depo.Id)).SeciliMaliyet);
        Assert.Equal(12, Assert.Single((await Sell(depo: depo.Id)).Detaylar).BirimMaliyet);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Null_ve_varyant_anahtarlari_database_unique_ile_korunur(bool hasVariant)
    {
        long? variant = hasVariant ? await Variant("A") : null;
        await Buy(Request(variant: variant));
        db.Add(new StokMaliyet { TenantId = tenant, SubeId = 1, DepoId = 1, StokKartId = stock.Id,
            StokKartVaryantId = variant, SonAlisMaliyeti = 99, AgirlikliOrtalamaMaliyet = 99,
            OlusturmaTarihi = DateTime.UtcNow, SonGuncellemeTarihi = DateTime.UtcNow });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Theory] [InlineData("StokMaliyet")] [InlineData("StokFis")] [InlineData("StokFisDetay")]
    [InlineData("StokHareket")] [InlineData("IslemLog")]
    public async Task Ilk_alis_hatasi_maliyet_dahil_her_seyi_rollback_yapar(string table)
    {
        var f = await purchases.CreateAsync(Request());
        var sql = $"CREATE TRIGGER FailCost BEFORE INSERT ON {table} BEGIN SELECT RAISE(ABORT, 'cost test'); END;";
        await db.Database.ExecuteSqlRawAsync(sql);
        await Assert.ThrowsAsync<DbUpdateException>(() => purchases.KesinlestirAsync(f.Id, 1));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.StokMaliyetleri.ToListAsync()); Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Equal(AlisFaturaDurumu.Taslak, (await purchases.GetByIdAsync(f.Id, 1)).Durum);
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER FailCost;");
        await purchases.KesinlestirAsync(f.Id, 1);
        Assert.Equal(10, (await Cost()).SeciliMaliyet);
    }

    [Theory] [InlineData("StokMaliyet")] [InlineData("AlisFatura")]
    public async Task Guncelleme_hatasi_onceki_maliyeti_ve_stogu_korur(string table)
    {
        await Buy(); var f = await purchases.CreateAsync(Request(100, 20));
        var sql = $"CREATE TRIGGER FailUpdate BEFORE UPDATE ON {table} BEGIN SELECT RAISE(ABORT, 'update test'); END;";
        await db.Database.ExecuteSqlRawAsync(sql);
        await Assert.ThrowsAsync<DbUpdateException>(() => purchases.KesinlestirAsync(f.Id, 1));
        db.ChangeTracker.Clear();
        Assert.Equal(10, (await Cost()).SonAlisMaliyeti); Assert.Equal(10, (await Cost()).AgirlikliOrtalamaMaliyet);
        Assert.Single(await db.StokHareketleri.ToListAsync());
        Assert.Equal(AlisFaturaDurumu.Taslak, (await purchases.GetByIdAsync(f.Id, 1)).Durum);
    }

    [Fact] public async Task Tekrar_kesinlestirme_maliyeti_ikinci_kez_degistirmez()
    {
        var f = await Buy(); var old = (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi;
        db.ChangeTracker.Clear();
        await purchases.KesinlestirAsync(f.Id, 1);
        Assert.Equal(old, (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi);
    }

    [Fact] public async Task Tenant_ve_sube_izolasyonu_okumada_korunur()
    {
        await Buy();
        var other = new Tenant { TenantId = Guid.NewGuid(), SubeId = 1, Ad = "Other", Kod = "T2" };
        db.Add(other); await db.SaveChangesAsync();
        var branch = new Sube { TenantId = other.TenantId, Ad = "Other", Kod = "S2" };
        db.Add(branch); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => costs.GetAsync(branch.Id, 1, stock.Id, null));
        var localBranch = new Sube { TenantId = tenant, Ad = "Local2", Kod = "L2" };
        db.Add(localBranch); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => costs.GetAsync(localBranch.Id, 1, stock.Id, null));
    }

    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task Pasif_ve_silinmis_depo_maliyet_okuyamaz(bool deleted)
    {
        await Buy();
        var d = await db.Depolar.SingleAsync();
        if (deleted) d.SilindiMi = true; else d.AktifMi = false;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Cost());
    }

    [Fact] public async Task Gecersiz_yontem_reddedilir_ayarlar_korunur()
    {
        await Assert.ThrowsAsync<UygulamaHatasi>(() => costs.YontemDegistirAsync(1, (MaliyetYontemi)99));
        Assert.Equal(MaliyetYontemi.AgirlikliOrtalama, (await db.TenantAyarlari.SingleAsync()).MaliyetYontemi);
    }

    [Fact] public async Task Devir_sayim_transfer_maliyet_uretmez_ve_degistirmez()
    {
        var depo = new Depo { TenantId = tenant, SubeId = 1, DepoKodu = "D2", Ad = "D2" };
        db.Add(depo); await db.SaveChangesAsync();
        var ledger = new StokServisi(db, new IslemLogServisi(db));
        StokFisOlusturRequest r = new() { SubeId = 1, DepoId = 1,
            Detaylar = new() { new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = 100 } } };
        await ledger.DevirAsync(r);
        Assert.Empty(await db.StokMaliyetleri.ToListAsync());
        // Positive opening stock has no assigned cost in this scope and contributes zero.
        await Buy(); Assert.Equal(5, (await Cost()).AgirlikliOrtalamaMaliyet);
        var before = (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi;
        r.Detaylar[0].Miktar = 50; await ledger.SayimAsync(r);
        r.DepoId = null; r.KaynakDepoId = 1; r.HedefDepoId = depo.Id; r.Detaylar[0].Miktar = 10;
        await ledger.TransferAsync(r);
        Assert.Single(await db.StokMaliyetleri.ToListAsync());
        Assert.Equal(before, (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi);
        Assert.Equal(5, (await Cost()).AgirlikliOrtalamaMaliyet);
        Assert.Equal(0, (await Cost(depo.Id)).SeciliMaliyet);
    }

    [Fact] public async Task Maliyet_alti_hane_hassasiyetini_korur()
    {
        var f = await Buy(Request(3, 0.3333m));
        Assert.Equal(1, f.ToplamMatrah);
        Assert.Equal(0.333333m, (await Cost()).SonAlisMaliyeti);
        var property = db.Model.FindEntityType(typeof(StokMaliyet))!.FindProperty(nameof(StokMaliyet.SonAlisMaliyeti))!;
        Assert.Equal(18, property.GetPrecision()); Assert.Equal(6, property.GetScale());
        var line = db.Model.FindEntityType(typeof(FaturaDetay))!.FindProperty(nameof(FaturaDetay.BirimMaliyet))!;
        Assert.Equal(18, line.GetPrecision()); Assert.Equal(6, line.GetScale());
    }

    [Fact] public async Task Maliyet_siniri_asilirsa_alis_kesinlesmez()
    {
        var f = await purchases.CreateAsync(Request(1, 9999999999999m));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => purchases.KesinlestirAsync(f.Id, 1));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.StokMaliyetleri.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Equal(AlisFaturaDurumu.Taslak, (await purchases.GetByIdAsync(f.Id, 1)).Durum);
    }

    [Fact] public async Task Ayni_stok_farkli_alis_birimlerinde_tek_efektif_maliyet_alir()
    {
        var koli = new StokKartSatisBirimi { TenantId = tenant, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "KOLI", BirimAdi = "Koli", Katsayi = 24 };
        db.Add(koli); await db.SaveChangesAsync();
        var r = Request(24, 10);
        r.Detaylar.Add(new() { StokKartId = stock.Id, StokKartSatisBirimiId = koli.Id, Miktar = 1, BirimFiyat = 288 });
        await Buy(r);
        Assert.Equal(11, (await Cost()).SonAlisMaliyeti);
        Assert.Equal(11, (await Cost()).AgirlikliOrtalamaMaliyet);
    }

    [Fact] public async Task Maliyet_snapshoti_siparisin_eski_katsayisini_kullanir()
    {
        await Buy();
        unit.Katsayi = 24; await db.SaveChangesAsync();
        var orders = new SiparisServisi(db);
        var order = await orders.SiparisOlusturAsync(new() { SubeId = 1, Kur = 1, ParaBirimKodu = "TRY" });
        await orders.KayitliFiyatlaSatirEkleAsync(db, order.Id, new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = 2, BirimFiyat = 300 });
        unit.Katsayi = 30; await db.SaveChangesAsync();
        var invoice = await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new() { SiparisId = order.Id });
        Assert.Equal(240, Assert.Single(invoice.Detaylar).BirimMaliyet);
    }

    [Fact] public async Task Yontem_degistirmek_maliyet_kaydini_ve_kdv_ayarlarini_degistirmez()
    {
        await Buy(); var before = (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi;
        await costs.YontemDegistirAsync(1, MaliyetYontemi.SonAlis);
        Assert.Equal(before, (await db.StokMaliyetleri.SingleAsync()).SonGuncellemeTarihi);
        var settings = await db.TenantAyarlari.SingleAsync();
        Assert.True(settings.SatisFiyatlariKdvDahilMi); Assert.False(settings.AlisFiyatlariKdvDahilMi);
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
