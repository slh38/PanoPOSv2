using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Sube : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Cihaz> Cihazlar { get; set; } = new List<Cihaz>();
    public ICollection<KullaniciSube> KullaniciSubeler { get; set; } = new List<KullaniciSube>();
}
