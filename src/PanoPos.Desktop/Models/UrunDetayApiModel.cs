namespace PanoPos.Desktop.Models;

public sealed class UrunDetayApiModel
{
    public long Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public long? UrunKategoriId { get; set; }
    public string? UrunKategoriAd { get; set; }
}
