using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class KullaniciOturum : BaseEntity
{
    public long KullaniciId { get; set; }
    public long CihazId { get; set; }
    public DateTime GirisTarihi { get; set; }
    public DateTime? CikisTarihi { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Cihaz Cihaz { get; set; } = null!;
}
