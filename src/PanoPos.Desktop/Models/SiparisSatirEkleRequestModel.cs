namespace PanoPos.Desktop.Models;

public sealed class SiparisSatirEkleRequestModel
{
    public long UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal? IndirimTutari { get; set; }
    public string? Aciklama { get; set; }
}
