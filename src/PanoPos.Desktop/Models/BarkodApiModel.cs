namespace PanoPos.Desktop.Models;

public sealed class BarkodApiModel
{
    public long Id { get; set; }
    public string BarkodNo { get; set; } = string.Empty;
    public long? UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public string? UrunAd { get; set; }
}
