using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class KullaniciRol : BaseEntity
{
    public long KullaniciId { get; set; }
    public long RolId { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
