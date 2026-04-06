using PanoPos.Domain.Enums;

namespace PanoPos.Application.Outbox;

public sealed class OutboxOlayEkleRequestDto
{
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long CihazId { get; set; }
    public string OlayTipi { get; set; } = string.Empty;
    public string KaynakTablo { get; set; } = string.Empty;
    public long KaynakId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public OutboxDurumu Durum { get; set; } = OutboxDurumu.Bekliyor;
    public int DenemeSayisi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
}
