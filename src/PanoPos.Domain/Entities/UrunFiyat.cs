using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class UrunFiyat : BaseEntity
{
    public long UrunSatisBirimiId { get; set; }
    public long FiyatTipiId { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";

    public UrunSatisBirimi UrunSatisBirimi { get; set; } = null!;
    public FiyatTipi FiyatTipi { get; set; } = null!;
}

