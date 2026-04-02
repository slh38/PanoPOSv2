namespace PanoPos.Domain.Common;

public interface IAuditableEntity
{
    Guid TenantId { get; set; }
    long SubeId { get; set; }
    DateTime OlusturmaTarihi { get; set; }
    DateTime? GuncellemeTarihi { get; set; }
    bool AktifMi { get; set; }
    long? OlusturanKullaniciId { get; set; }
    long? GuncelleyenKullaniciId { get; set; }
}
