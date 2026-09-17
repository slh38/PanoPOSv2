using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class StokKartSatisBirimi : BaseEntity
{
    public long StokKartId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public bool VarsayilanMi { get; set; }

    public StokKart StokKart { get; set; } = null!;
    public ICollection<Barkod> Barkodlar { get; set; } = new List<Barkod>();
    public ICollection<StokKartFiyat> Fiyatlar { get; set; } = new List<StokKartFiyat>();
}

