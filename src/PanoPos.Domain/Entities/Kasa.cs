using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Kasa : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }

    public ICollection<Cihaz> Cihazlar { get; set; } = new List<Cihaz>();
    public ICollection<Vardiya> Vardiyalar { get; set; } = new List<Vardiya>();
    public ICollection<KasaHareket> KasaHareketleri { get; set; } = new List<KasaHareket>();
}
