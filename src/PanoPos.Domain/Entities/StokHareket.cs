using PanoPos.Domain.Enums;

namespace PanoPos.Domain.Entities;

// Ledger records deliberately do not inherit the soft-deletable BaseEntity.
public sealed class StokHareket
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long DepoId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokFisId { get; set; }
    public long StokFisDetayId { get; set; }
    public StokHareketTipi StokHareketTipi { get; set; }
    public decimal Miktar { get; set; }
    public DateTime HareketTarihi { get; set; }
    public string? Aciklama { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public long? OlusturanKullaniciId { get; set; }
    public StokFisDetay StokFisDetay { get; set; } = null!;
}
