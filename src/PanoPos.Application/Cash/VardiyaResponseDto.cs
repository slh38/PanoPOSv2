namespace PanoPos.Application.Cash;

public sealed class VardiyaResponseDto
{
    public long VardiyaId { get; set; }
    public long KasaId { get; set; }
    public long CihazId { get; set; }
    public long KullaniciId { get; set; }
    public decimal AcilisNakit { get; set; }
    public DateTime AcilisTarihi { get; set; }
    public bool AktifMi { get; set; }
}
