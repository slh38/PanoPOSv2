using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Fatura : BaseEntity
{
    public string FaturaNo { get; set; } = string.Empty;
    public long? SiparisId { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal KalanTutar { get; set; }
    public decimal ToplamTutar { get; set; }
    public FaturaDurumu Durum { get; set; }
    public DateTime? KapanisTarihi { get; set; }
    public long? KapatanKullaniciId { get; set; }

    public Siparis? Siparis { get; set; }
    public Cari? Cari { get; set; }
    public ICollection<FaturaDetay> Detaylar { get; set; } = new List<FaturaDetay>();
}
