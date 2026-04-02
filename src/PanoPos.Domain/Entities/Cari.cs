using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Cari : BaseEntity
{
    public string? CariKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public CariTipi Tip { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? VergiNo { get; set; }
}
