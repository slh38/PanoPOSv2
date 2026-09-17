using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;
namespace PanoPos.Domain.Entities;
public sealed class AlisFatura : BaseEntity
{
    public long CariId { get; set; }
    public string FaturaNo { get; set; } = string.Empty;
    public DateTime FaturaTarihi { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; } = 1m;
    public bool KdvDahilMi { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal ToplamMatrah { get; set; }
    public decimal ToplamKdv { get; set; }
    public decimal NetToplam { get; set; }
    public AlisFaturaDurumu Durum { get; set; }
    public string? Aciklama { get; set; }
    public Cari Cari { get; set; } = null!;
    public ICollection<AlisFaturaDetay> Detaylar { get; set; } = new List<AlisFaturaDetay>();
}
