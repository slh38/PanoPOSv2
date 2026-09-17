using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Kdv : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal Oran { get; set; }
}
