namespace PanoPos.Application.Product;

public sealed class StokKartFiyatGuncelleRequestDto
{
    public decimal Fiyat { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
}
