using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class UrunDto
{
    public long Id { get; set; }
    public string? UrunKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public UrunTipi UrunTipi { get; set; }
    public long? UrunKategoriId { get; set; }
    public string? UrunKategoriAd { get; set; }
    public long? UrunGrupId { get; set; }
    public string? UrunGrupAd { get; set; }
    public bool AktifMi { get; set; }
    public List<UrunVaryantDto> Varyantlar { get; set; } = new();
    public List<BarkodDto> Barkodlar { get; set; } = new();
}
