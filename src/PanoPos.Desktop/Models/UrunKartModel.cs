namespace PanoPos.Desktop.Models;

public sealed class UrunKartModel
{
    public long UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public long? KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
}
