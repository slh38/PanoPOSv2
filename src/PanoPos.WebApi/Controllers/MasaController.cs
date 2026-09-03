using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Restaurant;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/masa")]
public sealed class MasaController : ControllerBase
{
    private readonly IMasaServisi _masaServisi;

    public MasaController(IMasaServisi masaServisi)
    {
        _masaServisi = masaServisi;
    }

    [HttpPost]
    public async Task<ActionResult<MasaDto>> Olustur([FromBody] MasaOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _masaServisi.MasaOlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<MasaDto>>> Listele([FromQuery] long subeId, CancellationToken cancellationToken)
    {
        return Ok(await _masaServisi.MasaListeleAsync(subeId, cancellationToken));
    }
}
