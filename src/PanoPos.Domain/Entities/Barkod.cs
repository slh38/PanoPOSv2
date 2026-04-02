using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Barkod : BaseEntity
{
    public string BarkodNo { get; set; } = string.Empty;
    public BarkodTipi BarkodTipi { get; set; }
    public long? UrunId { get; set; }
    public long? UrunVaryantId { get; set; }

    public Urun? Urun { get; set; }
    public UrunVaryant? UrunVaryant { get; set; }
}
