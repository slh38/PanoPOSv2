using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class StokFisDetay : BaseEntity
{
    public long StokFisId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public decimal Miktar { get; set; }
    public string? Aciklama { get; set; }
    // Count snapshots are in base units, independent of the selected unit.
    public decimal? SistemMiktari { get; set; }
    public decimal? SayilanMiktar { get; set; }
    public decimal? FarkMiktari { get; set; }
    public StokFis StokFis { get; set; } = null!;
}
