namespace PanoPos.Application.Order;

public sealed class SiparisSatirEkleRequestDto
{
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long? StokKartSatisBirimiId { get; set; }
    public long? FiyatTipiId { get; set; }
    // Legacy client fields are not authoritative; the master price supplies currency and price.
    public string? FiyatParaBirimKodu { get; set; }
    // One unit of the price currency expressed in the document currency.
    public decimal? FiyatKur { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal? IndirimTutari { get; set; }
    public string? Aciklama { get; set; }
}
