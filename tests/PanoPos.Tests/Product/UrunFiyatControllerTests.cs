using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.WebApi.Controllers;

namespace PanoPos.Tests.Product;

public sealed class UrunFiyatControllerTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly PanoPosDbContext _db;
    private readonly UrunFiyatController _controller;
    private readonly UrunSatisBirimi _birim;
    private readonly FiyatTipi _tip;

    public UrunFiyatControllerTests()
    {
        _connection.Open();
        _db = new PanoPosDbContext(new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        var urun = new Urun { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Ad = "Test" };
        _db.Urunler.Add(urun); _db.SaveChanges();
        _birim = new UrunSatisBirimi { TenantId = SystemSeedData.TenantGuid, SubeId = 1, UrunId = urun.Id, BirimAdi = "Adet", BirimKodu = "ADET", Katsayi = 1 };
        _tip = new FiyatTipi { TenantId = SystemSeedData.TenantGuid, SubeId = 1, Kod = "TEST", Ad = "Test" };
        _db.AddRange(_birim, _tip); _db.SaveChanges(); _controller = new UrunFiyatController(_db);
    }

    [Fact] public async Task Update_fiyati_ve_dort_ondaligi_korur() { var f=Ekle(10.1111m,"TRY"); var r=await Guncelle(f,2.5555m,"TRY"); Assert.Equal(2.5555m,r.Fiyat); }
    [Fact] public async Task Update_para_birimini_trim_ve_uppercase_yapar() { var f=Ekle(10,"TRY"); var r=await Guncelle(f,10," usd "); Assert.Equal("USD",r.ParaBirimKodu); }
    [Fact] public async Task Try_usd_olarak_update_edilebilir() { var f=Ekle(10,"TRY"); var r=await Guncelle(f,.25m,"USD"); Assert.Equal("USD",r.ParaBirimKodu); }
    [Fact] public void Ayni_birim_ve_tipte_ikinci_aktif_kayit_veritabani_tarafindan_engellenir() { Ekle(1,"TRY"); Assert.Throws<DbUpdateException>(()=>Ekle(2,"USD")); }
    [Fact] public async Task Kendi_kaydi_unique_conflict_uretmez() { var f=Ekle(1,"TRY"); var r=await Guncelle(f,2,"TRY"); Assert.Equal(2,r.Fiyat); }
    [Fact] public async Task Silinmis_kayit_update_edilemez() { var f=Ekle(1,"TRY"); f.SilindiMi=true; _db.SaveChanges(); await Assert.ThrowsAsync<UygulamaHatasi>(()=>_controller.Guncelle(f.Id,new(){Fiyat=2,ParaBirimKodu="USD",AktifMi=true},default)); }
    [Fact] public async Task Update_kendi_idsi_nedeniyle_false_positive_conflict_uretmez() { var f=Ekle(1,"TRY"); var r=await Guncelle(f,2,"USD"); Assert.Equal("USD",r.ParaBirimKodu); }
    [Fact] public async Task Decimal_dort_onadalik_response_da_korunur() { var f=Ekle(0.1234m,"USD"); var r=await Guncelle(f,0.1234m,"USD"); Assert.Equal(0.1234m,r.Fiyat); }

    private UrunFiyat Ekle(decimal fiyat,string para,bool aktif=true,bool yeniTip=false) { var tip=yeniTip?new FiyatTipi{TenantId=SystemSeedData.TenantGuid,SubeId=1,Kod=Guid.NewGuid().ToString(),Ad="x"}:_tip; if(yeniTip) { _db.Add(tip); _db.SaveChanges(); } var f=new UrunFiyat{TenantId=SystemSeedData.TenantGuid,SubeId=1,UrunSatisBirimiId=_birim.Id,FiyatTipiId=tip.Id,Fiyat=fiyat,ParaBirimKodu=para,AktifMi=aktif}; _db.Add(f);_db.SaveChanges();return f; }
    private async Task<UrunFiyatDto> Guncelle(UrunFiyat f,decimal fiyat,string para) { var x=await _controller.Guncelle(f.Id,new(){Fiyat=fiyat,ParaBirimKodu=para,AktifMi=true},default); return ((OkObjectResult)x.Result!).Value as UrunFiyatDto ?? throw new InvalidOperationException(); }
    public void Dispose(){_db.Dispose();_connection.Dispose();}
}
