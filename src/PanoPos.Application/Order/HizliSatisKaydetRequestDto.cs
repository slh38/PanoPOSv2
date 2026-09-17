using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Order;

public sealed class HizliSatisKaydetRequestDto
{
    public Guid? Surum { get; set; }
    public long? CariId { get; set; }
    [Range(1, long.MaxValue)] public long FiyatTipiId { get; set; }
    [Required, StringLength(10)] public string BelgeParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; } = 1m;
    public decimal? GenelIndirimOrani { get; set; }
    public decimal? GenelIndirimTutari { get; set; }
    [StringLength(500)] public string? Aciklama { get; set; }
    [Required, MinLength(1), MaxLength(500)] public List<HizliSatisSatirRequestDto> Satirlar { get; set; } = [];
}

public sealed class HizliSatisSatirRequestDto
{
    public long? SiparisDetayId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public decimal Miktar { get; set; }
    public long? FiyatTipiId { get; set; }
    public decimal? FiyatKur { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal? IndirimTutari { get; set; }
}

public sealed class BekleyenHizliSatisDto
{
    public long Id { get; set; }
    public string SiparisNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public long? CariId { get; set; }
    public string? CariAdi { get; set; }
    public int SatirSayisi { get; set; }
    public decimal NetToplam { get; set; }
    public string ParaBirimKodu { get; set; } = "";
    public string? Aciklama { get; set; }
}
