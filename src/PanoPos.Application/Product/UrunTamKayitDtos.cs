namespace PanoPos.Application.Product;

public sealed class UrunFiyatDto
{
    public long Id { get; set; }
    public long FiyatTipiId { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
}

public sealed class UrunSatisBirimiKayitDto
{
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public bool VarsayilanMi { get; set; }
    public string? BarkodNo { get; set; }
    public List<UrunFiyatDto> Fiyatlar { get; set; } = new();
}

public sealed class UrunTamKayitRequestDto : UrunOlusturRequestDto
{
    public List<UrunSatisBirimiKayitDto> SatisBirimleri { get; set; } = new();
}
