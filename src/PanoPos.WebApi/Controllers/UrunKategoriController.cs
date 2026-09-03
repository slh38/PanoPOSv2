using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun-kategori")]
public sealed class StokKategoriController : ControllerBase
{
    private readonly IStokKategoriServisi _urunKategoriServisi;

    public StokKategoriController(IStokKategoriServisi urunKategoriServisi)
    {
        _urunKategoriServisi = urunKategoriServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokKategoriDto>> Olustur([FromBody] StokKategoriOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunKategoriServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<StokKategoriDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _urunKategoriServisi.ListeleAsync(cancellationToken));
    }
}
