using PanoPos.Domain.Enums;

namespace PanoPos.Application.Restaurant;

public sealed class AdisyonDto
{
    public long Id { get; set; }
    public long MasaId { get; set; }
    public string MasaAd { get; set; } = string.Empty;
    public long AcanKullaniciId { get; set; }
    public long AcanCihazId { get; set; }
    public int? KisiSayisi { get; set; }
    public DateTime AcilisTarihi { get; set; }
    public DateTime? KapanisTarihi { get; set; }
    public AdisyonDurumu Durum { get; set; }
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; }
}
