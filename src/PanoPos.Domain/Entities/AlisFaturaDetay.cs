using PanoPos.Domain.Common;
namespace PanoPos.Domain.Entities;
public sealed class AlisFaturaDetay : BaseEntity
{
    public long AlisFaturaId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public string FiyatParaBirimKodu { get; set; } = "TRY";
    public decimal FiyatKur { get; set; } = 1m;
    public decimal SatirAraToplam { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal GenelIndirimPayi { get; set; }
    public long KdvId { get; set; }
    public decimal KdvOrani { get; set; }
    public bool KdvDahilMi { get; set; }
    public decimal Matrah { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal SatirNetToplam { get; set; }
    public AlisFatura AlisFatura { get; set; } = null!;
}
