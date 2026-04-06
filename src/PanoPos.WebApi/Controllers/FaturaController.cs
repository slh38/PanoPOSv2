using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/fatura")]
public sealed class FaturaController : ControllerBase
{
    private readonly IFaturaServisi _faturaServisi;

    public FaturaController(IFaturaServisi faturaServisi)
    {
        _faturaServisi = faturaServisi;
    }

    [HttpPost("olustur-siparisten")]
    public async Task<ActionResult<FaturaDto>> SiparistenOlustur([FromBody] SiparistenFaturaOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _faturaServisi.SiparistenFaturaOlusturAsync(request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FaturaDto>> Getir(long id, CancellationToken cancellationToken)
    {
        return Ok(await _faturaServisi.FaturaGetirAsync(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<FaturaListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] int? durum, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _faturaServisi.FaturaListeleAsync(subeId, durum, page, pageSize, cancellationToken));
    }

    [HttpPost("{id:long}/kapat")]
    public async Task<ActionResult<FaturaDto>> Kapat(long id, [FromBody] FaturaKapatRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _faturaServisi.FaturaKapatAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:long}/iptal")]
    public async Task<ActionResult<FaturaDto>> Iptal(long id, [FromBody] FaturaIptalRequestDto? request, CancellationToken cancellationToken)
    {
        return Ok(await _faturaServisi.FaturaIptalAsync(id, request, cancellationToken));
    }
}
