using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class VardiyaKapanis : BaseEntity
{
    public long VardiyaId { get; set; }
    public decimal BeklenenNakit { get; set; }
    public decimal SayilanNakit { get; set; }
    public decimal FarkTutar { get; set; }
    public decimal KartToplam { get; set; }
    public decimal VeresiyeToplam { get; set; }
    public string? Aciklama { get; set; }

    public Vardiya Vardiya { get; set; } = null!;
}
