using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class Banka : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
}
