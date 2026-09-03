using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class FaturaDetay : BaseEntity
{
    public long FaturaId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long? StokKartSatisBirimiId { get; set; }
    public string? BirimAdi { get; set; }
    public decimal? BirimKatsayi { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal SatirAraToplam { get; set; }
    public string FiyatParaBirimKodu { get; set; } = "TRY";
    public decimal FiyatKur { get; set; } = 1m;
    public decimal? IndirimOrani { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal SatirNetToplam { get; set; }
    public decimal SatirToplam { get; set; }
    public string? Aciklama { get; set; }

    public Fatura Fatura { get; set; } = null!;
    public StokKart StokKart { get; set; } = null!;
    public StokKartVaryant? StokKartVaryant { get; set; }
    public StokKartSatisBirimi? StokKartSatisBirimi { get; set; }
}
