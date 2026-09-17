using PanoPos.Domain.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

public sealed class StokFis : BaseEntity
{
    public StokFisTipi StokFisTipi { get; set; }
    public string FisNo { get; set; } = string.Empty;
    public DateTime FisTarihi { get; set; }
    public long? DepoId { get; set; }
    public long? KaynakDepoId { get; set; }
    public long? HedefDepoId { get; set; }
    public string? Aciklama { get; set; }
    public ICollection<StokFisDetay> Detaylar { get; set; } = new List<StokFisDetay>();
}
