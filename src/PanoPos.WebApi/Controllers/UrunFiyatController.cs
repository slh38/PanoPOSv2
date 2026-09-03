using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun-fiyat")]
public sealed class StokKartFiyatController : ControllerBase
{
    private readonly PanoPosDbContext _db;
    public StokKartFiyatController(PanoPosDbContext db) => _db = db;

    [HttpPut("{id:long}")]
    public async Task<ActionResult<StokKartFiyatDto>> Guncelle(long id, StokKartFiyatGuncelleRequestDto request, CancellationToken ct)
    {
        var fiyat = await _db.StokKartFiyatlari.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UygulamaHatasi(404, "Fiyat bulunamadi", "StokKart fiyati bulunamadi.", "product_price_not_found");
        if (fiyat.SilindiMi)
            throw new UygulamaHatasi(404, "Fiyat bulunamadi", "Silinmis urun fiyati guncellenemez.", "product_price_not_found");
        var paraBirimKodu = (request.ParaBirimKodu ?? string.Empty).Trim().ToUpperInvariant();
        if (request.Fiyat < 0 || string.IsNullOrWhiteSpace(paraBirimKodu))
            throw new UygulamaHatasi(400, "Gecersiz fiyat", "Fiyat ve para birimi zorunludur.", "product_price_invalid");
        var cakisma = await _db.StokKartFiyatlari.AnyAsync(x => x.Id != fiyat.Id && x.TenantId == fiyat.TenantId && x.StokKartSatisBirimiId == fiyat.StokKartSatisBirimiId && x.FiyatTipiId == fiyat.FiyatTipiId && x.AktifMi, ct);
        if (request.AktifMi && cakisma)
            throw new UygulamaHatasi(409, "Fiyat cakismasi", "Ayni satis birimi ve fiyat tipi icin aktif fiyat zaten var.", "product_price_duplicate");
        fiyat.Fiyat = request.Fiyat;
        fiyat.ParaBirimKodu = paraBirimKodu;
        fiyat.AktifMi = request.AktifMi;
        await _db.SaveChangesAsync(ct);
        return Ok(new StokKartFiyatDto { Id = fiyat.Id, FiyatTipiId = fiyat.FiyatTipiId, Fiyat = fiyat.Fiyat, ParaBirimKodu = fiyat.ParaBirimKodu });
    }
}
