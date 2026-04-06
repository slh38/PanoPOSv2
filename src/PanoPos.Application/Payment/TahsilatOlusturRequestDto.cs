using PanoPos.Domain.Enums;

namespace PanoPos.Application.Payment;

public sealed class TahsilatOlusturRequestDto
{
    public long SubeId { get; set; }
    public long FaturaId { get; set; }
    public OdemeTipi OdemeTipi { get; set; }
    public long? KasaId { get; set; }
    public long? BankaId { get; set; }
    public long KullaniciId { get; set; }
    public long CihazId { get; set; }
    public decimal Tutar { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public string? Aciklama { get; set; }
    public DateTime? TahsilatTarihi { get; set; }
}
