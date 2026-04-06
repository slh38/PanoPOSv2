using PanoPos.Domain.Enums;

namespace PanoPos.Application.Outbox;

public sealed class OutboxListeItemDto
{
    public long Id { get; set; }
    public string OlayTipi { get; set; } = string.Empty;
    public string KaynakTablo { get; set; } = string.Empty;
    public long KaynakId { get; set; }
    public OutboxDurumu Durum { get; set; }
    public int DenemeSayisi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GonderimTarihi { get; set; }
}
