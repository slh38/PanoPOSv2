using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-grup")]
public sealed class StokGrupController : ControllerBase
{
    private readonly IStokGrupServisi _stokGrupServisi;

    public StokGrupController(IStokGrupServisi stokGrupServisi)
    {
        _stokGrupServisi = stokGrupServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokGrupDto>> Olustur([FromBody] StokGrupOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _stokGrupServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<StokGrupDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _stokGrupServisi.ListeleAsync(cancellationToken));
    }
}
