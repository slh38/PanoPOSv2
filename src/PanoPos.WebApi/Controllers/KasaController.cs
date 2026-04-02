using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Cash;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/kasa")]
public sealed class KasaController : ControllerBase
{
    private readonly IKasaServisi _kasaServisi;

    public KasaController(IKasaServisi kasaServisi)
    {
        _kasaServisi = kasaServisi;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<KasaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<KasaDto>>> Getir(CancellationToken cancellationToken)
    {
        return Ok(await _kasaServisi.ListeleAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(KasaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KasaDto>> Olustur([FromBody] KasaOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _kasaServisi.OlusturAsync(request, cancellationToken));
    }
}
