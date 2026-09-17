namespace PanoPos.Application.Product;

public sealed class SatisStokDto
{
    public string? BarkodNo { get; set; }
    public long StokKartId { get; set; }
    public string StokKartAdi { get; set; } = string.Empty;
    public long? StokKartVaryantId { get; set; }
    public string? VaryantAdi { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public long FiyatTipiId { get; set; }
    public string FiyatTipiAdi { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public string FiyatParaBirimKodu { get; set; } = string.Empty;
    public long KdvId { get; set; }
    public decimal KdvOrani { get; set; }
}
