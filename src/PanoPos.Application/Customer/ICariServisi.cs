using PanoPos.Application.Common;

namespace PanoPos.Application.Customer;

public interface ICariServisi
{
    Task<CariDto> CariOlusturAsync(CariOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<CariDto> CariGuncelleAsync(long id, CariGuncelleRequestDto request, CancellationToken cancellationToken = default);
    Task<CariDto> CariGetirAsync(long id, long subeId, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<CariListeItemDto>> CariListeleAsync(long subeId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
}
