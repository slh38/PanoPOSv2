namespace PanoPos.Application.Order;

public sealed class SiparisSatirEkleRequestDto
{
    public long UrunId { get; set; }
    public long? UrunVaryantId { get; set; }
    public long? UrunSatisBirimiId { get; set; }
    public long? FiyatTipiId { get; set; }
    public string? FiyatParaBirimKodu { get; set; }
    public decimal? FiyatKur { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal? IndirimTutari { get; set; }
    public string? Aciklama { get; set; }
}
