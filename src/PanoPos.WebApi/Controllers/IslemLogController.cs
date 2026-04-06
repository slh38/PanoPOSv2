using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Audit;
using PanoPos.Application.Common;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/log/islem")]
public sealed class IslemLogController : ControllerBase
{
    private readonly IIslemLogServisi _islemLogServisi;

    public IslemLogController(IIslemLogServisi islemLogServisi)
    {
        _islemLogServisi = islemLogServisi;
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<IslemLogListeItemDto>>> Listele([FromQuery] IslemLogListeRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _islemLogServisi.ListeleAsync(request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<IslemLogDto>> Detay(long id, CancellationToken cancellationToken)
    {
        return Ok(await _islemLogServisi.DetayGetirAsync(id, cancellationToken));
    }
}
