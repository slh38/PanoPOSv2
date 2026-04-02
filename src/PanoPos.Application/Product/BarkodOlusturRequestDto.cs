using System.ComponentModel.DataAnnotations;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class BarkodOlusturRequestDto
{
    public long? UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    [Required]
    public string BarkodNo { get; set; } = string.Empty;
    public BarkodTipi BarkodTipi { get; set; }
}
