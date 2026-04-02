using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Product;

public sealed class RenkOlusturRequestDto
{
    [Range(1, long.MaxValue)]
    public long SubeId { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    [Required]
    public string Kod { get; set; } = string.Empty;
}
