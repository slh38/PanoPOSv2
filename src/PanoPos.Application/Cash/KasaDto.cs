namespace PanoPos.Application.Cash;

public sealed class KasaDto
{
    public long Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; }
}
