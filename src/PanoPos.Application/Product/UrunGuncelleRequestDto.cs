using System.ComponentModel.DataAnnotations;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class UrunGuncelleRequestDto
{
    public string? UrunKodu { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public UrunTipi UrunTipi { get; set; }
    public bool AktifMi { get; set; }
}
