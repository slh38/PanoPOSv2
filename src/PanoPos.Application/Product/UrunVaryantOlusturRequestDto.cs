using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Product;

public sealed class UrunVaryantOlusturRequestDto
{
    public long? RenkId { get; set; }
    public long? BedenId { get; set; }
    [Required]
    public string VaryantKodu { get; set; } = string.Empty;
    public bool BarkodluMu { get; set; }
}
