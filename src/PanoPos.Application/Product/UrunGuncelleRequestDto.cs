using System.ComponentModel.DataAnnotations;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public sealed class StokKartGuncelleRequestDto
{
    public string? StokKartKodu { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public StokKartTipi StokKartTipi { get; set; }
    public long? StokKategoriId { get; set; }
    public long? StokGrupId { get; set; }
    public bool AktifMi { get; set; }
}
