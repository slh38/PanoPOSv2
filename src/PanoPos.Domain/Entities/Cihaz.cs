using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Cihaz : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public long? VarsayilanKasaId { get; set; }

    public Sube Sube { get; set; } = null!;
    public Kasa? VarsayilanKasa { get; set; }
    public ICollection<KullaniciOturum> KullaniciOturumlar { get; set; } = new List<KullaniciOturum>();
    public ICollection<Vardiya> Vardiyalar { get; set; } = new List<Vardiya>();
    public ICollection<KasaHareket> KasaHareketleri { get; set; } = new List<KasaHareket>();
}
