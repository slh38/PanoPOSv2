using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Beden : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public ICollection<UrunVaryant> UrunVaryantlari { get; set; } = new List<UrunVaryant>();
}
