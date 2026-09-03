using PanoPos.Domain.Enums;

namespace PanoPos.Application.Customer;

public sealed class CariOlusturRequestDto
{
    public long SubeId { get; set; }
    public string? CariKodu { get; set; }
    public string Ad { get; set; } = string.Empty;
    public CariTipi Tip { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? VergiNo { get; set; }
    public bool AktifMi { get; set; } = true;
}
