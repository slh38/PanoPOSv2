using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun-grup")]
public sealed class UrunGrupController : ControllerBase
{
    private readonly IUrunGrupServisi _urunGrupServisi;

    public UrunGrupController(IUrunGrupServisi urunGrupServisi)
    {
        _urunGrupServisi = urunGrupServisi;
    }

    [HttpPost]
    public async Task<ActionResult<UrunGrupDto>> Olustur([FromBody] UrunGrupOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunGrupServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<UrunGrupDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _urunGrupServisi.ListeleAsync(cancellationToken));
    }
}
