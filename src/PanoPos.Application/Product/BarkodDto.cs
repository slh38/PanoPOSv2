using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class BarkodDto
{
    public long Id { get; set; }
    public string BarkodNo { get; set; } = string.Empty;
    public BarkodTipi BarkodTipi { get; set; }
    public long? UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public string? UrunAd { get; set; }
    public string? VaryantKodu { get; set; }
}
