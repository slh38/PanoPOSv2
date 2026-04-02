using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Cash;

public sealed class VardiyaAcRequestDto
{
    [Range(1, long.MaxValue)]
    public long KullaniciId { get; set; }
    [Range(1, long.MaxValue)]
    public long CihazId { get; set; }
    [Range(1, long.MaxValue)]
    public long KasaId { get; set; }
    [Range(0, double.MaxValue)]
    public decimal AcilisNakit { get; set; }
}
