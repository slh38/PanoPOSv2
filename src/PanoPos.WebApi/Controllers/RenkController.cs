using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/renk")]
public sealed class RenkController : ControllerBase
{
    private readonly IRenkServisi _renkServisi;

    public RenkController(IRenkServisi renkServisi)
    {
        _renkServisi = renkServisi;
    }

    [HttpPost]
    public async Task<ActionResult<RenkDto>> Olustur([FromBody] RenkOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _renkServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<RenkDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _renkServisi.ListeleAsync(cancellationToken));
    }
}
