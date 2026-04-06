using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Siparis : BaseEntity
{
    public string SiparisNo { get; set; } = string.Empty;
    public SiparisTipi SiparisTipi { get; set; }
    public long? AdisyonId { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public decimal ToplamTutar { get; set; }
    public SiparisDurumu Durum { get; set; }

    public Adisyon? Adisyon { get; set; }
    public Cari? Cari { get; set; }
    public ICollection<SiparisDetay> Detaylar { get; set; } = new List<SiparisDetay>();
}
