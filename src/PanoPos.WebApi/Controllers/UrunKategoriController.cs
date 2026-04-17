using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/urun-kategori")]
public sealed class UrunKategoriController : ControllerBase
{
    private readonly IUrunKategoriServisi _urunKategoriServisi;

    public UrunKategoriController(IUrunKategoriServisi urunKategoriServisi)
    {
        _urunKategoriServisi = urunKategoriServisi;
    }

    [HttpPost]
    public async Task<ActionResult<UrunKategoriDto>> Olustur([FromBody] UrunKategoriOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _urunKategoriServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<UrunKategoriDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _urunKategoriServisi.ListeleAsync(cancellationToken));
    }
}
