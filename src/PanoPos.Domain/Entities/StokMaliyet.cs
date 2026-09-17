namespace PanoPos.Domain.Entities;

// Current base-unit costs, not a document or a soft-deletable master record.
public sealed class StokMaliyet
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long DepoId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public decimal SonAlisMaliyeti { get; set; }
    public decimal AgirlikliOrtalamaMaliyet { get; set; }
    public DateTime? SonAlisTarihi { get; set; }
    public DateTime SonGuncellemeTarihi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}
