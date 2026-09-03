using PanoPos.Domain.Enums;

namespace PanoPos.Application.Order;

public sealed class SiparisOlusturRequestDto
{
    public long SubeId { get; set; }
    public SiparisTipi SiparisTipi { get; set; }
    public long? AdisyonId { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal? GenelIndirimTutari { get; set; }
}
