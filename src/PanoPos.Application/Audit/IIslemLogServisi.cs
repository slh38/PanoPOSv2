using PanoPos.Application.Common;

namespace PanoPos.Application.Audit;

public interface IIslemLogServisi
{
    Task<IslemLogDto> LogEkleAsync(IslemLogEkleRequestDto request, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<IslemLogListeItemDto>> ListeleAsync(IslemLogListeRequestDto request, CancellationToken cancellationToken = default);
    Task<IslemLogDto> DetayGetirAsync(long id, CancellationToken cancellationToken = default);
}
