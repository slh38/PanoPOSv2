using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Warehouse;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/depo")]
public sealed class DepoController : ControllerBase
{
    private readonly IDepoServisi _depoServisi;

    public DepoController(IDepoServisi depoServisi)
    {
        _depoServisi = depoServisi;
    }

    [HttpPost]
    public async Task<ActionResult<DepoDto>> Create(DepoKaydetRequestDto request, CancellationToken cancellationToken)
        => Ok(await _depoServisi.CreateAsync(request, cancellationToken));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<DepoDto>> Update(long id, DepoKaydetRequestDto request, CancellationToken cancellationToken)
        => Ok(await _depoServisi.UpdateAsync(id, request, cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DepoDto>> GetById(long id, [FromQuery] long subeId, CancellationToken cancellationToken)
        => Ok(await _depoServisi.GetByIdAsync(id, subeId, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<DepoDto>>> GetPaged([FromQuery] long subeId, [FromQuery] string? arama, [FromQuery] bool? aktifMi, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        => Ok(await _depoServisi.GetPagedAsync(subeId, arama, aktifMi, page, pageSize, cancellationToken));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, [FromQuery] long subeId, CancellationToken cancellationToken)
    {
        await _depoServisi.DeleteAsync(id, subeId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:long}/varsayilan-yap")]
    public async Task<ActionResult<DepoDto>> SetDefault(long id, [FromQuery] long subeId, CancellationToken cancellationToken)
        => Ok(await _depoServisi.SetDefaultAsync(id, subeId, cancellationToken));
}
