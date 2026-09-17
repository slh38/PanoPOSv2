using PanoPos.Application.Common;

namespace PanoPos.Application.Customer;

public interface ICariKartServisi
{
    Task<CariKartDto> CariKartOlusturAsync(CariKartOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<CariKartDto> CariKartGuncelleAsync(long id, CariKartGuncelleRequestDto request, CancellationToken cancellationToken = default);
    Task<CariKartDto> CariKartGetirAsync(long id, long subeId, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<CariKartListeItemDto>> CariKartListeleAsync(long subeId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
}
