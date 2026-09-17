namespace PanoPos.Application.Product;

public sealed class StokKartVaryantDto
{
    public long Id { get; set; }
    public long StokKartId { get; set; }
    public long? RenkId { get; set; }
    public string? RenkAd { get; set; }
    public long? BedenId { get; set; }
    public string? BedenAd { get; set; }
    public string VaryantKodu { get; set; } = string.Empty;
    public bool BarkodluMu { get; set; }
}
