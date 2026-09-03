using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class BarkodDto
{
    public long Id { get; set; }
    public string BarkodNo { get; set; } = string.Empty;
    public BarkodTipi BarkodTipi { get; set; }
    public long? StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public string? StokKartAd { get; set; }
    public string? VaryantKodu { get; set; }
}
