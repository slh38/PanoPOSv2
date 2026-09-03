using System.ComponentModel.DataAnnotations;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public class UrunOlusturRequestDto
{
    [Range(1, long.MaxValue)]
    public long SubeId { get; set; }
    public string? UrunKodu { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public UrunTipi UrunTipi { get; set; }
    public long? UrunKategoriId { get; set; }
    public long? UrunGrupId { get; set; }
}

