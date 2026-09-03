using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class StokKategori : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string? Kod { get; set; }
}
