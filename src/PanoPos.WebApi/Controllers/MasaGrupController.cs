using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Restaurant;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/masa-grup")]
public sealed class MasaGrupController : ControllerBase
{
    private readonly IMasaGrupServisi _masaGrupServisi;

    public MasaGrupController(IMasaGrupServisi masaGrupServisi)
    {
        _masaGrupServisi = masaGrupServisi;
    }

    [HttpPost]
    public async Task<ActionResult<MasaGrupDto>> Olustur([FromBody] MasaGrupOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _masaGrupServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<MasaGrupDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _masaGrupServisi.ListeleAsync(cancellationToken));
    }
}
