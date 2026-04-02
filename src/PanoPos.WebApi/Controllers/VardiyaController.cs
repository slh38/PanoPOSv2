using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Cash;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/vardiya")]
public sealed class VardiyaController : ControllerBase
{
    private readonly IVardiyaServisi _vardiyaServisi;

    public VardiyaController(IVardiyaServisi vardiyaServisi)
    {
        _vardiyaServisi = vardiyaServisi;
    }

    [HttpPost("ac")]
    [ProducesResponseType(typeof(VardiyaResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VardiyaResponseDto>> Ac([FromBody] VardiyaAcRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _vardiyaServisi.VardiyaAcAsync(request.KullaniciId, request.CihazId, request.KasaId, request.AcilisNakit, cancellationToken);
        return Ok(response);
    }

    [HttpPost("kapat")]
    [ProducesResponseType(typeof(VardiyaKapanisResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VardiyaKapanisResponseDto>> Kapat([FromBody] VardiyaKapatRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _vardiyaServisi.VardiyaKapatAsync(request.VardiyaId, request.SayilanNakit, request.Aciklama, cancellationToken);
        return Ok(response);
    }

    [HttpGet("aktif")]
    [ProducesResponseType(typeof(VardiyaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VardiyaResponseDto>> Aktif([FromQuery] long cihazId, CancellationToken cancellationToken)
    {
        var response = await _vardiyaServisi.AktifVardiyaGetirAsync(cihazId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
