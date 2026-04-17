using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class UrunKategori : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string? Kod { get; set; }
}
