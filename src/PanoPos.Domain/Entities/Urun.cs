using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class StokKart : BaseEntity
{
    public string? StokKartKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public StokKartTipi StokKartTipi { get; set; }
    public long? StokKategoriId { get; set; }
    public long? StokGrupId { get; set; }

    public StokKategori? StokKategori { get; set; }
    public StokGrup? StokGrup { get; set; }
    public ICollection<StokKartVaryant> Varyantlar { get; set; } = new List<StokKartVaryant>();
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
    public ICollection<StokKartSatisBirimi> SatisBirimleri { get; set; } = new List<StokKartSatisBirimi>();
}
