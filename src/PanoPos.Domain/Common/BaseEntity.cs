namespace PanoPos.Domain.Common;

public abstract class BaseEntity : IEntity, IAuditableEntity, ISoftDeletableEntity
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool SilindiMi { get; set; }
    public long? OlusturanKullaniciId { get; set; }
    public long? GuncelleyenKullaniciId { get; set; }
    public long? SilenKullaniciId { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    public void SoftDelete(long? silenKullaniciId, DateTime silinmeTarihi)
    {
        if (SilindiMi)
        {
            return;
        }

        AktifMi = false;
        SilindiMi = true;
        SilenKullaniciId = silenKullaniciId;
        SilinmeTarihi = silinmeTarihi;
        GuncelleyenKullaniciId = silenKullaniciId;
        GuncellemeTarihi = silinmeTarihi;
    }
}
