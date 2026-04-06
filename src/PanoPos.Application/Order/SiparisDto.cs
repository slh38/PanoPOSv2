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
    public decimal ToplamTutar { get; set; }
    public SiparisDurumu Durum { get; set; }
    public bool AktifMi { get; set; }
    public List<SiparisDetayDto> Detaylar { get; set; } = new();
}
