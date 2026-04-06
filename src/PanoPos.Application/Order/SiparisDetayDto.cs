namespace PanoPos.Application.Order;

public sealed class SiparisDetayDto
{
    public long Id { get; set; }
    public long UrunId { get; set; }
    public string UrunAd { get; set; } = string.Empty;
    public long? UrunVaryantId { get; set; }
    public string? VaryantKodu { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal SatirAraToplam { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal SatirNetToplam { get; set; }
    public decimal SatirToplam { get; set; }
    public string? Aciklama { get; set; }
}
