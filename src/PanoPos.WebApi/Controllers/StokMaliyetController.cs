using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Stock;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-maliyet")]
public sealed class StokMaliyetController(IStokMaliyetServisi service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(long subeId, long depoId, long stokKartId,
        long? stokKartVaryantId, CancellationToken ct) =>
        Ok(await service.GetAsync(subeId, depoId, stokKartId, stokKartVaryantId, ct));
}

[ApiController]
[Route("api/v1/tenant-ayar/maliyet-yontemi")]
public sealed class MaliyetYontemiController(IStokMaliyetServisi service) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Put(MaliyetYontemiRequest request, CancellationToken ct) =>
        Ok(new { MaliyetYontemi = await service.YontemDegistirAsync(request.SubeId, request.MaliyetYontemi, ct) });
}
