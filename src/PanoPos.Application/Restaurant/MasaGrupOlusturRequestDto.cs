namespace PanoPos.Application.Restaurant;

public sealed class MasaGrupOlusturRequestDto
{
    public long SubeId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Kod { get; set; }
}
