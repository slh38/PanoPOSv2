using PanoPos.Domain.Enums;

namespace PanoPos.Application.Invoice;

public sealed class FaturaListeItemDto
{
    public long Id { get; set; }
    public string FaturaNo { get; set; } = string.Empty;
    public long? SiparisId { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public decimal ToplamTutar { get; set; }
    public FaturaDurumu Durum { get; set; }
    public DateTime? KapanisTarihi { get; set; }
}
