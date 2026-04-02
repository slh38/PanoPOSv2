using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Restaurant;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/adisyon")]
public sealed class AdisyonController : ControllerBase
{
    private readonly IAdisyonServisi _adisyonServisi;

    public AdisyonController(IAdisyonServisi adisyonServisi)
    {
        _adisyonServisi = adisyonServisi;
    }

    [HttpPost("ac")]
    public async Task<ActionResult<AdisyonDto>> Ac([FromBody] AdisyonAcRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _adisyonServisi.AdisyonAcAsync(request, cancellationToken));
    }

    [HttpPost("kapat")]
    public async Task<ActionResult<AdisyonDto>> Kapat([FromBody] AdisyonKapatRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _adisyonServisi.AdisyonKapatAsync(request, cancellationToken));
    }

    [HttpGet("acik")]
    public async Task<ActionResult<AdisyonDto?>> Acik([FromQuery] long masaId, CancellationToken cancellationToken)
    {
        return Ok(await _adisyonServisi.AcikAdisyonGetirAsync(masaId, cancellationToken));
    }
}
