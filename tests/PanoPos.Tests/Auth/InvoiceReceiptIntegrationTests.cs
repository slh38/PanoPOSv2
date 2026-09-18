using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PanoPos.Application.Invoice;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Swagger;

namespace PanoPos.Tests.Auth;

public sealed partial class SessionIntegrationTests
{
    private async Task<long> ReceiptInvoice(int lines = 1, bool variant = false, bool discount = false)
    {
        await Login();
        if (variant)
        {
            await using var db = new PanoPosDbContext(options);
            db.Add(new Renk { Id = 11, TenantId = tenantA, SubeId = 1, Kod = "MAVI", Ad = "Mavi" });
            db.Add(new StokKartVaryant { Id = 11, TenantId = tenantA, SubeId = 1, StokKartId = 10, RenkId = 11, VaryantKodu = "MAVI-M" });
            await db.SaveChangesAsync();
        }
        var order = await Post("/api/v1/siparis", new { SiparisTipi = 2, ParaBirimKodu = "TRY", Kur = 1, CariId = 10 });
        var orderId = order.GetProperty("id").GetInt64();
        for (var i = 0; i < lines; i++)
            await Post($"/api/v1/siparis/{orderId}/satir", new { StokKartId = 10, StokKartSatisBirimiId = 10,
                StokKartVaryantId = variant ? (long?)11 : null, FiyatTipiId = 1, Miktar = 1,
                IndirimTutari = discount ? 100 : 0 });
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = orderId, Aciklama = "Fis testi" });
        return invoice.GetProperty("id").GetInt64();
    }

    private async Task ReceiptPay(long invoice, decimal amount, OdemeTipi type)
        => await Post("/api/v1/tahsilat", new { IslemAnahtari = Guid.NewGuid(), FaturaId = invoice,
            OdemeTipi = type, Tutar = amount, ParaBirimKodu = "TRY", Kur = 1,
            KasaId = type == OdemeTipi.Nakit ? (long?)10 : null,
            BankaId = type == OdemeTipi.KrediKarti ? (long?)10 : null, Aciklama = "Odeme" });

    private async Task<FaturaDto> ReceiptRead(long id)
        => (await client.GetFromJsonAsync<FaturaDto>($"/api/v1/fatura/{id}"))!;

    [Fact]
    public async Task Fis_header_sube_cari_depo_tarih_ve_context_doner()
    {
        var id = await ReceiptInvoice();
        var result = await ReceiptRead(id);
        await using var db = new PanoPosDbContext(options);
        var entity = await db.Faturalar.SingleAsync(x => x.Id == id);
        Assert.Equal(entity.FaturaNo, result.FaturaNo);
        Assert.Equal(entity.OlusturmaTarihi, result.FaturaTarihi);
        Assert.Equal(tenantA, result.TenantId);
        Assert.Equal((await db.Set<Tenant>().SingleAsync(x => x.TenantId == tenantA)).Ad, result.TenantAdi);
        Assert.Equal(1, result.SubeId);
        Assert.Equal((await db.Subeler.SingleAsync(x => x.Id == 1)).Ad, result.SubeAdi);
        Assert.Equal(10, result.CariId); Assert.Equal("A", result.CariKodu); Assert.Equal("Customer A", result.CariAdi);
        Assert.Equal(1, result.DepoId); Assert.Equal("Merkez Depo", result.DepoAdi);
        var cashier = await db.Kullanicilar.SingleAsync(x => x.Id == 1);
        Assert.Equal(1, result.KasiyerId); Assert.Equal(cashier.Ad + " " + cashier.Soyad, result.KasiyerAdi);
        Assert.Equal(4, result.CihazId); Assert.Equal("POS4", result.CihazAdi);
        Assert.Equal("TRY", result.ParaBirimKodu); Assert.Equal(1, result.Kur);
        Assert.Equal("Fis testi", result.Aciklama);
    }

    [Fact]
    public async Task Fis_kasiyer_ve_cihaz_okuyan_oturumdan_degil_faturadan_gelir()
    {
        var id = await ReceiptInvoice();
        await using (var db = new PanoPosDbContext(options))
        {
            db.Add(new Kullanici { Id = 3, TenantId = tenantA, SubeId = 1, Ad = "Diger", Soyad = "Kasiyer",
                PinHash = new PinHashServisi().Hashle("7777") });
            db.Add(new KullaniciSube { TenantId = tenantA, SubeId = 1, KullaniciId = 3, BagliSubeId = 1 });
            await db.SaveChangesAsync();
        }
        await Login("7777", 1);
        var result = await ReceiptRead(id);
        Assert.Equal(1, result.KasiyerId); Assert.Equal(4, result.CihazId);
        Assert.NotEqual("Diger Kasiyer", result.KasiyerAdi); Assert.Equal("POS4", result.CihazAdi);
    }

    [Theory]
    [InlineData("ad")] [InlineData("fiyat")] [InlineData("kdv")]
    [InlineData("birim")] [InlineData("katsayi")] [InlineData("tip")]
    [InlineData("varyant")] [InlineData("silinmis")]
    public async Task Fis_ticari_snapshot_master_degisse_de_korunur(string field)
    {
        var id = await ReceiptInvoice(variant: true, discount: true);
        var before = Assert.Single((await ReceiptRead(id)).Detaylar);
        await using (var db = new PanoPosDbContext(options))
        {
            switch (field)
            {
                case "ad": (await db.StokKartler.SingleAsync(x => x.Id == 10)).Ad = "Degisen"; break;
                case "fiyat": (await db.StokKartFiyatlari.SingleAsync(x => x.Id == 10)).Fiyat = 9999; break;
                case "kdv": (await db.Set<Kdv>().SingleAsync(x => x.Id == 1)).Oran = 99; break;
                case "birim":
                    var unit = await db.StokKartSatisBirimleri.SingleAsync(x => x.Id == 10);
                    unit.BirimAdi = "Koli"; unit.BirimKodu = "KOLI"; break;
                case "katsayi": (await db.StokKartSatisBirimleri.SingleAsync(x => x.Id == 10)).Katsayi = 48; break;
                case "tip": (await db.FiyatTipleri.SingleAsync(x => x.Id == 1)).Ad = "Yeni tip"; break;
                case "varyant": (await db.StokKartVaryantlari.SingleAsync(x => x.Id == 11)).VaryantKodu = "DEGISEN"; break;
                case "silinmis":
                    (await db.StokKartler.SingleAsync(x => x.Id == 10)).SilindiMi = true;
                    (await db.StokKartVaryantlari.SingleAsync(x => x.Id == 11)).SilindiMi = true;
                    (await db.StokKartSatisBirimleri.SingleAsync(x => x.Id == 10)).SilindiMi = true; break;
            }
            await db.SaveChangesAsync();
        }
        var after = Assert.Single((await ReceiptRead(id)).Detaylar);
        Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(after));
        Assert.Equal("Stock A", after.StokKartAd); Assert.Equal("MAVI-M", after.VaryantKodu);
        Assert.Equal("ADET", after.BirimKodu); Assert.Equal("Adet", after.BirimAdi);
        Assert.Equal(1, after.BirimKatsayi); Assert.Equal(1000, after.BirimFiyat);
        Assert.Equal(100, after.IndirimTutari); Assert.Equal(900, after.SatirNetToplam);
        Assert.Equal(1, after.FiyatTipiId); Assert.False(string.IsNullOrWhiteSpace(after.FiyatTipiAdi));
        Assert.Equal("TRY", after.FiyatParaBirimKodu); Assert.Equal(1, after.FiyatKur);
    }

    [Fact]
    public async Task Fis_satir_ve_header_kdv_iskonto_snapshotlari_birebir_doner()
    {
        var id = await ReceiptInvoice(discount: true);
        var dto = await ReceiptRead(id);
        await using var db = new PanoPosDbContext(options);
        var f = await db.Faturalar.SingleAsync(x => x.Id == id);
        var d = await db.FaturaDetaylari.SingleAsync(x => x.FaturaId == id);
        var line = Assert.Single(dto.Detaylar);
        Assert.Equal(f.AraToplam, dto.AraToplam); Assert.Equal(f.GenelIndirimOrani, dto.GenelIndirimOrani);
        Assert.Equal(f.GenelIndirimTutari, dto.GenelIndirimTutari);
        Assert.Equal(f.ToplamMatrah, dto.ToplamMatrah); Assert.Equal(f.ToplamKdv, dto.ToplamKdv);
        Assert.Equal(f.NetToplam, dto.NetToplam);
        Assert.Equal(d.KdvOrani, line.KdvOrani); Assert.Equal(d.KdvDahilMi, line.KdvDahilMi);
        Assert.Equal(d.Matrah, line.Matrah); Assert.Equal(d.KdvTutari, line.KdvTutari);
        Assert.Equal(d.IndirimOrani, line.IndirimOrani); Assert.Equal(d.GenelIndirimPayi, line.GenelIndirimPayi);
        Assert.Equal(d.BirimMaliyet, line.BirimMaliyet); Assert.Equal(d.Miktar, line.Miktar);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Fis_altiyuz_nakit_dortyuz_kart_tek_response_ve_kalan(bool complete)
    {
        var id = await ReceiptInvoice();
        await ReceiptPay(id, 600, OdemeTipi.Nakit);
        if (complete) await ReceiptPay(id, 400, OdemeTipi.KrediKarti);
        var dto = await ReceiptRead(id);
        Assert.Equal(600, dto.NakitToplam); Assert.Equal(complete ? 400 : 0, dto.KartToplam);
        Assert.Equal(0, dto.VeresiyeToplam);
        Assert.Equal(dto.Odemeler.Sum(p => p.Tutar), dto.OdenenTutar);
        Assert.Equal(complete ? 0 : 400, dto.KalanTutar);
        Assert.Equal(complete ? FaturaDurumu.Kapali : FaturaDurumu.Acik, dto.Durum);
        var cash = Assert.Single(dto.Odemeler.Where(p => p.OdemeTipi == OdemeTipi.Nakit));
        Assert.Equal(10, cash.KasaId); Assert.Equal("A", cash.KasaAdi); Assert.Null(cash.BankaId);
        Assert.Equal("TRY", cash.ParaBirimKodu); Assert.Equal("Odeme", cash.Aciklama);
        Assert.NotEqual(default, cash.Tarih);
        if (complete)
        {
            var card = Assert.Single(dto.Odemeler.Where(p => p.OdemeTipi == OdemeTipi.KrediKarti));
            Assert.Equal(10, card.BankaId); Assert.Equal("A", card.BankaAdi); Assert.Null(card.KasaId);
        }
        Assert.Equal(1, Assert.Single(dto.Detaylar).FiyatTipiId);
        var json = await client.GetStringAsync($"/api/v1/fatura/{id}");
        Assert.DoesNotContain("islemAnahtari", json); Assert.DoesNotContain("istekOzeti", json);
    }

    [Fact]
    public async Task Fis_uc_odeme_tipi_toplamlari_ve_cari_baglantisi()
    {
        var id = await ReceiptInvoice();
        await ReceiptPay(id, 200, OdemeTipi.Nakit);
        await ReceiptPay(id, 300, OdemeTipi.KrediKarti);
        await ReceiptPay(id, 500, OdemeTipi.Veresiye);
        var dto = await ReceiptRead(id);
        Assert.Equal(200, dto.NakitToplam); Assert.Equal(300, dto.KartToplam); Assert.Equal(500, dto.VeresiyeToplam);
        Assert.Equal(1000, dto.OdenenTutar); Assert.Equal(0, dto.KalanTutar);
        Assert.Equal(10, Assert.Single(dto.Odemeler.Where(p => p.OdemeTipi == OdemeTipi.Veresiye)).CariId);
        Assert.Equal(3, dto.Odemeler.Count);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Fis_baska_tenant_ve_sube_faturasi_okunamaz(bool otherTenant)
    {
        await Login();
        long id = 20;
        if (!otherTenant)
        {
            await using var db = new PanoPosDbContext(options);
            var f = new Fatura { TenantId = tenantA, SubeId = 3, DepoId = 3,
                FaturaNo = "OTHER-BRANCH", ParaBirimKodu = "TRY", Kur = 1 };
            db.Add(f); await db.SaveChangesAsync(); id = f.Id;
        }
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/fatura/{id}")).StatusCode);
        Assert.False((await client.GetAsync($"/api/v1/fatura/{id}?tenantId={tenantB}&subeId=2")).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fis_odemeleri_yanlis_tenant_sube_ve_gecersiz_kayitlari_dislar()
    {
        var id = await ReceiptInvoice();
        await ReceiptPay(id, 100, OdemeTipi.Nakit);
        await using (var db = new PanoPosDbContext(options))
        {
            foreach (var kind in new[] { "tenant", "branch", "inactive", "deleted" })
                db.Add(new Tahsilat { TenantId = kind == "tenant" ? tenantB : tenantA,
                    SubeId = kind == "branch" ? 3 : 1, FaturaId = id, TahsilatFisNo = kind,
                    OdemeTipi = OdemeTipi.Nakit, Tutar = 500, ParaBirimKodu = "TRY", Kur = 1,
                    AktifMi = kind != "inactive", SilindiMi = kind == "deleted" });
            await db.SaveChangesAsync();
            (await db.Tahsilatlar.SingleAsync(x => x.TahsilatFisNo == "inactive")).AktifMi = false;
            (await db.Tahsilatlar.SingleAsync(x => x.TahsilatFisNo == "deleted")).SilindiMi = true;
            await db.SaveChangesAsync();
        }
        var dto = await ReceiptRead(id);
        Assert.Single(dto.Odemeler); Assert.Equal(100, dto.NakitToplam); Assert.Equal(100, dto.OdenenTutar);
    }

    [Fact]
    public async Task Fis_legacy_bilinmeyen_cihaz_ve_urun_adi_uydurulmaz()
    {
        await Login("5678", 2);
        await using (var db = new PanoPosDbContext(options))
        {
            db.Add(new FaturaDetay { TenantId = tenantB, SubeId = 2, FaturaId = 20,
                StokKartId = 20, StokKartSatisBirimiId = 20, KdvId = 20,
                Miktar = 1, BirimFiyat = 1000, SatirNetToplam = 1000 });
            await db.SaveChangesAsync();
        }
        var dto = await ReceiptRead(20);
        Assert.Null(dto.CihazId); Assert.Null(dto.CihazAdi); Assert.Null(dto.KasiyerId);
        Assert.Equal(string.Empty, Assert.Single(dto.Detaylar).StokKartAd);
    }

    [Fact]
    public void Fis_swagger_header_detay_odeme_semasi()
    {
        var doc = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var fatura = doc.Components.Schemas[nameof(FaturaDto)];
        foreach (var property in new[] { "faturaTarihi", "subeAdi", "kasiyerAdi", "cihazAdi", "odemeler", "nakitToplam", "kartToplam", "veresiyeToplam" })
            Assert.Contains(property, fatura.Properties.Keys);
        var payment = doc.Components.Schemas[nameof(FaturaOdemeDto)];
        Assert.DoesNotContain("islemAnahtari", payment.Properties.Keys);
        Assert.DoesNotContain("istekOzeti", payment.Properties.Keys);
    }

    [Fact]
    public async Task Fis_yuzde_yirmi_kdv_ve_iskonto_sonrasi_matrah_doner()
    {
        await using (var db = new PanoPosDbContext(options))
        {
            var kdv = await db.Set<Kdv>().SingleAsync(x => x.TenantId == tenantA && x.Oran == 20);
            (await db.StokKartler.SingleAsync(x => x.Id == 10)).KdvId = kdv.Id;
            await db.SaveChangesAsync();
        }
        var dto = await ReceiptRead(await ReceiptInvoice(discount: true));
        var line = Assert.Single(dto.Detaylar);
        Assert.Equal(20, line.KdvOrani); Assert.True(line.KdvDahilMi);
        Assert.Equal(100, line.IndirimTutari); Assert.Equal(750, line.Matrah);
        Assert.Equal(150, line.KdvTutari); Assert.Equal(900, line.SatirNetToplam);
        Assert.Equal(750, dto.ToplamMatrah); Assert.Equal(150, dto.ToplamKdv);
        Assert.Equal(900, dto.NetToplam);
    }

    [Theory]
    [InlineData(1)] [InlineData(12)]
    public async Task Fis_okuma_sorgu_sayisi_satir_ve_odeme_sayisiyla_artmaz(int count)
    {
        var id = await ReceiptInvoice(lines: count);
        for (var i = 0; i < count; i++) await ReceiptPay(id, 10, OdemeTipi.Nakit);
        var counter = new ReceiptQueryCounter();
        await using var db = new PanoPosDbContext(new DbContextOptionsBuilder<PanoPosDbContext>(options)
            .AddInterceptors(counter).Options);
        var dto = await new FaturaServisi(db).FaturaGetirAsync(id);
        Assert.Equal(count, dto.Detaylar.Count); Assert.Equal(count, dto.Odemeler.Count);
        Assert.Equal(6, counter.Count);
    }

    private sealed class ReceiptQueryCounter : DbCommandInterceptor
    {
        public int Count { get; private set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Count++;
            Assert.DoesNotContain("SELECT *", command.CommandText, StringComparison.OrdinalIgnoreCase);
            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    public async Task Fis_snapshot_icin_master_include_silinmis_urunu_sessizce_atlamaz()
    {
        await Login();
        await using (var db = new PanoPosDbContext(options))
        {
            db.Add(new StokKart { Id = 11, TenantId = tenantA, SubeId = 1, StokKartKodu = "A11", Ad = "Second", KdvId = 1 });
            db.Add(new StokKartSatisBirimi { Id = 11, TenantId = tenantA, SubeId = 1, StokKartId = 11,
                BirimKodu = "ADET", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true });
            db.Add(new StokKartFiyat { TenantId = tenantA, SubeId = 1, StokKartSatisBirimiId = 11,
                FiyatTipiId = 1, Fiyat = 100, ParaBirimKodu = "TRY" });
            await db.SaveChangesAsync();
        }
        var orderId = await Order();
        await Post($"/api/v1/siparis/{orderId}/satir", new { StokKartId = 11, StokKartSatisBirimiId = 11, FiyatTipiId = 1, Miktar = 1 });
        await using (var db = new PanoPosDbContext(options))
        {
            (await db.StokKartler.SingleAsync(x => x.Id == 11)).SilindiMi = true;
            await db.SaveChangesAsync();
        }
        var response = await client.PostAsJsonAsync("/api/v1/fatura/olustur-siparisten", new { SiparisId = orderId });
        Assert.False(response.IsSuccessStatusCode);
        await using var check = new PanoPosDbContext(options);
        Assert.False(await check.Faturalar.AnyAsync(x => x.SiparisId == orderId));
        Assert.False(await check.StokFisleri.AnyAsync(x => x.TenantId == tenantA));
    }
}
