using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Product;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stok-kategori")]
public sealed class StokKategoriController : ControllerBase
{
    private readonly IStokKategoriServisi _stokKategoriServisi;

    public StokKategoriController(IStokKategoriServisi stokKategoriServisi)
    {
        _stokKategoriServisi = stokKategoriServisi;
    }

    [HttpPost]
    public async Task<ActionResult<StokKategoriDto>> Olustur([FromBody] StokKategoriOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _stokKategoriServisi.OlusturAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<StokKategoriDto>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _stokKategoriServisi.ListeleAsync(cancellationToken));
    }
}
