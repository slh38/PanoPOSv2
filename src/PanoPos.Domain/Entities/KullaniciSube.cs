using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class KullaniciSube : BaseEntity
{
    public long KullaniciId { get; set; }
    public long BagliSubeId { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Sube Sube { get; set; } = null!;
}
