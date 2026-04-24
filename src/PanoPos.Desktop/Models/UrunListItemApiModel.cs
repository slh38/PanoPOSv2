namespace PanoPos.Desktop.Models;

public sealed class UrunListItemApiModel
{
    public long Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public long? UrunKategoriId { get; set; }
    public string? UrunKategoriAd { get; set; }
    public bool AktifMi { get; set; }
}
