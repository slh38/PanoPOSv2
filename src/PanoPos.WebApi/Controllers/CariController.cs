using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Customer;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/cari")]
public sealed class CariController : ControllerBase
{
    private readonly ICariServisi _cariServisi;

    public CariController(ICariServisi cariServisi)
    {
        _cariServisi = cariServisi;
    }

    [HttpPost]
    public async Task<ActionResult<CariDto>> Olustur([FromBody] CariOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _cariServisi.CariOlusturAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CariDto>> Guncelle(long id, [FromBody] CariGuncelleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _cariServisi.CariGuncelleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CariDto>> Getir(long id, [FromQuery] long subeId, CancellationToken cancellationToken)
    {
        return Ok(await _cariServisi.CariGetirAsync(id, subeId, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<CariListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _cariServisi.CariListeleAsync(subeId, search, page, pageSize, cancellationToken));
    }
}
