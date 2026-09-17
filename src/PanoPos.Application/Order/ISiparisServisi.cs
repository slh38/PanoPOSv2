using PanoPos.Application.Common;

namespace PanoPos.Application.Order;

public interface ISiparisServisi
{
    Task<SiparisDto> HizliSatisKaydetAsync(long? id, HizliSatisKaydetRequestDto request, CancellationToken cancellationToken = default);
    Task<SiparisDto> BekleyenHizliSatisGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<BekleyenHizliSatisDto>> BekleyenHizliSatisListeleAsync(string? arama, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisSatirEkleAsync(long id, SiparisSatirEkleRequestDto request, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<SiparisListeItemDto>> SiparisListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SiparisDto> SiparisIptalAsync(long id, SiparisIptalRequestDto? request = null, CancellationToken cancellationToken = default);
}
