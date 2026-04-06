using PanoPos.Domain.Enums;

namespace PanoPos.Application.Order;

public sealed class SiparisDto
{
    public long Id { get; set; }
    public string SiparisNo { get; set; } = string.Empty;
    public SiparisTipi SiparisTipi { get; set; }
    public long? AdisyonId { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public decimal ToplamTutar { get; set; }
    public SiparisDurumu Durum { get; set; }
    public bool AktifMi { get; set; }
    public List<SiparisDetayDto> Detaylar { get; set; } = new();
}
