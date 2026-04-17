namespace PanoPos.Application.Restaurant;

public sealed class MasaOlusturRequestDto
{
    public long SubeId { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public long? MasaGrupId { get; set; }
    public int? Kapasite { get; set; }
}
