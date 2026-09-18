using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Product;

namespace PanoPos.Tests.Product;

public sealed class SatisStokCozumTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly SatisStokCozumServisi service;
    private readonly StokKart stock;
    private readonly StokKartSatisBirimi unit, box;
    private readonly StokKartVaryant variant;
    private readonly Guid tenant = SystemSeedData.TenantGuid;

    private sealed class Context(Guid tenant) : IIslemBaglami
    {
        public bool Dogrulandi => true;
        public Guid TenantId => tenant;
        public long SubeId => 1;
        public long KullaniciId => 1;
        public long CihazId => 1;
        public long KullaniciOturumId => 1;
    }

    public SatisStokCozumTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options, new Context(tenant));
        db.Database.EnsureCreated();
        stock = new() { TenantId = tenant, SubeId = 1, Ad = "Su", KdvId = 4 };
        db.Add(stock); db.SaveChanges();
        unit = new() { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true };
        box = new() { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, BirimKodu = "KOLI", BirimAdi = "Koli", Katsayi = 24 };
        var color = new Renk { TenantId = tenant, SubeId = 1, Kod = "M", Ad = "Mavi" };
        db.AddRange(unit, box, color); db.SaveChanges();
        variant = new() { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, VaryantKodu = "MAVI", RenkId = color.Id };
        db.Add(variant); db.SaveChanges();
        db.AddRange(
            new Barkod { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id, BarkodNo = "AD" },
            new Barkod { TenantId = tenant, SubeId = 1, StokKartId = stock.Id, StokKartSatisBirimiId = box.Id, BarkodNo = "KOLI" },
            new Barkod { TenantId = tenant, SubeId = 1, StokKartVaryantId = variant.Id, StokKartSatisBirimiId = box.Id, BarkodNo = "VARYANT" });
        foreach (var b in new[] { unit, box })
            foreach (var p in new[] { (1L, 100m), (2L, 105m), (3L, 90m) })
                db.Add(new StokKartFiyat { TenantId = tenant, SubeId = 1, StokKartSatisBirimiId = b.Id, FiyatTipiId = p.Item1, Fiyat = p.Item2, ParaBirimKodu = "TRY" });
        db.SaveChanges();
        service = new(db);
    }

    [Theory] [InlineData("AD", 1)] [InlineData("KOLI", 24)] [InlineData("VARYANT", 24)]
    public async Task Barkod_stok_birim_varyant_ve_kdv_cozer(string barcode, decimal coefficient)
    {
        var r = await service.BarkodCozAsync(barcode, 1);
        Assert.Equal(stock.Id, r.StokKartId); Assert.Equal("Su", r.StokKartAdi);
        Assert.Equal(coefficient, r.Katsayi); Assert.Equal(barcode, r.BarkodNo);
        Assert.Equal(barcode == "AD" ? unit.Id : box.Id, r.StokKartSatisBirimiId);
        Assert.Equal(4, r.KdvId); Assert.Equal(20, r.KdvOrani);
        Assert.Equal(barcode == "VARYANT" ? variant.Id : (long?)null, r.StokKartVaryantId);
    }

    [Theory] [InlineData(1, 100)] [InlineData(2, 105)] [InlineData(3, 90)]
    public async Task Secilen_fiyat_tipi_barkod_ve_manuel_cozumde_ayni(long type, decimal price)
    {
        var b = await service.BarkodCozAsync("AD", type);
        var m = await service.FiyatCozAsync(unit.Id, type);
        Assert.Equal(price, b.Fiyat); Assert.Equal(price, m.Fiyat);
        Assert.Equal(type, b.FiyatTipiId); Assert.Equal("TRY", b.FiyatParaBirimKodu);
    }

    [Fact] public async Task Fiyat_bulunamazsa_baska_tipe_gecmez()
    {
        var price = await Price(); price.AktifMi = false; await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync("AD", 1));
        Assert.Equal("price_not_found", ex.ErrorCode);
    }

    [Fact] public async Task Fiyat_tipi_zorunlu()
    {
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync("AD", 0));
        Assert.Equal("price_type_required", ex.ErrorCode);
    }

    [Fact] public async Task Barkod_birimi_eksikse_varsayilan_tahmin_edilmez()
    {
        (await db.Barkodlar.SingleAsync(x => x.BarkodNo == "AD")).StokKartSatisBirimiId = null;
        await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync("AD", 1));
        Assert.Equal("sales_unit_required", ex.ErrorCode);
    }

    [Theory]
    [InlineData("barkod", false)] [InlineData("barkod", true)]
    [InlineData("stok", false)] [InlineData("stok", true)]
    [InlineData("varyant", false)] [InlineData("varyant", true)]
    [InlineData("birim", false)] [InlineData("birim", true)]
    [InlineData("tip", false)] [InlineData("tip", true)]
    [InlineData("fiyat", false)] [InlineData("fiyat", true)]
    [InlineData("kdv", false)] [InlineData("kdv", true)]
    public async Task Pasif_veya_silinmis_master_kullanilamaz(string name, bool deleted)
    {
        PanoPos.Domain.Common.BaseEntity entity = name switch {
            "barkod" => await db.Barkodlar.SingleAsync(x => x.BarkodNo == "VARYANT"),
            "stok" => stock, "varyant" => variant, "birim" => box,
            "tip" => await db.FiyatTipleri.SingleAsync(x => x.Id == 1),
            "fiyat" => await db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == box.Id && x.FiyatTipiId == 1),
            _ => await db.Kdvler.SingleAsync(x => x.Id == 4) };
        if (deleted) entity.SilindiMi = true; else entity.AktifMi = false;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync("VARYANT", 1));
    }

    [Theory] [InlineData("barkod")] [InlineData("stok")] [InlineData("varyant")]
    [InlineData("birim")] [InlineData("tip")] [InlineData("fiyat")] [InlineData("kdv")]
    public async Task Her_master_iliski_tenant_ile_sinirlanir(string name)
    {
        PanoPos.Domain.Common.BaseEntity entity = name switch {
            "barkod" => await db.Barkodlar.SingleAsync(x => x.BarkodNo == "VARYANT"),
            "stok" => stock, "varyant" => variant, "birim" => box,
            "tip" => await db.FiyatTipleri.SingleAsync(x => x.Id == 1),
            "fiyat" => await db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == box.Id && x.FiyatTipiId == 1),
            _ => await db.Kdvler.SingleAsync(x => x.Id == 4) };
        // Simulate pre-existing inconsistent relations without bypassing the read context.
        await using var setup = new PanoPosDbContext(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        var foreign = (PanoPos.Domain.Common.BaseEntity)(await setup.FindAsync(entity.GetType(), entity.Id))!;
        var otherTenant = Guid.NewGuid();
        setup.Add(new Tenant { TenantId = otherTenant, SubeId = 1, Kod = "OTHER", Ad = "Other tenant" });
        await setup.SaveChangesAsync();
        foreign.TenantId = otherTenant;
        await setup.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync("VARYANT", 1));
    }

    private Task<StokKartFiyat> Price() => db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == unit.Id && x.FiyatTipiId == 1);

    private async Task<SiparisDetay> Line(string documentCurrency = "TRY", decimal? rate = null, decimal suppliedPrice = 1, long? type = 1)
    {
        var orders = new SiparisServisi(db);
        var order = await orders.SiparisOlusturAsync(new() { SubeId = 1, SiparisTipi = SiparisTipi.HizliSatisBekleyen, ParaBirimKodu = documentCurrency, Kur = 42.5m });
        await orders.SiparisSatirEkleAsync(order.Id, new() { StokKartId = stock.Id, StokKartSatisBirimiId = unit.Id,
            FiyatTipiId = type, FiyatKur = rate, Miktar = 1, BirimFiyat = suppliedPrice, FiyatParaBirimKodu = "FAKE" });
        return await db.SiparisDetaylari.SingleAsync(x => x.SiparisId == order.Id);
    }

    [Theory] [InlineData("TRY", "TRY", 42.5, 10, 1)] [InlineData("USD", "TRY", 42.5, 425, 42.5)]
    [InlineData("USD", "USD", 42.5, 10, 1)] [InlineData("TRY", "USD", 0.02, 0.2, 0.02)]
    [InlineData(" jpy ", " try ", 0.3, 3, 0.3)]
    public async Task Kur_yalniz_farkli_para_biriminde_belge_fiyatina_uygulanir(string source, string target, decimal rate, decimal expected, decimal snapshotRate)
    {
        var price = await Price(); price.Fiyat = 10; price.ParaBirimKodu = source; await db.SaveChangesAsync();
        var line = await Line(target, rate);
        Assert.Equal(expected, line.BirimFiyat); Assert.Equal(snapshotRate, line.FiyatKur);
        Assert.Equal(source.Trim().ToUpperInvariant(), line.FiyatParaBirimKodu);
        Assert.Equal(1, line.FiyatTipiId);
    }

    [Theory] [InlineData(null)] [InlineData(0)] [InlineData(-1)]
    public async Task Farkli_para_biriminde_kur_zorunlu(int? rate)
    {
        (await Price()).ParaBirimKodu = "USD"; await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => Line(rate: rate));
        Assert.Equal("price_currency_invalid", ex.ErrorCode); Assert.Empty(await db.SiparisDetaylari.ToListAsync());
    }

    [Theory] [InlineData(0)] [InlineData(999999)] [InlineData(-1)]
    public async Task Istemci_fiyati_ve_para_birimi_masteri_degistiremez(decimal supplied)
    {
        var line = await Line(suppliedPrice: supplied);
        Assert.Equal(100, line.BirimFiyat); Assert.Equal("TRY", line.FiyatParaBirimKodu);
    }

    [Fact] public async Task Siparis_fiyat_tipi_olmadan_istemci_fiyatina_donmez()
    {
        await Assert.ThrowsAsync<UygulamaHatasi>(() => Line(type: null));
        Assert.Empty(await db.SiparisDetaylari.ToListAsync());
    }

    [Fact] public async Task Dort_ondalik_away_from_zero_yuvarlanir()
    {
        var p = await Price(); p.Fiyat = 1.2345m; p.ParaBirimKodu = "USD"; await db.SaveChangesAsync();
        var line = await Line(rate: 1.000041m);
        Assert.Equal(1.2346m, line.BirimFiyat);
    }

    [Fact] public async Task Master_degisse_de_siparis_fatura_snapshotlari_korunur()
    {
        var p = await Price(); p.Fiyat = 10; p.ParaBirimKodu = "USD"; await db.SaveChangesAsync();
        var line = await Line(rate: 42.5m);
        var typeName = line.FiyatTipiAdi;
        p.Fiyat = 999; p.ParaBirimKodu = "EUR";
        (await db.FiyatTipleri.SingleAsync(x => x.Id == 1)).Ad = "Changed";
        (await db.Kdvler.SingleAsync(x => x.Id == 4)).Oran = 25;
        await db.SaveChangesAsync();
        var f = await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new() { SiparisId = line.SiparisId });
        var d = Assert.Single(f.Detaylar);
        Assert.Equal(425, d.BirimFiyat); Assert.Equal("USD", d.FiyatParaBirimKodu); Assert.Equal(42.5m, d.FiyatKur);
        Assert.Equal(1, d.FiyatTipiId); Assert.Equal(typeName, d.FiyatTipiAdi); Assert.Equal(20, d.KdvOrani);
        Assert.Equal(line.BirimKatsayi, d.BirimKatsayi); Assert.Equal(line.StokKartSatisBirimiId, d.StokKartSatisBirimiId);
    }

    [Fact] public async Task Listeler_sayfali_ve_aktif_kayitlarla_sinirlidir()
    {
        (await db.FiyatTipleri.SingleAsync(x => x.Id == 3)).AktifMi = false;
        box.SilindiMi = true; await db.SaveChangesAsync();
        var types = await service.FiyatTipleriAsync(1, 1);
        Assert.Equal(2, types.ToplamKayit); Assert.Single(types.Kayitlar);
        var units = await service.SatisBirimleriAsync(stock.Id, 1, 20);
        Assert.Equal(unit.Id, Assert.Single(units.Kayitlar).Id);
    }

    [Theory] [InlineData(0, 1)] [InlineData(1, 0)] [InlineData(1, 201)]
    public async Task Liste_sinirlarina_uyulur(int page, int size)
        => await Assert.ThrowsAsync<UygulamaHatasi>(() => service.FiyatTipleriAsync(page, size));

    [Fact] public async Task Kategori_grup_arama_ve_pagination_birlikte_calisir()
    {
        var category = new StokKategori { TenantId = tenant, SubeId = 1, Kod = "ICE", Ad = "Icecek" };
        var group = new StokGrup { TenantId = tenant, SubeId = 1, Kod = "SU", Ad = "Su" };
        db.AddRange(category, group); await db.SaveChangesAsync();
        stock.StokKategoriId = category.Id; stock.StokGrupId = group.Id;
        db.Add(new StokKart { TenantId = tenant, SubeId = 1, Ad = "Su 2", KdvId = 4, StokKategoriId = category.Id, StokGrupId = group.Id });
        await db.SaveChangesAsync();
        var catalog = new StokKartServisi(db);
        var first = await catalog.StokKartListeleAsync("Su", 1, 1, kategoriId: category.Id, grupId: group.Id, aktifMi: true);
        var second = await catalog.StokKartListeleAsync("Su", 2, 1, kategoriId: category.Id, grupId: group.Id, aktifMi: true);
        Assert.Equal(2, first.ToplamKayit); Assert.Single(first.Kayitlar); Assert.Single(second.Kayitlar);
        Assert.NotEqual(first.Kayitlar[0].Id, second.Kayitlar[0].Id);
        Assert.Empty((await catalog.StokKartListeleAsync(null, 1, 20, kategoriId: 999)).Kayitlar);
        Assert.Empty((await catalog.StokKartListeleAsync(null, 1, 20, grupId: 999)).Kayitlar);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Baska_stok_birimi_barkod_ve_varyantta_kullanilamaz(bool variantBarcode)
    {
        var other = new StokKart { TenantId = tenant, SubeId = 1, Ad = "Other", KdvId = 4 };
        db.Add(other); await db.SaveChangesAsync();
        box.StokKartId = other.Id; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BarkodCozAsync(variantBarcode ? "VARYANT" : "KOLI", 1));
    }

    [Theory] [InlineData(OdemeTipi.Nakit)] [InlineData(OdemeTipi.KrediKarti)] [InlineData(OdemeTipi.Veresiye)]
    public async Task Odeme_tipi_fiyat_snapshotini_degistirmez(OdemeTipi payment)
    {
        var line = await Line(type: 2);
        var kasa = new Kasa { TenantId = tenant, SubeId = 1, Ad = "Kasa" };
        var banka = new Banka { TenantId = tenant, SubeId = 1, Kod = "BNK", Ad = "Banka" };
        var cari = new CariKart { TenantId = tenant, SubeId = 1, CariKodu = "C", Ad = "Cari", Tip = CariTipi.Alici };
        db.AddRange(kasa, banka, cari); await db.SaveChangesAsync();
        (await db.Siparisler.SingleAsync(x => x.Id == line.SiparisId)).CariId = cari.Id;
        await db.SaveChangesAsync();
        var invoice = await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new() { SiparisId = line.SiparisId });
        await new PanoPos.Infrastructure.Payment.TahsilatServisi(db).TahsilatOlusturAsync(new() { IslemAnahtari = Guid.NewGuid(),
            SubeId = 1, FaturaId = invoice.Id, OdemeTipi = payment, Tutar = invoice.NetToplam,
            ParaBirimKodu = "TRY", Kur = 1, KullaniciId = 1, CihazId = 1,
            KasaId = kasa.Id, BankaId = banka.Id });
        var result = await new FaturaServisi(db).FaturaGetirAsync(invoice.Id);
        Assert.Equal(105, result.Detaylar[0].BirimFiyat); Assert.Equal(2, result.Detaylar[0].FiyatTipiId);
    }

    [Theory] [InlineData(null)] [InlineData(0)] [InlineData(-1)]
    public async Task Ayni_para_biriminde_bos_sifir_negatif_girilen_kur_bire_sabitlenir(int? rate)
    {
        var line = await Line(rate: rate);
        Assert.Equal(1, line.FiyatKur); Assert.Equal(100, line.BirimFiyat);
    }

    [Fact] public async Task Sipariste_fiyat_yoksa_lookup_gibi_acik_hata_ve_sifir_satir()
    {
        (await Price()).AktifMi = false; await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => Line(suppliedPrice: 100));
        Assert.Equal("price_not_found", ex.ErrorCode);
        Assert.Empty(await db.SiparisDetaylari.ToListAsync());
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
