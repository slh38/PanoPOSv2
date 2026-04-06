using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class CariHareket : BaseEntity
{
    public long CariId { get; set; }
    public long? FaturaId { get; set; }
    public long? TahsilatId { get; set; }
    public CariHareketTipi HareketTipi { get; set; }
    public decimal Tutar { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal YerelTutar { get; set; }
    public DateTime HareketTarihi { get; set; }
    public string? Aciklama { get; set; }

    public Cari Cari { get; set; } = null!;
    public Fatura? Fatura { get; set; }
    public Tahsilat? Tahsilat { get; set; }
}
