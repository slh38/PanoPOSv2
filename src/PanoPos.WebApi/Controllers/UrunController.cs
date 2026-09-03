using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun")]
public sealed class StokKartController : ControllerBase
{
    private readonly IStokKartServisi _urunServisi;

    public StokKartController(IStokKartServisi urunServisi)
    {
        _urunServisi = urunServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokKartDto>> Olustur([FromBody] StokKartOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.StokKartOlusturAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<StokKartDto>> Guncelle(long id, [FromBody] StokKartGuncelleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.StokKartGuncelleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<StokKartDto>> Detay(long id, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.StokKartDetayGetirAsync(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<StokKartListeItemDto>>> Listele([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _urunServisi.StokKartListeleAsync(search, page, pageSize, cancellationToken));
    }

    [HttpPost("{urunId:long}/varyant")]
    public async Task<ActionResult<StokKartVaryantDto>> VaryantOlustur(long urunId, [FromBody] StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.StokKartVaryantOlusturAsync(urunId, request, cancellationToken));
    }

    [HttpGet("{urunId:long}/varyant")]
    public async Task<ActionResult<List<StokKartVaryantDto>>> VaryantlariGetir(long urunId, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.StokKartVaryantlariGetirAsync(urunId, cancellationToken));
    }
}
