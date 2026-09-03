using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class StokKartFiyat : BaseEntity
{
    public long StokKartSatisBirimiId { get; set; }
    public long FiyatTipiId { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";

    public StokKartSatisBirimi StokKartSatisBirimi { get; set; } = null!;
    public FiyatTipi FiyatTipi { get; set; } = null!;
}

