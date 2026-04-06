using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class FaturaDetay : BaseEntity
{
    public long FaturaId { get; set; }
    public long UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal SatirToplam { get; set; }
    public string? Aciklama { get; set; }

    public Fatura Fatura { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
    public UrunVaryant? UrunVaryant { get; set; }
}
