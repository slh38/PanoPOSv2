using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Order;

public sealed class HizliSatisTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly SiparisServisi service;
    private readonly long unitId, secondUnitId;
    private readonly Guid tenant = SystemSeedData.TenantGuid;
    private sealed class Context(Guid tenant, long branch = 1) : IIslemBaglami
    {
        public bool Dogrulandi => true;
        public Guid TenantId => tenant;
        public long SubeId => branch;
        public long KullaniciId => 1;
        public long CihazId => 1;
        public long KullaniciOturumId => 1;
    }
    private PanoPosDbContext Db(Guid tenantId, long branch = 1) => new(
        new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options, new Context(tenantId, branch));

    public HizliSatisTests()
    {
        connection.Open(); db = Db(tenant); db.Database.EnsureCreated();
        var stock = new StokKart { TenantId = tenant, SubeId = 1, Ad = "Su", KdvId = 4 };
        var other = new StokKart { TenantId = tenant, SubeId = 1, Ad = "Ekmek", KdvId = 3 };
        db.AddRange(stock, other); db.SaveChanges();
        var unit = new StokKartSatisBirimi { TenantId = tenant, SubeId = 1, StokKartId = stock.Id,
            BirimKodu = "KOLI", BirimAdi = "Koli", Katsayi = 24, VarsayilanMi = true };
        var second = new StokKartSatisBirimi { TenantId = tenant, SubeId = 1, StokKartId = other.Id,
            BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true };
        db.AddRange(unit, second); db.SaveChanges(); unitId = unit.Id; secondUnitId = second.Id;
        db.AddRange(new StokKartFiyat { TenantId = tenant, SubeId = 1, StokKartSatisBirimiId = unitId,
                FiyatTipiId = 1, Fiyat = 120, ParaBirimKodu = "TRY" },
            new StokKartFiyat { TenantId = tenant, SubeId = 1, StokKartSatisBirimiId = secondUnitId,
                FiyatTipiId = 1, Fiyat = 110, ParaBirimKodu = "TRY" });
        db.SaveChanges(); service = new(db);
    }
    private HizliSatisKaydetRequestDto Request(SiparisDto? saved = null) => new() {
        Surum = saved?.Surum, FiyatTipiId = 1, Satirlar = [new() { SiparisDetayId = saved?.Detaylar.First().Id,
            StokKartSatisBirimiId = unitId, Miktar = 1 }] };

    [Fact] public async Task Beklet_tek_seferde_header_detay_olusturur_stok_fatura_tahsilat_uretmez()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        Assert.Equal(120m, r.NetToplam); Assert.Single(r.Detaylar);
        Assert.Equal(SiparisTipi.HizliSatisBekleyen, r.SiparisTipi); Assert.Equal(SiparisDurumu.Bekliyor, r.Durum);
        Assert.Equal(1, await db.Siparisler.CountAsync()); Assert.Equal(1, await db.SiparisDetaylari.CountAsync());
        Assert.Empty(await db.StokHareketleri.ToListAsync()); Assert.Empty(await db.Faturalar.ToListAsync());
        Assert.Empty(await db.Tahsilatlar.ToListAsync());
    }

    [Fact] public async Task Geri_ac_ticari_snapshotlari_korur()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        var price = await db.StokKartFiyatlari.FirstAsync(); price.Fiyat = 999; await db.SaveChangesAsync();
        var opened = await service.BekleyenHizliSatisGetirAsync(r.Id); var line = Assert.Single(opened.Detaylar);
        Assert.Equal(r.Surum, opened.Surum); Assert.Equal(1, opened.FiyatTipiId);
        Assert.Equal("KOLI", line.BirimKodu); Assert.Equal("Koli", line.BirimAdi); Assert.Equal(24m, line.BirimKatsayi);
        Assert.Equal("Su", line.StokKartAd); Assert.Equal(unitId, line.StokKartSatisBirimiId);
        Assert.Equal(120m, line.BirimFiyat); Assert.Equal(1, line.FiyatTipiId); Assert.NotNull(line.FiyatTipiAdi);
        Assert.Equal("TRY", line.FiyatParaBirimKodu); Assert.Equal(1m, line.FiyatKur);
        Assert.Equal(4, line.KdvId); Assert.Equal(20m, line.KdvOrani); Assert.True(line.KdvDahilMi);
        Assert.Equal(100m, line.Matrah); Assert.Equal(20m, line.KdvTutari); Assert.Equal(120m, line.SatirNetToplam);
    }

    [Fact] public async Task Toplu_update_miktar_degistirir_satir_siler_yeni_satir_ekler()
    {
        var input = Request(); input.Satirlar.Add(new() { StokKartSatisBirimiId = secondUnitId, Miktar = 1 });
        var r = await service.HizliSatisKaydetAsync(null, input);
        var edit = Request(r); edit.Satirlar[0].Miktar = 2;
        edit.Satirlar.Add(new() { StokKartSatisBirimiId = secondUnitId, Miktar = 3 });
        var updated = await service.HizliSatisKaydetAsync(r.Id, edit);
        Assert.Equal(570m, updated.NetToplam); Assert.Equal(2, updated.Detaylar.Count);
        Assert.Contains(updated.Detaylar, x => x.Id == r.Detaylar[0].Id && x.Miktar == 2);
        Assert.DoesNotContain(updated.Detaylar, x => x.Id == r.Detaylar[1].Id);
        Assert.True(await db.SiparisDetaylari.IgnoreQueryFilters().AnyAsync(x => x.Id == r.Detaylar[1].Id && x.SilindiMi));
        Assert.NotEqual(r.Surum, updated.Surum);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Satir_ve_genel_iskonto_merkezi_motorla_hesaplanir(bool amount)
    {
        var input = Request(); input.Satirlar.Add(new() { StokKartSatisBirimiId = secondUnitId, Miktar = 1 });
        if (amount) { input.Satirlar[0].IndirimTutari = 12; input.GenelIndirimTutari = 21.8m; }
        else { input.Satirlar[0].IndirimOrani = 10; input.GenelIndirimOrani = 10; }
        var r = await service.HizliSatisKaydetAsync(null, input);
        Assert.Equal(196.2m, r.NetToplam); Assert.Equal(21.8m, r.GenelIndirimTutari);
        Assert.Equal(r.ToplamMatrah + r.ToplamKdv, r.NetToplam);
        Assert.Equal(r.NetToplam, r.Detaylar.Sum(x => x.SatirNetToplam));
        Assert.Equal(21.8m, r.Detaylar.Sum(x => x.GenelIndirimPayi));
    }

    [Fact] public async Task Degismemis_satir_dahil_tum_sepet_kayitta_yeniden_fiyatlanir()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        (await db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == unitId)).Fiyat = 240;
        (await db.Kdvler.SingleAsync(x => x.Id == 4)).Oran = 25; await db.SaveChangesAsync();
        var updated = await service.HizliSatisKaydetAsync(r.Id, Request(r));
        Assert.Equal(240m, updated.NetToplam); Assert.Equal(25m, updated.Detaylar[0].KdvOrani);
    }

    [Theory] [InlineData(null)] [InlineData(0.0)] [InlineData(-1.0)] [InlineData(42.5)]
    public async Task Doviz_satiri_acik_pozitif_kur_gerektirir(double? rate)
    {
        var price = await db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == unitId);
        price.Fiyat = 10; price.ParaBirimKodu = "USD"; await db.SaveChangesAsync();
        var input = Request(); input.Satirlar[0].FiyatKur = (decimal?)rate;
        if (rate is null or <= 0) {
            await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(null, input));
            Assert.Empty(await db.Siparisler.ToListAsync());
        } else {
            var r = await service.HizliSatisKaydetAsync(null, input);
            Assert.Equal(425m, r.Detaylar[0].BirimFiyat); Assert.Equal(42.5m, r.Detaylar[0].FiyatKur);
            Assert.Equal("USD", r.Detaylar[0].FiyatParaBirimKodu);
        }
    }

    [Fact] public async Task Ayni_para_biriminde_kur_bir_ve_kod_normalize()
    {
        var input = Request(); input.BelgeParaBirimKodu = " try "; input.Satirlar[0].FiyatKur = -1;
        var r = await service.HizliSatisKaydetAsync(null, input);
        Assert.Equal("TRY", r.ParaBirimKodu); Assert.Equal(1m, r.Detaylar[0].FiyatKur);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Son_satir_hatasi_tum_transactioni_geri_alir(bool update)
    {
        var r = update ? await service.HizliSatisKaydetAsync(null, Request()) : null;
        var input = Request(r); input.Satirlar[0].Miktar = 5;
        input.Satirlar.Add(new() { StokKartSatisBirimiId = long.MaxValue, Miktar = 1 });
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r?.Id, input));
        Assert.Equal(update ? 1 : 0, await db.Siparisler.CountAsync());
        Assert.Equal(update ? 1 : 0, await db.SiparisDetaylari.IgnoreQueryFilters().CountAsync());
        if (r != null) { var unchanged = await service.BekleyenHizliSatisGetirAsync(r.Id);
            Assert.Equal(120m, unchanged.NetToplam); Assert.Equal(r.Surum, unchanged.Surum); }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Eski_veya_eksik_surum_yeni_kaydi_ezemez(bool missing)
    {
        var r = await service.HizliSatisKaydetAsync(null, Request()); var edit = Request(r); edit.Satirlar[0].Miktar = 2;
        await service.HizliSatisKaydetAsync(r.Id, edit);
        var old = Request(r); if (missing) old.Surum = null;
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r.Id, old));
        Assert.Equal("cart_version_conflict", ex.ErrorCode); Assert.Equal(409, ex.StatusCode);
        Assert.Equal(240m, (await service.BekleyenHizliSatisGetirAsync(r.Id)).NetToplam);
    }

    [Fact] public async Task Eski_satir_apisinin_degisikligi_surum_degistirir()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        await service.SiparisSatirEkleAsync(r.Id, new() { StokKartId = r.Detaylar[0].StokKartId,
            StokKartSatisBirimiId = unitId, FiyatTipiId = 1, Miktar = 1 });
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r.Id, Request(r)));
        Assert.Equal("cart_version_conflict", ex.ErrorCode);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Tenant_ve_sube_disindaki_satis_okunamaz_degismez_iptal_ve_fatura_olmaz(bool otherTenant)
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        using var other = Db(otherTenant ? Guid.NewGuid() : tenant, otherTenant ? 1 : 99);
        var s = new SiparisServisi(other);
        Assert.Empty((await s.BekleyenHizliSatisListeleAsync(null, 1, 10)).Kayitlar);
        await Assert.ThrowsAsync<UygulamaHatasi>(() => s.BekleyenHizliSatisGetirAsync(r.Id));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => s.HizliSatisKaydetAsync(r.Id, Request(r)));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => s.SiparisIptalAsync(r.Id));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => new FaturaServisi(other).SiparistenFaturaOlusturAsync(new() { SiparisId = r.Id }));
    }

    [Fact] public async Task Liste_arama_pagination_ve_iptal_filtreleri()
    {
        for (var i = 0; i < 3; i++) { var input = Request(); input.Aciklama = "Musteri " + i;
            await service.HizliSatisKaydetAsync(null, input); }
        var first = await service.BekleyenHizliSatisListeleAsync("Musteri", 1, 2);
        var second = await service.BekleyenHizliSatisListeleAsync("Musteri", 2, 2);
        Assert.Equal(3, first.ToplamKayit); Assert.Equal(2, first.Kayitlar.Count); Assert.Single(second.Kayitlar);
        Assert.Single((await service.BekleyenHizliSatisListeleAsync("Musteri 1", 1, 10)).Kayitlar);
        Assert.All(first.Kayitlar, x => Assert.Equal(1, x.SatirSayisi));
        await service.SiparisIptalAsync(first.Kayitlar[0].Id);
        Assert.Equal(2, (await service.BekleyenHizliSatisListeleAsync(null, 1, 10)).ToplamKayit);
        Assert.Empty(await db.StokHareketleri.ToListAsync()); Assert.Empty(await db.Faturalar.ToListAsync());
    }

    [Theory] [InlineData(0, 10)] [InlineData(1, 201)] [InlineData(int.MaxValue, 200)]
    public async Task Liste_kontrolsuz_sayfalama_reddeder(int page, int size)
        => await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BekleyenHizliSatisListeleAsync(null, page, size));

    [Fact] public async Task Bos_sepet_reddedilir()
    {
        var input = Request(); input.Satirlar.Clear();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(null, input));
        Assert.Empty(await db.Siparisler.ToListAsync());
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Baska_satis_satiri_ve_duplicate_satir_id_reddedilir(bool duplicate)
    {
        var a = await service.HizliSatisKaydetAsync(null, Request());
        var b = await service.HizliSatisKaydetAsync(null, Request()); var edit = Request(a);
        if (duplicate) edit.Satirlar.Add(edit.Satirlar[0]); else edit.Satirlar[0].SiparisDetayId = b.Detaylar[0].Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(a.Id, edit));
    }

    [Fact] public async Task Guncellenen_siparis_fatura_snapshoti_ve_tek_stok_cikisi()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request()); var edit = Request(r); edit.Satirlar[0].Miktar = 2;
        var current = await service.HizliSatisKaydetAsync(r.Id, edit);
        var invoices = new FaturaServisi(db); var f = await invoices.SiparistenFaturaOlusturAsync(new() { SiparisId = r.Id });
        Assert.Equal(current.NetToplam, f.NetToplam); Assert.Equal(2m, f.Detaylar[0].Miktar);
        Assert.Equal(current.Detaylar[0].FiyatKur, f.Detaylar[0].FiyatKur);
        Assert.Equal(-48m, (await db.StokHareketleri.ToListAsync()).Sum(x => x.Miktar));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => invoices.SiparistenFaturaOlusturAsync(new() { SiparisId = r.Id }));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r.Id, Request(current)));
        Assert.Single(await db.StokHareketleri.ToListAsync());
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Restaurant_ve_iptal_satis_toplu_duzenlenemez(bool canceled)
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        if (canceled) await service.SiparisIptalAsync(r.Id);
        else { (await db.Siparisler.SingleAsync(x => x.Id == r.Id)).SiparisTipi = SiparisTipi.Masa; await db.SaveChangesAsync(); }
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.BekleyenHizliSatisGetirAsync(r.Id));
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r.Id, Request(r)));
        Assert.Empty((await service.BekleyenHizliSatisListeleAsync(null, 1, 10)).Kayitlar);
    }
    [Fact] public async Task Iki_ayri_context_ayni_surumu_yazamaz()
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        using var first = Db(tenant); using var second = Db(tenant);
        var a = await first.Siparisler.SingleAsync(x => x.Id == r.Id);
        var b = await second.Siparisler.SingleAsync(x => x.Id == r.Id);
        a.Aciklama = "Yeni kayit"; await first.SaveChangesAsync();
        b.Aciklama = "Eski ekran";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        Assert.Equal("Yeni kayit", (await service.BekleyenHizliSatisGetirAsync(r.Id)).Aciklama);
    }

    [Theory] [InlineData("birim")] [InlineData("fiyat")] [InlineData("tip")] [InlineData("kdv")]
    public async Task Update_pasif_master_ile_yeni_fiyat_olusturamaz(string master)
    {
        var r = await service.HizliSatisKaydetAsync(null, Request());
        PanoPos.Domain.Common.BaseEntity entity = master switch {
            "birim" => await db.StokKartSatisBirimleri.SingleAsync(x => x.Id == unitId),
            "fiyat" => await db.StokKartFiyatlari.SingleAsync(x => x.StokKartSatisBirimiId == unitId),
            "tip" => await db.FiyatTipleri.SingleAsync(x => x.Id == 1),
            _ => await db.Kdvler.SingleAsync(x => x.Id == 4) };
        entity.AktifMi = false; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(() => service.HizliSatisKaydetAsync(r.Id, Request(r)));
        Assert.Equal(120m, (await service.BekleyenHizliSatisGetirAsync(r.Id)).NetToplam);
    }
    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
