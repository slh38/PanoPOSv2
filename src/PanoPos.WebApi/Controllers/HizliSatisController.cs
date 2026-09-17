using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Order;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/hizli-satis")]
public sealed class HizliSatisController(ISiparisServisi service) : ControllerBase
{
    [HttpPost("beklet")]
    [HttpPost("siparis")]
    public async Task<ActionResult<SiparisDto>> Kaydet(HizliSatisKaydetRequestDto request, CancellationToken ct)
        => Ok(await service.HizliSatisKaydetAsync(null, request, ct));

    [HttpGet("bekleyen")]
    public async Task<ActionResult<SayfaliSonucDto<BekleyenHizliSatisDto>>> Listele(
        [FromQuery] string? arama, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await service.BekleyenHizliSatisListeleAsync(arama, page, pageSize, ct));

    [HttpGet("bekleyen/{id:long}")]
    public async Task<ActionResult<SiparisDto>> Getir(long id, CancellationToken ct)
        => Ok(await service.BekleyenHizliSatisGetirAsync(id, ct));

    [HttpPut("bekleyen/{id:long}")]
    public async Task<ActionResult<SiparisDto>> Guncelle(long id, HizliSatisKaydetRequestDto request, CancellationToken ct)
        => Ok(await service.HizliSatisKaydetAsync(id, request, ct));
}
