using PanoPos.Application.Common;

namespace PanoPos.Application.Order;

public interface ISiparisServisi
{
    Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisSatirEkleAsync(long id, SiparisSatirEkleRequestDto request, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<SiparisListeItemDto>> SiparisListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisIptalAsync(long id, SiparisIptalRequestDto? request = null, CancellationToken cancellationToken = default);
}
