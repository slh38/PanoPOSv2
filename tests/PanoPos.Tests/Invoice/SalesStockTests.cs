using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Payment;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Invoice;

public sealed class SalesStockTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly SiparisServisi orders;
    private readonly FaturaServisi invoices;
    private readonly StokKart stock;
    private readonly StokKartSatisBirimi unit;
    private readonly Guid tenant = SystemSeedData.TenantGuid;

    public SalesStockTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        orders = new(db);
        invoices = new(db, new OutboxServisi(db));
        stock = new() { TenantId = tenant, SubeId = 1, Ad = "Test", KdvId = 4 };
        db.Add(stock); db.SaveChanges();
        unit = new() { TenantId = tenant, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true };
        db.Add(unit); db.SaveChanges();
    }

    private async Task<long> Order(decimal quantity = 1, decimal price = 100, long? variant = null, long? adisyon = null, long? cari = null)
    {
        var o = await orders.SiparisOlusturAsync(new() { SubeId = 1, Kur = 1, ParaBirimKodu = "TRY",
            SiparisTipi = adisyon.HasValue ? SiparisTipi.Masa : SiparisTipi.HizliSatisBekleyen, AdisyonId = adisyon, CariId = cari });
        await orders.KayitliFiyatlaSatirEkleAsync(db, o.Id, new() { StokKartId = stock.Id, StokKartVaryantId = variant,
            StokKartSatisBirimiId = unit.Id, Miktar = quantity, BirimFiyat = price });
        return o.Id;
    }

    private Task<FaturaDto> Invoice(long id, long? depo = null) =>
        invoices.SiparistenFaturaOlusturAsync(new() { SiparisId = id, DepoId = depo });

    [Fact] public async Task Bos_siparis_stok_uretmez()
    {
        await orders.SiparisOlusturAsync(new() { SubeId = 1, Kur = 1, ParaBirimKodu = "TRY" });
        Assert.Empty(await db.StokFisleri.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Fact] public async Task Bekleyen_siparis_ve_satirlari_stok_uretmez()
    {
        var id = await Order(10);
        Assert.Equal(SiparisDurumu.Bekliyor, (await orders.SiparisGetirAsync(id)).Durum);
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Theory]
    [InlineData(10, 1, -10)]
    [InlineData(2, 24, -48)]
    [InlineData(5, 1, -5)]
    public async Task Fatura_snapshot_miktari_negatif_ledger_olur(decimal qty, decimal coefficient, decimal expected)
    {
        unit.Katsayi = coefficient; await db.SaveChangesAsync();
        var f = await Invoice(await Order(qty));
        var fis = await db.StokFisleri.Include(x => x.Detaylar).SingleAsync();
        var movement = await db.StokHareketleri.SingleAsync();
        Assert.Equal(f.Id, fis.FaturaId); Assert.Null(fis.AlisFaturaId);
        Assert.Equal(StokFisTipi.Satis, fis.StokFisTipi);
        Assert.Equal(StokHareketTipi.Satis, movement.StokHareketTipi);
        Assert.Equal(expected, movement.Miktar); Assert.Equal(1, movement.DepoId);
        Assert.Equal(1, f.DepoId); Assert.Null(fis.KaynakDepoId); Assert.Null(fis.HedefDepoId);
        Assert.Equal(coefficient, Assert.Single(fis.Detaylar).Katsayi);
        Assert.Empty(await db.Tahsilatlar.ToListAsync());
        Assert.Single(await db.OutboxOlaylari.Where(x => x.KaynakTablo == "Fatura").ToListAsync());
        Assert.Single(await db.IslemLoglari.Where(x => x.ModulAdi == "Fatura").ToListAsync());
    }

    [Fact] public async Task Acikca_secilen_depo_kullanilir()
    {
        var depo = new Depo { TenantId = tenant, SubeId = 1, DepoKodu = "IKINCI", Ad = "Ikinci" };
        db.Add(depo); await db.SaveChangesAsync();
        var f = await Invoice(await Order(), depo.Id);
        Assert.Equal(depo.Id, f.DepoId);
        Assert.Equal(depo.Id, (await db.StokHareketleri.SingleAsync()).DepoId);
    }

    [Fact] public async Task Varsayilan_depo_yoksa_fatura_olusmaz()
    {
        (await db.Depolar.SingleAsync()).VarsayilanMi = false; await db.SaveChangesAsync();
        var id = await Order();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Invoice(id));
        Assert.Empty(await db.Faturalar.ToListAsync());
    }

    [Theory] [InlineData("inactive")] [InlineData("deleted")] [InlineData("tenant")] [InlineData("branch")]
    public async Task Gecersiz_depo_reddedilir(string reason)
    {
        var id = await Order();
        var depo = await db.Depolar.SingleAsync();
        if (reason == "inactive") depo.AktifMi = false;
        if (reason == "deleted") depo.SilindiMi = true;
        if (reason == "tenant")
        {
            var t = new Tenant { TenantId = Guid.NewGuid(), SubeId = 1, Ad = "Other" };
            db.Add(t); await db.SaveChangesAsync(); depo.TenantId = t.TenantId;
        }
        if (reason == "branch")
        {
            var branch = new Sube { TenantId = tenant, Ad = "Other", Kod = "OTHER" };
            db.Add(branch); await db.SaveChangesAsync(); depo.SubeId = branch.Id;
        }
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Invoice(id, depo.Id));
        Assert.Empty(await db.Faturalar.ToListAsync());
    }

    [Fact] public async Task Tekrar_donusum_ikinci_stok_uretmez()
    {
        var id = await Order(10); await Invoice(id);
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Invoice(id));
        Assert.Single(await db.StokFisleri.ToListAsync());
        Assert.Equal(-10, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Fact] public async Task Database_ikinci_fatura_stok_fisini_engeller()
    {
        var f = await Invoice(await Order());
        db.Add(new StokFis { TenantId = tenant, SubeId = 1, DepoId = 1, FaturaId = f.Id,
            FisNo = "DUPLICATE", FisTarihi = DateTime.UtcNow, StokFisTipi = StokFisTipi.Satis });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact] public async Task Master_birim_degisse_de_snapshot_korunur()
    {
        unit.Katsayi = 24; unit.BirimKodu = "KOLI"; unit.BirimAdi = "Koli"; await db.SaveChangesAsync();
        var id = await Order(2);
        unit.Katsayi = 30; unit.BirimKodu = "YENI"; unit.BirimAdi = "Yeni"; await db.SaveChangesAsync();
        await Invoice(id);
        var line = await db.StokFisDetaylari.SingleAsync();
        Assert.Equal(24, line.Katsayi); Assert.Equal("KOLI", line.BirimKodu); Assert.Equal("Koli", line.BirimAdi);
        Assert.Equal(-48, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Fact] public async Task Varyant_ve_null_stok_birbirine_karismaz()
    {
        var color = new Renk { TenantId = tenant, SubeId = 1, Ad = "Mavi", Kod = "MAVI" };
        db.Add(color); await db.SaveChangesAsync();
        var variant = new StokKartVaryant { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, VaryantKodu = "V1", RenkId = color.Id };
        db.Add(variant); await db.SaveChangesAsync();
        var id = await Order(2, variant: variant.Id);
        await orders.KayitliFiyatlaSatirEkleAsync(db, id, new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, Miktar = 3, BirimFiyat = 100 });
        await Invoice(id);
        var movements = await db.StokHareketleri.ToListAsync();
        Assert.Equal(-2, movements.Single(x => x.StokKartVaryantId == variant.Id).Miktar);
        Assert.Equal(-3, movements.Single(x => x.StokKartVaryantId == null).Miktar);
    }

    [Theory] [InlineData(true, 120)] [InlineData(false, 100)] [InlineData(true, 105)] [InlineData(true, 90)]
    public async Task Kdv_ve_fiyat_stok_miktarini_etkilemez(bool included, decimal price)
    {
        (await db.TenantAyarlari.SingleAsync()).SatisFiyatlariKdvDahilMi = included;
        await db.SaveChangesAsync();
        await Invoice(await Order(1, price));
        Assert.Equal(-1, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Theory] [InlineData("StokFis")] [InlineData("StokFisDetay")] [InlineData("StokHareket")]
    [InlineData("IslemLog")] [InlineData("OutboxOlay")] [InlineData("FaturaDetay")]
    public async Task Her_asamada_hata_tum_satisi_rollback_yapar(string table)
    {
        var id = await Order();
        // Table names come only from the fixed InlineData values, not external input.
        var sql = $"CREATE TRIGGER FailSale BEFORE INSERT ON {table} BEGIN SELECT RAISE(ABORT, 'sale failure'); END;";
        await db.Database.ExecuteSqlRawAsync(sql);
        await Assert.ThrowsAnyAsync<Exception>(() => Invoice(id));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Faturalar.ToListAsync()); Assert.Empty(await db.FaturaDetaylari.ToListAsync());
        Assert.Empty(await db.StokFisleri.ToListAsync()); Assert.Empty(await db.StokFisDetaylari.ToListAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Empty(await db.IslemLoglari.Where(x => x.ModulAdi == "Fatura").ToListAsync());
        Assert.Empty(await db.OutboxOlaylari.ToListAsync());
        Assert.Equal(SiparisDurumu.Bekliyor, (await orders.SiparisGetirAsync(id)).Durum);
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER FailSale;");
        await Invoice(id);
        Assert.Single(await db.StokHareketleri.ToListAsync());
    }

    [Fact] public async Task Faturasiz_siparis_iptali_stok_uretmez()
    {
        await orders.SiparisIptalAsync(await Order());
        Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Theory] [InlineData("depo")] [InlineData("miktar")] [InlineData("katsayi")] [InlineData("delete")]
    public async Task Stoklanmis_fatura_normal_yazma_ile_degistirilemez(string field)
    {
        var f = await Invoice(await Order());
        var invoice = await db.Faturalar.SingleAsync();
        var line = await db.FaturaDetaylari.SingleAsync();
        if (field == "depo") invoice.DepoId = 999;
        if (field == "miktar") line.Miktar = 2;
        if (field == "katsayi") line.BirimKatsayi = 24;
        if (field == "delete") invoice.SilindiMi = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Equal(1, (await invoices.FaturaGetirAsync(f.Id)).DepoId);
        Assert.Equal(-1, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Kismi_ve_parcali_tahsilat_stok_uretmez(bool complete)
    {
        var f = await Invoice(await Order(10));
        var kasa = new Kasa { TenantId = tenant, SubeId = 1, Ad = "Kasa" };
        var banka = new Banka { TenantId = tenant, SubeId = 1, Ad = "Banka", Kod = "B01" };
        db.AddRange(kasa, banka); await db.SaveChangesAsync();
        var payments = new TahsilatServisi(db);
        await payments.TahsilatOlusturAsync(new() { IslemAnahtari = Guid.NewGuid(), SubeId = 1, FaturaId = f.Id, OdemeTipi = OdemeTipi.Nakit,
            KasaId = kasa.Id, KullaniciId = 1, CihazId = 1, Tutar = 600, ParaBirimKodu = "TRY", Kur = 1 });
        if (complete)
            await payments.TahsilatOlusturAsync(new() { IslemAnahtari = Guid.NewGuid(), SubeId = 1, FaturaId = f.Id, OdemeTipi = OdemeTipi.KrediKarti,
                BankaId = banka.Id, KullaniciId = 1, CihazId = 1, Tutar = 400, ParaBirimKodu = "TRY", Kur = 1 });
        Assert.Single(await db.StokFisleri.ToListAsync());
        Assert.Equal(-10, (await db.StokHareketleri.SingleAsync()).Miktar);
        Assert.Equal(complete ? 0 : 400, (await invoices.FaturaGetirAsync(f.Id)).KalanTutar);
    }

    [Fact] public async Task Restoran_acik_adisyon_stok_uretmez_fatura_uretir()
    {
        var masa = new Masa { TenantId = tenant, SubeId = 1, Kod = "M1", Ad = "Masa", MasaDurumId = 1 };
        db.Add(masa); await db.SaveChangesAsync();
        var adisyon = new Adisyon { TenantId = tenant, SubeId = 1, MasaId = masa.Id,
            AcanKullaniciId = 1, AcanCihazId = 1, Durum = AdisyonDurumu.Acik, AcilisTarihi = DateTime.UtcNow };
        db.Add(adisyon); await db.SaveChangesAsync();
        var id = await Order(2, adisyon: adisyon.Id);
        Assert.Empty(await db.StokHareketleri.ToListAsync());
        await Invoice(id);
        Assert.Equal(-2, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Fact] public async Task Bozuk_coklu_varsayilan_depo_sessiz_secilmez()
    {
        await db.Database.ExecuteSqlRawAsync("DROP INDEX IX_Depo_TenantId_SubeId;");
        db.Add(new Depo { TenantId = tenant, SubeId = 1, DepoKodu = "OTHER", Ad = "Other", VarsayilanMi = true });
        await db.SaveChangesAsync();
        var id = await Order();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Invoice(id));
        Assert.Empty(await db.Faturalar.ToListAsync());
    }

    [Theory] [InlineData(0)] [InlineData(-1)]
    public async Task Gecersiz_snapshot_tam_rollback_yapar(decimal coefficient)
    {
        var id = await Order();
        (await db.SiparisDetaylari.SingleAsync()).BirimKatsayi = coefficient;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Invoice(id));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Faturalar.ToListAsync()); Assert.Empty(await db.StokHareketleri.ToListAsync());
    }

    [Fact] public async Task Stoklanmis_fatura_iptali_ters_hareket_gerektirir()
    {
        var f = await Invoice(await Order());
        var error = await Assert.ThrowsAsync<UygulamaHatasi>(() => invoices.FaturaIptalAsync(f.Id));
        Assert.Contains("ters stok", error.Message);
        Assert.Equal(FaturaDurumu.Acik, (await invoices.FaturaGetirAsync(f.Id)).Durum);
    }

    [Fact] public async Task Siparis_durum_guncelleme_hatasi_ledger_rollback_yapar()
    {
        var id = await Order();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER FailState BEFORE UPDATE ON Siparis BEGIN SELECT RAISE(ABORT, 'state failure'); END;");
        await Assert.ThrowsAsync<DbUpdateException>(() => Invoice(id));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Faturalar.ToListAsync()); Assert.Empty(await db.StokHareketleri.ToListAsync());
        Assert.Empty(await db.StokFisleri.ToListAsync()); Assert.Empty(await db.IslemLoglari.ToListAsync());
        Assert.Equal(SiparisDurumu.Bekliyor, (await orders.SiparisGetirAsync(id)).Durum);
    }

    [Theory] [InlineData(1, 100)] [InlineData(2, 105)] [InlineData(3, 90)]
    public async Task Farkli_fiyat_tipleri_ayni_stok_cikisini_uretir(long typeId, decimal price)
    {
        db.Add(new StokKartFiyat { TenantId = tenant, SubeId = 1, StokKartSatisBirimiId = unit.Id,
            FiyatTipiId = typeId, Fiyat = price, ParaBirimKodu = "TRY" });
        await db.SaveChangesAsync();
        var resolved = await db.StokKartFiyatlari.SingleAsync(x => x.FiyatTipiId == typeId);
        await Invoice(await Order(1, resolved.Fiyat));
        Assert.Equal(-1, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Fact] public async Task Veresiye_stok_hareketini_cogaltmaz()
    {
        var cari = new CariKart { TenantId = tenant, SubeId = 1, CariKodu = "A1", Ad = "Alici", Tip = CariTipi.Alici };
        db.Add(cari); await db.SaveChangesAsync();
        var f = await Invoice(await Order(cari: cari.Id));
        await new TahsilatServisi(db).TahsilatOlusturAsync(new() { IslemAnahtari = Guid.NewGuid(), SubeId = 1, FaturaId = f.Id,
            OdemeTipi = OdemeTipi.Veresiye, KullaniciId = 1, CihazId = 1, Tutar = f.NetToplam, ParaBirimKodu = "TRY", Kur = 1 });
        Assert.Single(await db.StokFisleri.ToListAsync());
        Assert.Equal(-1, (await db.StokHareketleri.SingleAsync()).Miktar);
    }

    [Fact] public async Task Database_olmayan_fatura_baglantisini_engeller()
    {
        db.Add(new StokFis { TenantId = tenant, SubeId = 1, DepoId = 1, FaturaId = 999,
            FisNo = "INVALID", FisTarihi = DateTime.UtcNow, StokFisTipi = StokFisTipi.Satis });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
