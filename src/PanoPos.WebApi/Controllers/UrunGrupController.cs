using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun-grup")]
public sealed class StokGrupController : ControllerBase
{
    private readonly IStokGrupServisi _urunGrupServisi;

    public StokGrupController(IStokGrupServisi urunGrupServisi)
    {
        _urunGrupServisi = urunGrupServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokGrupDto>> Olustur([FromBody] StokGrupOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunGrupServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<StokGrupDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _urunGrupServisi.ListeleAsync(cancellationToken));
    }
}
