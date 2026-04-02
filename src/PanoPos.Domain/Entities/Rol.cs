using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Rol : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public ICollection<KullaniciRol> KullaniciRoller { get; set; } = new List<KullaniciRol>();
}
