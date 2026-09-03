using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Renk : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public ICollection<StokKartVaryant> StokKartVaryantlari { get; set; } = new List<StokKartVaryant>();
}
