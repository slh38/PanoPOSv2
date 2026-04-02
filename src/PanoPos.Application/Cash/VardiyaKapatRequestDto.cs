using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Cash;

public sealed class VardiyaKapatRequestDto
{
    [Range(1, long.MaxValue)]
    public long VardiyaId { get; set; }
    [Range(0, double.MaxValue)]
    public decimal SayilanNakit { get; set; }
    public string? Aciklama { get; set; }
}
