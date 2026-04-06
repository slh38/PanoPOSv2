using PanoPos.Application.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Outbox;

public interface IOutboxServisi
{
    Task<OutboxOlayDto> OlayEkleAsync(OutboxOlayEkleRequestDto request, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<OutboxListeItemDto>> BekleyenleriListeleAsync(long subeId, OutboxDurumu? durum, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OutboxOlayDto> GonderildiIsaretleAsync(long id, CancellationToken cancellationToken = default);
    Task<OutboxOlayDto> HataIsaretleAsync(long id, string hataMesaji, CancellationToken cancellationToken = default);
    Task<OutboxOlayDto> GetirAsync(long id, CancellationToken cancellationToken = default);
}
