using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class Adisyon : BaseEntity
{
    public long MasaId { get; set; }
    public long AcanKullaniciId { get; set; }
    public long AcanCihazId { get; set; }
    public int? KisiSayisi { get; set; }
    public DateTime AcilisTarihi { get; set; }
    public DateTime? KapanisTarihi { get; set; }
    public AdisyonDurumu Durum { get; set; }
    public string? Aciklama { get; set; }

    public Masa? Masa { get; set; }
}
