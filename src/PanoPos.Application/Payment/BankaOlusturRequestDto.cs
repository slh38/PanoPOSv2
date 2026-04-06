namespace PanoPos.Application.Payment;

public sealed class BankaOlusturRequestDto
{
    public long SubeId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
