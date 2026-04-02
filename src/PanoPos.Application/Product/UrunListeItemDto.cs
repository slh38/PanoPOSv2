using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class UrunListeItemDto
{
    public long Id { get; set; }
    public string? UrunKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public UrunTipi UrunTipi { get; set; }
    public bool AktifMi { get; set; }
}
