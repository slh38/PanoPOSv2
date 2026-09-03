using System.ComponentModel.DataAnnotations;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Product;

public class StokKartOlusturRequestDto
{
    [Range(1, long.MaxValue)]
    public long SubeId { get; set; }
    public string? StokKartKodu { get; set; }
    [Required]
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public StokKartTipi StokKartTipi { get; set; }
    public long? StokKategoriId { get; set; }
    public long? StokGrupId { get; set; }
}

