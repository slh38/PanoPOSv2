using PanoPos.Application.Common;

namespace PanoPos.Application.Payment;

public interface ITahsilatServisi
{
    Task<TahsilatDto> TahsilatOlusturAsync(TahsilatOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<TahsilatDto> TahsilatGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<TahsilatListeItemDto>> TahsilatListeleAsync(long subeId, int page, int pageSize, CancellationToken cancellationToken = default);
}
