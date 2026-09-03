using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun")]
public sealed class UrunTamKayitController : ControllerBase
{
    private readonly PanoPosDbContext _db;
    public UrunTamKayitController(PanoPosDbContext db) => _db = db;

    [HttpPost("tam-kayit")]
    public async Task<ActionResult> Kaydet(UrunTamKayitRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Ad) || request.SatisBirimleri.Count == 0)
            throw new UygulamaHatasi(400, "Gecersiz istek", "Urun adi ve en az bir satis birimi zorunludur.", "product_registration_invalid");
        var sube = await _db.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, ct) ?? throw new UygulamaHatasi(404,"Sube bulunamadi","Sube bulunamadi.","sube_not_found");
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var urun = new Urun { TenantId=sube.TenantId, SubeId=sube.Id, UrunKodu=string.IsNullOrWhiteSpace(request.UrunKodu)?null:request.UrunKodu.Trim(), Ad=request.Ad.Trim(), Aciklama=request.Aciklama?.Trim(), UrunTipi=request.UrunTipi, UrunKategoriId=request.UrunKategoriId, UrunGrupId=request.UrunGrupId };
        _db.Urunler.Add(urun); await _db.SaveChangesAsync(ct);
        foreach (var item in request.SatisBirimleri)
        {
            if (item.Katsayi <= 0 || string.IsNullOrWhiteSpace(item.BirimAdi)) throw new UygulamaHatasi(400,"Gecersiz birim","Birim adi ve katsayi gecersiz.","sales_unit_invalid");
            var birim = new UrunSatisBirimi { TenantId=sube.TenantId, SubeId=sube.Id, UrunId=urun.Id, BirimKodu=string.IsNullOrWhiteSpace(item.BirimKodu)?item.BirimAdi.Trim().ToUpperInvariant():item.BirimKodu.Trim().ToUpperInvariant(), BirimAdi=item.BirimAdi.Trim(), Katsayi=item.Katsayi, VarsayilanMi=item.VarsayilanMi };
            _db.UrunSatisBirimleri.Add(birim); await _db.SaveChangesAsync(ct);
            if (!string.IsNullOrWhiteSpace(item.BarkodNo)) _db.Barkodlar.Add(new Barkod { TenantId=sube.TenantId, SubeId=sube.Id, UrunId=urun.Id, UrunSatisBirimiId=birim.Id, BarkodNo=item.BarkodNo.Trim(), BarkodTipi=PanoPos.Domain.Enums.BarkodTipi.Ean });
            foreach(var fiyat in item.Fiyatlar) {
                var kod=(fiyat.ParaBirimKodu??string.Empty).Trim().ToUpperInvariant(); if (string.IsNullOrWhiteSpace(kod) || fiyat.Fiyat < 0) throw new UygulamaHatasi(400,"Gecersiz fiyat","Fiyat ve para birimi gecersiz.","price_invalid");
                if (!await _db.FiyatTipleri.AnyAsync(x=>x.Id==fiyat.FiyatTipiId && x.TenantId==sube.TenantId,ct)) throw new UygulamaHatasi(404,"Fiyat tipi bulunamadi","Fiyat tipi bulunamadi.","price_type_not_found");
                _db.UrunFiyatlari.Add(new UrunFiyat { TenantId=sube.TenantId, SubeId=sube.Id, UrunSatisBirimiId=birim.Id, FiyatTipiId=fiyat.FiyatTipiId, Fiyat=fiyat.Fiyat, ParaBirimKodu=kod });
            }
        }
        await _db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Ok(new { urun.Id });
    }
}
