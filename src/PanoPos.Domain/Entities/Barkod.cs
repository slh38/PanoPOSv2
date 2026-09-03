using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Barkod : BaseEntity
{
    public string BarkodNo { get; set; } = string.Empty;
    public BarkodTipi BarkodTipi { get; set; }
    public long? StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long? StokKartSatisBirimiId { get; set; }

    public StokKart? StokKart { get; set; }
    public StokKartVaryant? StokKartVaryant { get; set; }
    public StokKartSatisBirimi? StokKartSatisBirimi { get; set; }
}
