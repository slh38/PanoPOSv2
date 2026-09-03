using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class UrunSatisBirimi : BaseEntity
{
    public long UrunId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public bool VarsayilanMi { get; set; }

    public Urun Urun { get; set; } = null!;
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
    public ICollection<UrunFiyat> Fiyatlar { get; set; } = new List<UrunFiyat>();
}

