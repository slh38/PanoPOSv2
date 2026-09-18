using PanoPos.Domain.Enums;

namespace PanoPos.Application.Invoice;

public sealed class FaturaOdemeDto
{
    public long TahsilatId { get; set; }
    public DateTime Tarih { get; set; }
    public OdemeTipi OdemeTipi { get; set; }
    public decimal Tutar { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public long? KasaId { get; set; }
    public string? KasaAdi { get; set; }
    public long? BankaId { get; set; }
    public string? BankaAdi { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
}
