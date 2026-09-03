using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Product;

public sealed class StokKategoriOlusturRequestDto
{
    [Range(1, long.MaxValue)]
    public long SubeId { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    public string? Kod { get; set; }
}
