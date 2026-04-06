namespace PanoPos.Domain.Entities;

public sealed class OutboxOlay
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long CihazId { get; set; }
    public string OlayTipi { get; set; } = string.Empty;
    public string KaynakTablo { get; set; } = string.Empty;
    public long KaynakId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public PanoPos.Domain.Enums.OutboxDurumu Durum { get; set; }
    public int DenemeSayisi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GonderimTarihi { get; set; }
    public string? SonHataMesaji { get; set; }

    public Cihaz Cihaz { get; set; } = null!;
}


