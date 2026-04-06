using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Payment;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/tahsilat")]
public sealed class TahsilatController : ControllerBase
{
    private readonly ITahsilatServisi _tahsilatServisi;

    public TahsilatController(ITahsilatServisi tahsilatServisi)
    {
        _tahsilatServisi = tahsilatServisi;
    }

    [HttpPost]
    public async Task<ActionResult<TahsilatDto>> Olustur([FromBody] TahsilatOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _tahsilatServisi.TahsilatOlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<TahsilatListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _tahsilatServisi.TahsilatListeleAsync(subeId, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TahsilatDto>> Getir(long id, CancellationToken cancellationToken)
    {
        return Ok(await _tahsilatServisi.TahsilatGetirAsync(id, cancellationToken));
    }
}
