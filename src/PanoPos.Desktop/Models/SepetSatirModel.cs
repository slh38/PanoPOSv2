namespace PanoPos.Desktop.Models;

public sealed class SepetSatirModel
{
    public long UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal SatirNetToplam { get; set; }
    public string BarkodNo { get; set; } = string.Empty;
}
