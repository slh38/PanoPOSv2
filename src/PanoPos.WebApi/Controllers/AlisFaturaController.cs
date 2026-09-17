using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Purchase;
namespace PanoPos.WebApi.Controllers;
[ApiController]
[Route("api/v1/alis-fatura")]
public sealed class AlisFaturaController(IAlisFaturaServisi service) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Create(AlisFaturaKaydetRequest request, CancellationToken ct) => Ok(await service.CreateAsync(request, ct));
    [HttpPut("{id:long}")] public async Task<IActionResult> Update(long id, AlisFaturaKaydetRequest request, CancellationToken ct) => Ok(await service.UpdateAsync(id, request, ct));
    [HttpGet("{id:long}")] public async Task<IActionResult> Get(long id, long subeId, CancellationToken ct) => Ok(await service.GetByIdAsync(id, subeId, ct));
    [HttpGet] public async Task<IActionResult> List([FromQuery] AlisFaturaFiltre filtre, CancellationToken ct) => Ok(await service.GetPagedAsync(filtre, ct));
    [HttpDelete("{id:long}")] public async Task<IActionResult> Delete(long id, long subeId, CancellationToken ct) { await service.DeleteAsync(id, subeId, ct); return NoContent(); }
    [HttpPost("{id:long}/kesinlestir")] public async Task<IActionResult> Finalize(long id, long subeId, CancellationToken ct) => Ok(await service.KesinlestirAsync(id, subeId, ct));
    [HttpPost("{id:long}/iptal")] public async Task<IActionResult> Cancel(long id, long subeId, CancellationToken ct) => Ok(await service.IptalAsync(id, subeId, ct));
}
