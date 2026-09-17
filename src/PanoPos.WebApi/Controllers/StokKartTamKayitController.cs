using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-kart")]
public sealed class StokKartTamKayitController : ControllerBase
{
    private readonly PanoPosDbContext _db;
    public StokKartTamKayitController(PanoPosDbContext db) => _db = db;

    [HttpPost("tam-kayit")]
    public async Task<ActionResult> Kaydet(StokKartTamKayitRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Ad) || request.SatisBirimleri.Count == 0)
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKart adi ve en az bir satis birimi zorunludur.", "product_registration_invalid");
        var sube = await _db.Subeler.YetkiliSube(_db).SingleOrDefaultAsync(x => x.Id == request.SubeId, ct) ?? throw new UygulamaHatasi(404,"Sube bulunamadi","Sube bulunamadi.","sube_not_found");
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var kdv = await _db.Kdvler.TenantKapsami(_db).SingleOrDefaultAsync(x => x.Id == request.KdvId && x.TenantId == sube.TenantId && x.AktifMi, ct)
            ?? throw new UygulamaHatasi(400, "Gecersiz KDV", "Ayni tenant icinde aktif KDV secilmelidir.", "kdv_invalid");
        var stokKart = new StokKart { TenantId=sube.TenantId, SubeId=sube.Id, StokKartKodu=string.IsNullOrWhiteSpace(request.StokKartKodu)?null:request.StokKartKodu.Trim(), Ad=request.Ad.Trim(), Aciklama=request.Aciklama?.Trim(), StokKartTipi=request.StokKartTipi, StokKategoriId=request.StokKategoriId, StokGrupId=request.StokGrupId };
        stokKart.KdvId = kdv.Id;
        if (request.StokKategoriId.HasValue && !await _db.StokKategorileri.TenantKapsami(_db).AnyAsync(x => x.Id == request.StokKategoriId && x.TenantId == sube.TenantId && x.AktifMi, ct))
            throw new UygulamaHatasi(404, "Kategori bulunamadi", "Aktif kategori bulunamadi.", "stok_kategori_not_found");
        if (request.StokGrupId.HasValue && !await _db.StokGruplari.TenantKapsami(_db).AnyAsync(x => x.Id == request.StokGrupId && x.TenantId == sube.TenantId && x.AktifMi, ct))
            throw new UygulamaHatasi(404, "Grup bulunamadi", "Aktif grup bulunamadi.", "stok_grup_not_found");
        _db.StokKartler.Add(stokKart); await _db.SaveChangesAsync(ct);
        foreach (var item in request.SatisBirimleri)
        {
            if (item.Katsayi <= 0 || string.IsNullOrWhiteSpace(item.BirimAdi)) throw new UygulamaHatasi(400,"Gecersiz birim","Birim adi ve katsayi gecersiz.","sales_unit_invalid");
            var birim = new StokKartSatisBirimi { TenantId=sube.TenantId, SubeId=sube.Id, StokKartId=stokKart.Id, BirimKodu=string.IsNullOrWhiteSpace(item.BirimKodu)?item.BirimAdi.Trim().ToUpperInvariant():item.BirimKodu.Trim().ToUpperInvariant(), BirimAdi=item.BirimAdi.Trim(), Katsayi=item.Katsayi, VarsayilanMi=item.VarsayilanMi };
            _db.StokKartSatisBirimleri.Add(birim); await _db.SaveChangesAsync(ct);
            if (!string.IsNullOrWhiteSpace(item.BarkodNo)) _db.Barkodlar.Add(new Barkod { TenantId=sube.TenantId, SubeId=sube.Id, StokKartId=stokKart.Id, StokKartSatisBirimiId=birim.Id, BarkodNo=item.BarkodNo.Trim(), BarkodTipi=PanoPos.Domain.Enums.BarkodTipi.Ean });
            foreach(var fiyat in item.Fiyatlar) {
                var kod=(fiyat.ParaBirimKodu??string.Empty).Trim().ToUpperInvariant(); if (string.IsNullOrWhiteSpace(kod) || fiyat.Fiyat < 0) throw new UygulamaHatasi(400,"Gecersiz fiyat","Fiyat ve para birimi gecersiz.","price_invalid");
                if (!await _db.FiyatTipleri.TenantKapsami(_db).AnyAsync(x=>x.Id==fiyat.FiyatTipiId && x.TenantId==sube.TenantId,ct)) throw new UygulamaHatasi(404,"Fiyat tipi bulunamadi","Fiyat tipi bulunamadi.","price_type_not_found");
                _db.StokKartFiyatlari.Add(new StokKartFiyat { TenantId=sube.TenantId, SubeId=sube.Id, StokKartSatisBirimiId=birim.Id, FiyatTipiId=fiyat.FiyatTipiId, Fiyat=fiyat.Fiyat, ParaBirimKodu=kod });
            }
        }
        await _db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Ok(new { stokKart.Id, stokKart.KdvId, KdvOrani = kdv.Oran });
    }
}
