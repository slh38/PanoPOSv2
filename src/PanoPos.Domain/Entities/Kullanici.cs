using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Kullanici : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public DateTime? PinSonDegistirmeTarihi { get; set; }
    public DateTime? SonGirisTarihi { get; set; }
    public int BasarisizGirisSayisi { get; set; }
    public bool KilitliMi { get; set; }

    public ICollection<KullaniciRol> KullaniciRoller { get; set; } = new List<KullaniciRol>();
    public ICollection<KullaniciSube> KullaniciSubeler { get; set; } = new List<KullaniciSube>();
    public ICollection<KullaniciOturum> KullaniciOturumlar { get; set; } = new List<KullaniciOturum>();
}
