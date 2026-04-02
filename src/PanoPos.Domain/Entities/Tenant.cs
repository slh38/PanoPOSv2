using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;

    public ICollection<Sube> Subeler { get; set; } = new List<Sube>();
}
