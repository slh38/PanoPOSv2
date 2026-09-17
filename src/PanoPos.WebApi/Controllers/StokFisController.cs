using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Stock;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-fis")]
public sealed class StokFisController(IStokServisi service) : ControllerBase
{
    [HttpPost("devir")]
    public async Task<ActionResult<StokFisDto>> Devir(StokFisOlusturRequest request, CancellationToken ct)
        => Ok(await service.DevirAsync(request, ct));

    [HttpPost("sayim")]
    public async Task<ActionResult<StokFisDto>> Sayim(StokFisOlusturRequest request, CancellationToken ct)
        => Ok(await service.SayimAsync(request, ct));

    [HttpPost("depo-transfer")]
    public async Task<ActionResult<StokFisDto>> Transfer(StokFisOlusturRequest request, CancellationToken ct)
        => Ok(await service.TransferAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<StokFisDto>> Get(long id, [FromQuery] long subeId, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, subeId, ct));

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<StokFisListeDto>>> List([FromQuery] StokFisFiltre request, CancellationToken ct)
        => Ok(await service.GetPagedAsync(request, ct));

    [HttpGet("/api/v1/stok/miktar")]
    public async Task<ActionResult<StokMiktarDto>> Miktar([FromQuery] long subeId, [FromQuery] long depoId,
        [FromQuery] long stokKartId, [FromQuery] long? stokKartVaryantId, CancellationToken ct)
        => Ok(await service.GetMiktarAsync(subeId, depoId, stokKartId, stokKartVaryantId, ct));
}
