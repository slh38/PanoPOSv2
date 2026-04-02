using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/beden")]
public sealed class BedenController : ControllerBase
{
    private readonly IBedenServisi _bedenServisi;

    public BedenController(IBedenServisi bedenServisi)
    {
        _bedenServisi = bedenServisi;
    }

    [HttpPost]
    public async Task<ActionResult<BedenDto>> Olustur([FromBody] BedenOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _bedenServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<BedenDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _bedenServisi.ListeleAsync(cancellationToken));
    }
}
