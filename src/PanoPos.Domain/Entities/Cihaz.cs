using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Cihaz : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public Sube Sube { get; set; } = null!;
    public ICollection<KullaniciOturum> KullaniciOturumlar { get; set; } = new List<KullaniciOturum>();
}
