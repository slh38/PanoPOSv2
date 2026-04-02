using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class UrunVaryant : BaseEntity
{
    public long UrunId { get; set; }
    public long? RenkId { get; set; }
    public long? BedenId { get; set; }
    public string VaryantKodu { get; set; } = string.Empty;
    public bool BarkodluMu { get; set; }

    public Urun Urun { get; set; } = null!;
    public Renk? Renk { get; set; }
    public Beden? Beden { get; set; }
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
}
