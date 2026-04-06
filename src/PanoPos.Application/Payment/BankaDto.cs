namespace PanoPos.Application.Payment;

public sealed class BankaDto
{
    public long Id { get; set; }
    public long SubeId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
}
