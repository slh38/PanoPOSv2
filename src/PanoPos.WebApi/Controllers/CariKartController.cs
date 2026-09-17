using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Customer;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/carikart")]
public sealed class CariKartController : ControllerBase
{
    private readonly ICariKartServisi _cariKartServisi;

    public CariKartController(ICariKartServisi cariKartServisi)
    {
        _cariKartServisi = cariKartServisi;
    }

    [HttpPost]
    public async Task<ActionResult<CariKartDto>> Olustur([FromBody] CariKartOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _cariKartServisi.CariKartOlusturAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CariKartDto>> Guncelle(long id, [FromBody] CariKartGuncelleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _cariKartServisi.CariKartGuncelleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CariKartDto>> Getir(long id, [FromQuery] long subeId, CancellationToken cancellationToken)
    {
        return Ok(await _cariKartServisi.CariKartGetirAsync(id, subeId, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<CariKartListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _cariKartServisi.CariKartListeleAsync(subeId, search, page, pageSize, cancellationToken));
    }
}
