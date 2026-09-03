using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class FiyatTipi : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public ICollection<UrunFiyat> UrunFiyatlari { get; set; } = new List<UrunFiyat>();
}

