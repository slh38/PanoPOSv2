using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class UrunListeItemDto
{
    public long Id { get; set; }
    public string? UrunKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public UrunTipi UrunTipi { get; set; }
    public long? UrunKategoriId { get; set; }
    public string? UrunKategoriAd { get; set; }
    public long? UrunGrupId { get; set; }
    public string? UrunGrupAd { get; set; }
    public bool AktifMi { get; set; }
}
