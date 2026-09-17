using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Tax;
namespace PanoPos.WebApi.Controllers;
[ApiController]
[Route("api/v1/kdv")]
public sealed class KdvController(IKdvServisi service) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Create(KdvKaydetRequest request, CancellationToken ct) => Ok(await service.CreateAsync(request, ct));
    [HttpPut("{id:long}")] public async Task<IActionResult> Update(long id, KdvKaydetRequest request, CancellationToken ct) => Ok(await service.UpdateAsync(id, request, ct));
    [HttpGet("{id:long}")] public async Task<IActionResult> Get(long id, long subeId, CancellationToken ct) => Ok(await service.GetByIdAsync(id, subeId, ct));
    [HttpGet] public async Task<IActionResult> List(long subeId, string? arama, bool? aktifMi, int page = 1, int pageSize = 50, CancellationToken ct = default) => Ok(await service.GetPagedAsync(subeId, arama, aktifMi, page, pageSize, ct));
    [HttpDelete("{id:long}")] public async Task<IActionResult> Delete(long id, long subeId, CancellationToken ct) { await service.DeleteAsync(id, subeId, ct); return NoContent(); }
}
