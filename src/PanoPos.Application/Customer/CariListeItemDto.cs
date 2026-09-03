using PanoPos.Domain.Enums;

namespace PanoPos.Application.Customer;

public sealed class CariListeItemDto
{
    public long Id { get; set; }
    public string? CariKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public CariTipi Tip { get; set; }
    public string? Telefon { get; set; }
    public bool AktifMi { get; set; }
}
