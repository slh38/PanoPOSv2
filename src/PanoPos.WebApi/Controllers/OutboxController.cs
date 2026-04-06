using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Enums;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/outbox")]
public sealed class OutboxController : ControllerBase
{
    private readonly IOutboxServisi _outboxServisi;

    public OutboxController(IOutboxServisi outboxServisi)
    {
        _outboxServisi = outboxServisi;
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<OutboxListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] OutboxDurumu? durum, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _outboxServisi.BekleyenleriListeleAsync(subeId, durum, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OutboxOlayDto>> Getir(long id, CancellationToken cancellationToken)
    {
        return Ok(await _outboxServisi.GetirAsync(id, cancellationToken));
    }
}
