using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun")]
public sealed class UrunController : ControllerBase
{
    private readonly IUrunServisi _urunServisi;

    public UrunController(IUrunServisi urunServisi)
    {
        _urunServisi = urunServisi;
    }

    [HttpPost]
    public async Task<ActionResult<UrunDto>> Olustur([FromBody] UrunOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.UrunOlusturAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UrunDto>> Guncelle(long id, [FromBody] UrunGuncelleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.UrunGuncelleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UrunDto>> Detay(long id, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.UrunDetayGetirAsync(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<UrunListeItemDto>>> Listele([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _urunServisi.UrunListeleAsync(search, page, pageSize, cancellationToken));
    }

    [HttpPost("{urunId:long}/varyant")]
    public async Task<ActionResult<UrunVaryantDto>> VaryantOlustur(long urunId, [FromBody] UrunVaryantOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.UrunVaryantOlusturAsync(urunId, request, cancellationToken));
    }

    [HttpGet("{urunId:long}/varyant")]
    public async Task<ActionResult<List<UrunVaryantDto>>> VaryantlariGetir(long urunId, CancellationToken cancellationToken)
    {
        return Ok(await _urunServisi.UrunVaryantlariGetirAsync(urunId, cancellationToken));
    }
}
