using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-kart")]
public sealed class StokKartController : ControllerBase
{
    private readonly IStokKartServisi _stokKartServisi;

    public StokKartController(IStokKartServisi stokKartServisi)
    {
        _stokKartServisi = stokKartServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokKartDto>> Olustur([FromBody] StokKartOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _stokKartServisi.StokKartOlusturAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<StokKartDto>> Guncelle(long id, [FromBody] StokKartGuncelleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _stokKartServisi.StokKartGuncelleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<StokKartDto>> Detay(long id, CancellationToken cancellationToken)
    {
        return Ok(await _stokKartServisi.StokKartDetayGetirAsync(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<StokKartListeItemDto>>> Listele([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default,
        [FromQuery] long? kategoriId = null, [FromQuery] long? grupId = null, [FromQuery] string? arama = null, [FromQuery] bool? aktifMi = null)
    {
        return Ok(await _stokKartServisi.StokKartListeleAsync(arama ?? search, page, pageSize, cancellationToken, kategoriId, grupId, aktifMi));
    }

    [HttpPost("{stokKartId:long}/varyant")]
    public async Task<ActionResult<StokKartVaryantDto>> VaryantOlustur(long stokKartId, [FromBody] StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _stokKartServisi.StokKartVaryantOlusturAsync(stokKartId, request, cancellationToken));
    }

    [HttpGet("{stokKartId:long}/varyant")]
    public async Task<ActionResult<List<StokKartVaryantDto>>> VaryantlariGetir(long stokKartId, CancellationToken cancellationToken)
    {
        return Ok(await _stokKartServisi.StokKartVaryantlariGetirAsync(stokKartId, cancellationToken));
    }
}
