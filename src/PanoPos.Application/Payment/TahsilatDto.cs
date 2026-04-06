using PanoPos.Domain.Enums;

namespace PanoPos.Application.Payment;

public sealed class TahsilatDto
{
    public long Id { get; set; }
    public long FaturaId { get; set; }
    public string TahsilatFisNo { get; set; } = string.Empty;
    public OdemeTipi OdemeTipi { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal Tutar { get; set; }
    public decimal YerelTutar { get; set; }
    public decimal FaturaOdenenTutar { get; set; }
    public decimal FaturaKalanTutar { get; set; }
    public FaturaDurumu FaturaDurumu { get; set; }
    public string? Aciklama { get; set; }
    public DateTime TahsilatTarihi { get; set; }
    public bool AktifMi { get; set; }
}
