using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Masa : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public long MasaDurumId { get; set; }

    public MasaDurum? MasaDurum { get; set; }
    public ICollection<Adisyon> Adisyonlar { get; set; } = new List<Adisyon>();
}
