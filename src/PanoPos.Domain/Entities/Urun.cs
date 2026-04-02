using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Urun : BaseEntity
{
    public string? UrunKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public UrunTipi UrunTipi { get; set; }

    public ICollection<UrunVaryant> Varyantlar { get; set; } = new List<UrunVaryant>();
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
}
