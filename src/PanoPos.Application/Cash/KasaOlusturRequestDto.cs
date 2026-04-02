using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Cash;

public sealed class KasaOlusturRequestDto
{
    [Required]
    public string Ad { get; set; } = string.Empty;

    public string? Aciklama { get; set; }
    [Range(1, long.MaxValue)]
    public long SubeId { get; set; }
}
