using PanoPos.Domain.Enums;

namespace PanoPos.Application.Order;

public sealed class SiparisListeItemDto
{
    public long Id { get; set; }
    public string SiparisNo { get; set; } = string.Empty;
    public SiparisTipi SiparisTipi { get; set; }
    public SiparisDurumu Durum { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
}
