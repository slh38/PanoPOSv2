using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Payment;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/banka")]
public sealed class BankaController : ControllerBase
{
    private readonly IBankaServisi _bankaServisi;

    public BankaController(IBankaServisi bankaServisi)
    {
        _bankaServisi = bankaServisi;
    }

    [HttpPost]
    public async Task<ActionResult<BankaDto>> Olustur([FromBody] BankaOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _bankaServisi.BankaOlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<BankaDto>>> Listele([FromQuery] long subeId, CancellationToken cancellationToken)
    {
        return Ok(await _bankaServisi.BankaListeleAsync(subeId, cancellationToken));
    }
}
