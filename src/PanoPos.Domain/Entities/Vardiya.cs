using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Vardiya : BaseEntity
{
    public long KasaId { get; set; }
    public long CihazId { get; set; }
    public long KullaniciId { get; set; }
    public DateTime AcilisTarihi { get; set; }
    public DateTime? KapanisTarihi { get; set; }
    public decimal AcilisNakit { get; set; }

    public Kasa Kasa { get; set; } = null!;
    public Cihaz Cihaz { get; set; } = null!;
    public Kullanici Kullanici { get; set; } = null!;
    public VardiyaKapanis? VardiyaKapanis { get; set; }
    public ICollection<KasaHareket> KasaHareketleri { get; set; } = new List<KasaHareket>();
}
