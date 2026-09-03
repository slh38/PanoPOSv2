using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class StokKartVaryant : BaseEntity
{
    public long StokKartId { get; set; }
    public long? RenkId { get; set; }
    public long? BedenId { get; set; }
    public string VaryantKodu { get; set; } = string.Empty;
    public bool BarkodluMu { get; set; }

    public StokKart StokKart { get; set; } = null!;
    public Renk? Renk { get; set; }
    public Beden? Beden { get; set; }
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
}
