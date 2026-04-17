namespace PanoPos.Application.Restaurant;

public sealed class MasaDto
{
    public long Id { get; set; }
    public long SubeId { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public long MasaDurumId { get; set; }
    public string MasaDurumAd { get; set; } = string.Empty;
    public long? MasaGrupId { get; set; }
    public string? MasaGrupAdi { get; set; }
    public int? Kapasite { get; set; }
    public bool AktifMi { get; set; }
}
