using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/satis")]
public sealed class SatisController(ISatisStokCozumServisi servis) : ControllerBase
{
    [HttpGet("barkod/{barkodNo}")]
    public async Task<IActionResult> Barkod(string barkodNo, [FromQuery] long fiyatTipiId, CancellationToken ct)
        => Ok(await servis.BarkodCozAsync(barkodNo, fiyatTipiId, ct));

    [HttpGet("fiyat")]
    public async Task<IActionResult> Fiyat([FromQuery] long stokKartSatisBirimiId, [FromQuery] long fiyatTipiId,
        [FromQuery] long? stokKartVaryantId, CancellationToken ct)
        => Ok(await servis.FiyatCozAsync(stokKartSatisBirimiId, fiyatTipiId, stokKartVaryantId, ct));

    [HttpGet("/api/v1/fiyat-tipi")]
    public async Task<IActionResult> FiyatTipleri(int page = 1, int pageSize = 50, CancellationToken ct = default)
        => Ok(await servis.FiyatTipleriAsync(page, pageSize, ct));

    [HttpGet("/api/v1/stok-kart/{id:long}/satis-birimleri")]
    public async Task<IActionResult> SatisBirimleri(long id, int page = 1, int pageSize = 50, CancellationToken ct = default)
        => Ok(await servis.SatisBirimleriAsync(id, page, pageSize, ct));
}
