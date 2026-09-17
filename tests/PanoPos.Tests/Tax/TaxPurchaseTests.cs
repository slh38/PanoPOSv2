using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Tax;
using PanoPos.Application.Purchase;
using PanoPos.Application.Product;
using PanoPos.Application.Order;
using PanoPos.Application.Invoice;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.Infrastructure.Tax;
using PanoPos.Infrastructure.Purchase;
using PanoPos.Infrastructure.Product;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Invoice;
namespace PanoPos.Tests.Tax;

public sealed class TaxPurchaseTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PanoPosDbContext db;
    private readonly VergiHesaplamaServisi calc = new();
    private readonly KdvServisi kdv;
    private readonly AlisFaturaServisi purchase;
    private readonly StokKart stok;
    private readonly StokKartSatisBirimi birim;
    private readonly Cari cari;
    public TaxPurchaseTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        kdv = new(db); purchase = new(db, calc);
        stok = new() { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Ad = "Test stok", KdvId = 4 };
        cari = new() { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Ad = "Alici ve satici", CariKodu = "C01", Tip = CariTipi.Alici };
        db.AddRange(stok, cari); db.SaveChanges();
        birim = new() { TenantId = stok.TenantId, SubeId = 1, StokKartId = stok.Id, BirimKodu = "KOLI", BirimAdi = "Koli", Katsayi = 12 };
        db.Add(birim); db.SaveChanges();
    }
    private AlisFaturaKaydetRequest Request(decimal price = 100) => new() {
        SubeId = 1, DepoId = 1, CariId = cari.Id, FaturaNo = "AL-001", FaturaTarihi = new DateTime(2026,9,16),
        Detaylar = new() { new() { StokKartId = stok.Id, StokKartSatisBirimiId = birim.Id, Miktar = 10, BirimFiyat = price, IndirimOrani = 10 } }
    };
    private KdvKaydetRequest KdvRequest(decimal oran = 25, string kod = " kdv25 ") => new() { SubeId = 1, Kod = kod, Ad = " Yeni oran ", Oran = oran };
    [Fact] public async Task Seed_oranlari_olusur() => Assert.Equal(new decimal[] {0,1,10,20}, (await kdv.GetPagedAsync(1,null,null,1,50)).Kayitlar.Select(x=>x.Oran));
    [Fact] public async Task Yeni_oran_eklenir_ve_normalize_edilir() {
        var r = await kdv.CreateAsync(KdvRequest());
        Assert.Equal("KDV25", r.Kod); Assert.Equal("Yeni oran", r.Ad); Assert.Equal(25m,r.Oran);
    }
    [Theory] [InlineData("KDV20",25)] [InlineData("FARKLI",20)]
    public async Task Aktif_kod_veya_oran_duplicate_engellenir(string kod, int oran) => await Assert.ThrowsAsync<UygulamaHatasi>(()=>kdv.CreateAsync(KdvRequest(oran,kod)));
    [Theory] [InlineData(-1)] [InlineData(101)] [InlineData(1.111)]
    public async Task Gecersiz_oran_reddedilir(decimal oran) => await Assert.ThrowsAsync<UygulamaHatasi>(()=>kdv.CreateAsync(KdvRequest(oran)));
    [Fact] public async Task Kullanilan_kdv_silinemez() => await Assert.ThrowsAsync<UygulamaHatasi>(()=>kdv.DeleteAsync(4,1));
    [Fact] public async Task Kdv_update_ve_soft_delete() {
        var r = await kdv.CreateAsync(KdvRequest()); var req = KdvRequest(26," kdv26 ");
        r = await kdv.UpdateAsync(r.Id,req); Assert.Equal(26m,r.Oran);
        await kdv.DeleteAsync(r.Id,1);
        Assert.True((await db.Kdvler.IgnoreQueryFilters().SingleAsync(x=>x.Id==r.Id)).SilindiMi);
        Assert.DoesNotContain((await kdv.GetPagedAsync(1,null,null,1,50)).Kayitlar,x=>x.Id==r.Id);
    }
    [Fact] public async Task Kdv_pagination_search_aktif_filtre() {
        var r = await kdv.CreateAsync(KdvRequest());
        await kdv.UpdateAsync(r.Id,new() { SubeId=1,Kod=r.Kod,Ad=r.Ad,Oran=r.Oran,AktifMi=false });
        Assert.Single((await kdv.GetPagedAsync(1,"KDV25",false,1,1)).Kayitlar);
        Assert.Empty((await kdv.GetPagedAsync(1,"KDV25",true,1,1)).Kayitlar);
        Assert.Single((await kdv.GetPagedAsync(1,null,null,5,1)).Kayitlar);
    }
    [Fact] public async Task Stok_gecerli_kdv_ile_olusturulur() {
        var r = await new StokKartServisi(db).StokKartOlusturAsync(new() { SubeId=1,Ad="Yeni",KdvId=4 });
        Assert.Equal(4,r.KdvId);Assert.Equal(20m,r.KdvOrani);
    }
    [Fact] public async Task Stok_kdv_zorunlu() => await Assert.ThrowsAsync<UygulamaHatasi>(()=>new StokKartServisi(db).StokKartOlusturAsync(new() { SubeId=1,Ad="Yeni" }));
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Stok_pasif_veya_silinmis_kdv_kullanamaz(bool sil) {
        var r=await kdv.CreateAsync(KdvRequest());
        if(sil) await kdv.DeleteAsync(r.Id,1);
        else await kdv.UpdateAsync(r.Id,new() { SubeId=1,Kod=r.Kod,Ad=r.Ad,Oran=r.Oran,AktifMi=false });
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>new StokKartServisi(db).StokKartOlusturAsync(new() { SubeId=1,Ad="Yeni",KdvId=r.Id }));
    }
    [Theory] [InlineData(false,100)] [InlineData(true,120)]
    public void Dahil_haric_iskontolu_hesap(bool dahil, decimal fiyat) {
        var r=calc.Hesapla(new[] {new VergiSatir(10,fiyat,20,dahil,10)});
        Assert.Equal(900,r.ToplamMatrah);Assert.Equal(180,r.ToplamKdv);Assert.Equal(1080,r.NetToplam);
    }
    [Fact] public void Sifir_kdv() {
        var r=calc.Hesapla(new[] {new VergiSatir(1,100,0,false)});
        Assert.Equal(0,r.ToplamKdv);Assert.Equal(100,r.NetToplam);
    }
    [Fact] public void Genel_iskonto_matrahi_azaltir() {
        var r=calc.Hesapla(new[] {new VergiSatir(1,100,20,false)},10);
        Assert.Equal(90,r.ToplamMatrah);Assert.Equal(18,r.ToplamKdv);Assert.Equal(108,r.NetToplam);
    }
    [Fact] public void Farkli_oranlara_genel_iskonto_dagitilir() {
        var r=calc.Hesapla(new[] {new VergiSatir(1,100,10,false),new VergiSatir(1,200,20,false)},null,30);
        Assert.Equal(90,r.Satirlar[0].Matrah);Assert.Equal(180,r.Satirlar[1].Matrah);
        Assert.Equal(45,r.ToplamKdv);Assert.Equal(315,r.NetToplam);
    }
    [Fact] public void Kurus_farki_deterministik_ve_toplamlar_esit() {
        var input = new[] {new VergiSatir(1,1,10,true),new VergiSatir(1,1,20,true),new VergiSatir(1,1,0,true)};
        var r=calc.Hesapla(input,null,0.01m);
        Assert.Equal(0.01m,r.Satirlar[0].GenelIndirimPayi);Assert.Equal(2.99m,r.NetToplam);
        Assert.Equal(r.NetToplam,r.ToplamMatrah+r.ToplamKdv);
        Assert.Equal(r.Satirlar.ToArray(),calc.Hesapla(input,null,0.01m).Satirlar.ToArray());
    }
    [Theory] [InlineData(true,120)] [InlineData(false,100)]
    public async Task Satis_snapshot_ve_fatura_aktarimi(bool dahil, decimal fiyat) {
        var ayar=await db.TenantAyarlari.SingleAsync();ayar.SatisFiyatlariKdvDahilMi=dahil;await db.SaveChangesAsync();
        var service=new SiparisServisi(db);
        var sip=await service.SiparisOlusturAsync(new(){SubeId=1,SiparisTipi=SiparisTipi.HizliSatisBekleyen,ParaBirimKodu="TRY",Kur=1});
        ayar.SatisFiyatlariKdvDahilMi=!dahil;await db.SaveChangesAsync();
        sip=await service.SiparisSatirEkleAsync(sip.Id,new(){StokKartId=stok.Id,StokKartSatisBirimiId=birim.Id,Miktar=10,BirimFiyat=fiyat,IndirimOrani=10});
        Assert.Equal(dahil,sip.Detaylar[0].KdvDahilMi);
        Assert.Equal(900,sip.ToplamMatrah);Assert.Equal(180,sip.ToplamKdv);
        var master=await db.Kdvler.SingleAsync(x=>x.Id==4);master.Oran=25;await db.SaveChangesAsync();
        var f=await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new(){SiparisId=sip.Id});
        Assert.Equal(20,f.Detaylar[0].KdvOrani);Assert.Equal(sip.ToplamKdv,f.ToplamKdv);Assert.Equal(1080,f.NetToplam);
        var entity=await db.FaturaDetaylari.SingleAsync();
        Assert.Equal(12m,entity.BirimKatsayi);Assert.Equal(birim.Id,entity.StokKartSatisBirimiId);
    }
    [Fact] public async Task Alis_transaction_hata_rollback() {
        var r=Request();r.Detaylar.Add(new(){StokKartId=long.MaxValue,StokKartSatisBirimiId=birim.Id,Miktar=1,BirimFiyat=10});
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(r));
        db.ChangeTracker.Clear();Assert.Empty(await db.AlisFaturalar.ToListAsync());Assert.Empty(await db.AlisFaturaDetaylari.ToListAsync());
    }
    [Theory] [InlineData(false,100)] [InlineData(true,120)]
    public async Task Alis_toplam_ve_snapshot(bool dahil,decimal fiyat) {
        var ayar=await db.TenantAyarlari.SingleAsync();ayar.AlisFiyatlariKdvDahilMi=dahil;await db.SaveChangesAsync();
        var f=await purchase.CreateAsync(Request(fiyat));var l=Assert.Single(f.Detaylar);
        Assert.Equal(900,f.ToplamMatrah);Assert.Equal(180,f.ToplamKdv);Assert.Equal(1080,f.NetToplam);
        Assert.Equal("KOLI",l.BirimKodu);Assert.Equal(12,l.Katsayi);Assert.Equal(20,l.KdvOrani);Assert.Equal(dahil,l.KdvDahilMi);
    }
    [Fact] public async Task Alis_ayar_master_degisse_de_snapshot_korunur() {
        var f=await purchase.CreateAsync(Request());
        var a=await db.TenantAyarlari.SingleAsync();a.AlisFiyatlariKdvDahilMi=true;
        var k=await db.Kdvler.SingleAsync(x=>x.Id==4);k.Oran=25;birim.Katsayi=24;birim.BirimAdi="Degisti";await db.SaveChangesAsync();
        var r=Request();r.Detaylar[0].Id=f.Detaylar[0].Id;r.Detaylar[0].Miktar=20;
        f=await purchase.UpdateAsync(f.Id,r);
        Assert.Equal(20,f.Detaylar[0].KdvOrani);Assert.False(f.Detaylar[0].KdvDahilMi);
        Assert.Equal(12,f.Detaylar[0].Katsayi);Assert.Equal("Koli",f.Detaylar[0].BirimAdi);Assert.Equal(2160,f.NetToplam);
    }
    [Fact] public async Task Taslak_update_ve_satir_degistirme() {
        var f=await purchase.CreateAsync(Request());var r=Request(200);r.FaturaNo="DUZELT";
        f=await purchase.UpdateAsync(f.Id,r);Assert.Equal(2160,f.NetToplam);Assert.Single(f.Detaylar);Assert.Equal("DUZELT",f.FaturaNo);
        Assert.Equal(2,await db.AlisFaturaDetaylari.IgnoreQueryFilters().CountAsync());
    }
    [Fact] public async Task Kesinlestirme_idempotent_ve_cari_yan_etkisiz() {
        var f=await purchase.CreateAsync(Request());var hareket=await db.CariHareketleri.CountAsync();
        await purchase.KesinlestirAsync(f.Id,1);f=await purchase.KesinlestirAsync(f.Id,1);
        Assert.Equal(AlisFaturaDurumu.Kesinlesti,f.Durum);Assert.Equal(hareket,await db.CariHareketleri.CountAsync());
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Kilitli_belge_update_delete_engellenir(bool iptal) {
        var f=await purchase.CreateAsync(Request());
        if(iptal)await purchase.IptalAsync(f.Id,1);else await purchase.KesinlestirAsync(f.Id,1);
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.UpdateAsync(f.Id,Request()));
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.DeleteAsync(f.Id,1));
    }
    [Fact] public async Task Taslak_soft_delete() {
        var f=await purchase.CreateAsync(Request());await purchase.DeleteAsync(f.Id,1);
        Assert.Empty((await purchase.GetPagedAsync(new(){SubeId=1})).Kayitlar);
        Assert.True((await db.AlisFaturalar.IgnoreQueryFilters().SingleAsync()).SilindiMi);
    }
    [Fact] public async Task Alis_dapper_pagination_search_filtre() {
        await purchase.CreateAsync(Request());var r=Request();r.FaturaNo="AL-002";await purchase.CreateAsync(r);
        var page=await purchase.GetPagedAsync(new(){SubeId=1,Arama="C01",CariId=cari.Id,Durum=AlisFaturaDurumu.Taslak,Page=2,PageSize=1,
            BaslangicTarihi=new(2026,9,1),BitisTarihi=new(2026,9,30)});
        Assert.Equal(2,page.ToplamKayit);Assert.Single(page.Kayitlar);
        Assert.Single((await purchase.GetPagedAsync(new(){SubeId=1,Arama="AL-002"})).Kayitlar);
    }
    [Fact] public async Task Tenant_izolasyonu_ve_yabanci_iliskiler() {
        var tenant=Guid.NewGuid();
        db.Tenantler.Add(new(){TenantId=tenant,SubeId=1,Kod="OTHER",Ad="Other"});await db.SaveChangesAsync();
        var sube=new Sube{TenantId=tenant,SubeId=1,Kod="OTHER",Ad="Other"};db.Add(sube);await db.SaveChangesAsync();
        var foreign=new Kdv{TenantId=tenant,SubeId=sube.Id,Kod="KDV20",Ad="Other",Oran=20};db.Add(foreign);await db.SaveChangesAsync();
        Assert.Single((await kdv.GetPagedAsync(sube.Id,null,null,1,20)).Kayitlar);
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>kdv.GetByIdAsync(4,sube.Id));
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>new StokKartServisi(db).StokKartOlusturAsync(new(){SubeId=1,Ad="Bad",KdvId=foreign.Id}));
        var f=await purchase.CreateAsync(Request());
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.GetByIdAsync(f.Id,sube.Id));
        Assert.Empty((await purchase.GetPagedAsync(new(){SubeId=sube.Id})).Kayitlar);
        var r=Request();r.SubeId=sube.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(r));
        var otherCari=new Cari{TenantId=tenant,SubeId=sube.Id,Ad="Other"};db.Add(otherCari);await db.SaveChangesAsync();
        r.CariId=otherCari.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(r));
        stok.KdvId=foreign.Id;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(Request()));
    }
    [Fact] public async Task Alis_para_birimi_normalize_ve_try_kuru_bir() {
        var r=Request();r.ParaBirimKodu=" try ";r.Kur=4;r.Detaylar[0].FiyatParaBirimKodu=" try ";r.Detaylar[0].FiyatKur=4;
        var f=await purchase.CreateAsync(r);Assert.Equal("TRY",f.ParaBirimKodu);Assert.Equal(1,f.Kur);Assert.Equal(1,f.Detaylar[0].FiyatKur);
    }
    [Fact] public async Task Pasif_kdv_duplicate_aktif_oranla_olusabilir() {
        var r=KdvRequest(20,"KDV20");r.AktifMi=false;
        var created=await kdv.CreateAsync(r);
        Assert.False(created.AktifMi);
        r.AktifMi=true;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>kdv.UpdateAsync(created.Id,r));
    }
    [Fact] public async Task Alis_database_hatasi_header_ve_satirlari_rollback_yapar() {
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER FailPurchase BEFORE INSERT ON AlisFaturaDetay BEGIN SELECT RAISE(ABORT, 'test failure'); END;");
        await Assert.ThrowsAsync<DbUpdateException>(()=>purchase.CreateAsync(Request()));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.AlisFaturalar.ToListAsync());Assert.Empty(await db.AlisFaturaDetaylari.ToListAsync());
    }
    [Fact] public async Task Satis_fatura_tahsilat_tam_akis() {
        var s=new SiparisServisi(db);
        var order=await s.SiparisOlusturAsync(new(){SubeId=1,SiparisTipi=SiparisTipi.HizliSatisBekleyen,ParaBirimKodu="TRY",Kur=1});
        order=await s.SiparisSatirEkleAsync(order.Id,new(){StokKartId=stok.Id,Miktar=1,BirimFiyat=120});
        var invoice=await new FaturaServisi(db).SiparistenFaturaOlusturAsync(new(){SiparisId=order.Id});
        var kasa=new Kasa {TenantId=stok.TenantId,SubeId=1,Ad="Test kasa"};db.Add(kasa);await db.SaveChangesAsync();
        await new PanoPos.Infrastructure.Payment.TahsilatServisi(db).TahsilatOlusturAsync(new(){
            SubeId=1,FaturaId=invoice.Id,OdemeTipi=OdemeTipi.Nakit,KasaId=kasa.Id,KullaniciId=1,CihazId=1,
            Tutar=120,ParaBirimKodu="TRY",Kur=1});
        invoice=await new FaturaServisi(db).FaturaGetirAsync(invoice.Id);
        Assert.Equal(FaturaDurumu.Kapali,invoice.Durum);Assert.Equal(20,invoice.ToplamKdv);Assert.Equal(0,invoice.KalanTutar);
    }
    [Fact] public async Task Satis_ek_satir_eski_kdv_snapshotini_degistirmez() {
        var s=new SiparisServisi(db);
        var order=await s.SiparisOlusturAsync(new(){SubeId=1,SiparisTipi=SiparisTipi.HizliSatisBekleyen,ParaBirimKodu="TRY",Kur=1,GenelIndirimOrani=10});
        order=await s.SiparisSatirEkleAsync(order.Id,new(){StokKartId=stok.Id,Miktar=1,BirimFiyat=120});
        var k=await db.Kdvler.SingleAsync(x=>x.Id==4);k.Oran=25;await db.SaveChangesAsync();
        order=await s.SiparisSatirEkleAsync(order.Id,new(){StokKartId=stok.Id,Miktar=1,BirimFiyat=125});
        Assert.Equal(20,order.Detaylar[0].KdvOrani);Assert.Equal(25,order.Detaylar[1].KdvOrani);
        Assert.Equal(order.NetToplam,order.Detaylar.Sum(x=>x.SatirNetToplam));
        Assert.Equal(order.ToplamKdv,order.Detaylar.Sum(x=>x.KdvTutari));
    }
    [Fact] public async Task Stok_update_gecerli_kdv_ve_yabanci_kdv_engeli() {
        var service=new StokKartServisi(db);
        var result=await service.StokKartGuncelleAsync(stok.Id,new(){Ad=stok.Ad,KdvId=3,AktifMi=true});
        Assert.Equal(10,result.KdvOrani);
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>service.StokKartGuncelleAsync(stok.Id,new(){Ad=stok.Ad,KdvId=long.MaxValue,AktifMi=true}));
    }
    [Fact] public async Task Tam_kayit_kdv_dogrulanir() {
        var controller=new PanoPos.WebApi.Controllers.StokKartTamKayitController(db);
        var r=new StokKartTamKayitRequestDto {SubeId=1,Ad="Tam Kayit",KdvId=3,
            SatisBirimleri=new(){new(){BirimAdi="Adet",BirimKodu="ADET",Katsayi=1}}};
        await controller.Kaydet(r,default);
        Assert.Equal(3,(await db.StokKartler.SingleAsync(x=>x.Ad=="Tam Kayit")).KdvId);
        r.KdvId=long.MaxValue;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>controller.Kaydet(r,default));
        Assert.Single(await db.StokKartler.Where(x=>x.Ad=="Tam Kayit").ToListAsync());
    }
    [Fact] public async Task Alis_farkli_kdv_genel_indirim_header_satir_esit() {
        var second=new StokKart{TenantId=stok.TenantId,SubeId=1,Ad="KDV10",KdvId=3};db.Add(second);await db.SaveChangesAsync();
        var unit=new StokKartSatisBirimi{TenantId=stok.TenantId,SubeId=1,StokKartId=second.Id,BirimKodu="AD",BirimAdi="Adet",Katsayi=1};db.Add(unit);await db.SaveChangesAsync();
        var r=Request();r.Detaylar[0].Miktar=1;r.Detaylar[0].IndirimOrani=null;r.GenelIndirimTutari=30;
        r.Detaylar.Add(new(){StokKartId=second.Id,StokKartSatisBirimiId=unit.Id,Miktar=1,BirimFiyat=200});
        var f=await purchase.CreateAsync(r);
        Assert.Equal(270,f.ToplamMatrah);Assert.Equal(36,f.ToplamKdv);Assert.Equal(306,f.NetToplam);
        Assert.Equal(f.NetToplam,f.Detaylar.Sum(x=>x.SatirNetToplam));
    }
    [Fact] public async Task Yabanci_satis_birimi_ve_varyant_reddedilir() {
        var other=new StokKart{TenantId=stok.TenantId,SubeId=1,Ad="Diger",KdvId=3};db.Add(other);await db.SaveChangesAsync();
        var unit=new StokKartSatisBirimi{TenantId=stok.TenantId,SubeId=1,StokKartId=other.Id,BirimKodu="AD",BirimAdi="Adet",Katsayi=1};db.Add(unit);
        var renk=new Renk{TenantId=stok.TenantId,SubeId=1,Ad="Test renk"};db.Add(renk);await db.SaveChangesAsync();
        var variant=new StokKartVaryant{TenantId=stok.TenantId,SubeId=1,StokKartId=other.Id,VaryantKodu="V1",RenkId=renk.Id};db.Add(variant);await db.SaveChangesAsync();
        var r=Request();r.Detaylar[0].StokKartSatisBirimiId=unit.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(r));
        r=Request();r.Detaylar[0].StokKartVaryantId=variant.Id;
        await Assert.ThrowsAsync<UygulamaHatasi>(()=>purchase.CreateAsync(r));
    }
    [Fact] public async Task Silinen_cari_gecmis_alis_belgesini_gizlemez() {
        var f=await purchase.CreateAsync(Request());await purchase.KesinlestirAsync(f.Id,1);
        cari.SoftDelete(null,DateTime.UtcNow);await db.SaveChangesAsync();
        var stored=await purchase.GetByIdAsync(f.Id,1);
        Assert.Equal(f.NetToplam,stored.NetToplam);Assert.Single(stored.Detaylar);
    }
    [Theory] [InlineData("KDV20",25)] [InlineData("DUP",20)]
    public async Task Sqlite_unique_index_dogrudan_duplicate_engeller(string kod,decimal oran) {
        db.Kdvler.Add(new(){TenantId=stok.TenantId,SubeId=1,Kod=kod,Ad="Duplicate",Oran=oran});
        await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());
    }
    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
