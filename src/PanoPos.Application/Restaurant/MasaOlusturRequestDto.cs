namespace PanoPos.Application.Restaurant;

public sealed class MasaOlusturRequestDto
{
    public long SubeId { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
}
