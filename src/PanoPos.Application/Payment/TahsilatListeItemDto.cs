using PanoPos.Domain.Enums;

namespace PanoPos.Application.Payment;

public sealed class TahsilatListeItemDto
{
    public long Id { get; set; }
    public string TahsilatFisNo { get; set; } = string.Empty;
    public long FaturaId { get; set; }
    public OdemeTipi OdemeTipi { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal Tutar { get; set; }
    public decimal YerelTutar { get; set; }
    public DateTime TahsilatTarihi { get; set; }
}
