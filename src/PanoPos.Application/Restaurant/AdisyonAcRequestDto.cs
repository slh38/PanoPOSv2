namespace PanoPos.Application.Restaurant;

public sealed class AdisyonAcRequestDto
{
    public long MasaId { get; set; }
    public long AcanKullaniciId { get; set; }
    public long AcanCihazId { get; set; }
    public int? KisiSayisi { get; set; }
    public string? Aciklama { get; set; }
}
