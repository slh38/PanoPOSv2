using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;
using PanoPos.Application.Order;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/siparis")]
public sealed class SiparisController : ControllerBase
{
    private readonly ISiparisServisi _siparisServisi;

    public SiparisController(ISiparisServisi siparisServisi)
    {
        _siparisServisi = siparisServisi;
    }

    [HttpPost]
    public async Task<ActionResult<SiparisDto>> Olustur([FromBody] SiparisOlusturRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _siparisServisi.SiparisOlusturAsync(request, cancellationToken));
    }

    [HttpPost("{id:long}/satir")]
    public async Task<ActionResult<SiparisDto>> SatirEkle(long id, [FromBody] SiparisSatirEkleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _siparisServisi.SiparisSatirEkleAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SiparisDto>> Getir(long id, CancellationToken cancellationToken)
    {
        return Ok(await _siparisServisi.SiparisGetirAsync(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SayfaliSonucDto<SiparisListeItemDto>>> Listele([FromQuery] long subeId, [FromQuery] int? durum, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _siparisServisi.SiparisListeleAsync(subeId, durum, page, pageSize, cancellationToken));
    }

    [HttpPost("{id:long}/iptal")]
    public async Task<ActionResult<SiparisDto>> Iptal(long id, [FromBody] SiparisIptalRequestDto? request, CancellationToken cancellationToken)
    {
        return Ok(await _siparisServisi.SiparisIptalAsync(id, request, cancellationToken));
    }
}
