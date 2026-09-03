using PanoPos.Application.Common;

namespace PanoPos.Application.Invoice;

public interface IFaturaServisi
{
    Task<FaturaDto> SiparistenFaturaOlusturAsync(SiparistenFaturaOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<FaturaDto> FaturaGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<FaturaListeItemDto>> FaturaListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FaturaDto> FaturaKapatAsync(long id, FaturaKapatRequestDto request, CancellationToken cancellationToken = default);
    Task<FaturaDto> FaturaIptalAsync(long id, FaturaIptalRequestDto? request = null, CancellationToken cancellationToken = default);
}
