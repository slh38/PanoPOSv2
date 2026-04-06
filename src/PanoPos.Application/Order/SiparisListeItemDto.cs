using PanoPos.Domain.Enums;

namespace PanoPos.Application.Order;

public sealed class SiparisListeItemDto
{
    public long Id { get; set; }
    public string SiparisNo { get; set; } = string.Empty;
    public SiparisTipi SiparisTipi { get; set; }
    public long? AdisyonId { get; set; }
    public decimal ToplamTutar { get; set; }
    public SiparisDurumu Durum { get; set; }
}
