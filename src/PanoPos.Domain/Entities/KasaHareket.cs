using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class KasaHareket : BaseEntity
{
    public long KasaId { get; set; }
    public long? VardiyaId { get; set; }
    public long KullaniciId { get; set; }
    public long CihazId { get; set; }
    public KasaIslemTipi IslemTipi { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public string? ReferansTip { get; set; }
    public long? ReferansId { get; set; }
    public DateTime Tarih { get; set; }

    public Kasa Kasa { get; set; } = null!;
    public Vardiya? Vardiya { get; set; }
    public Kullanici Kullanici { get; set; } = null!;
    public Cihaz Cihaz { get; set; } = null!;
}
