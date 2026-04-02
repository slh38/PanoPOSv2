using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/barkod")]
public sealed class BarkodController : ControllerBase
{
    private readonly IBarkodServisi _barkodServisi;

    public BarkodController(IBarkodServisi barkodServisi)
    {
        _barkodServisi = barkodServisi;
    }

    [HttpPost]
    public async Task<ActionResult<BarkodDto>> Olustur([FromBody] BarkodOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _barkodServisi.BarkodOlusturAsync(request, cancellationToken));
    }

    [HttpGet("{barkodNo}")]
    public async Task<ActionResult<BarkodDto>> Getir(string barkodNo, CancellationToken cancellationToken)
    {
        var sonuc = await _barkodServisi.BarkodIleBulAsync(barkodNo, cancellationToken);
        return sonuc is null ? NotFound() : Ok(sonuc);
    }
}
