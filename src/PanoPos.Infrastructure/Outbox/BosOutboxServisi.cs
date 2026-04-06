using PanoPos.Application.Common;
using PanoPos.Application.Outbox;

namespace PanoPos.Infrastructure.Outbox;

public sealed class BosOutboxServisi : IOutboxServisi
{
    public Task<OutboxOlayDto> OlayEkleAsync(OutboxOlayEkleRequestDto request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OutboxOlayDto
        {
            TenantId = request.TenantId,
            SubeId = request.SubeId,
            CihazId = request.CihazId,
            OlayTipi = request.OlayTipi,
            KaynakTablo = request.KaynakTablo,
            KaynakId = request.KaynakId,
            PayloadJson = request.PayloadJson,
            Durum = request.Durum,
            DenemeSayisi = request.DenemeSayisi,
            OlusturmaTarihi = request.OlusturmaTarihi ?? DateTime.UtcNow
        });
    }

    public Task<SayfaliSonucDto<OutboxListeItemDto>> BekleyenleriListeleAsync(long subeId, PanoPos.Domain.Enums.OutboxDurumu? durum, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SayfaliSonucDto<OutboxListeItemDto>
        {
            Sayfa = page,
            SayfaBoyutu = pageSize
        });
    }

    public Task<OutboxOlayDto> GonderildiIsaretleAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OutboxOlayDto { Id = id, Durum = PanoPos.Domain.Enums.OutboxDurumu.Gonderildi, GonderimTarihi = DateTime.UtcNow });
    }

    public Task<OutboxOlayDto> HataIsaretleAsync(long id, string hataMesaji, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OutboxOlayDto { Id = id, Durum = PanoPos.Domain.Enums.OutboxDurumu.Hata, SonHataMesaji = hataMesaji, DenemeSayisi = 1 });
    }

    public Task<OutboxOlayDto> GetirAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OutboxOlayDto { Id = id });
    }
}
