using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class StokKartListeItemDto
{
    public long Id { get; set; }
    public string? StokKartKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public StokKartTipi StokKartTipi { get; set; }
    public long? StokKategoriId { get; set; }
    public string? StokKategoriAd { get; set; }
    public long? StokGrupId { get; set; }
    public string? StokGrupAd { get; set; }
    public bool AktifMi { get; set; }
}
