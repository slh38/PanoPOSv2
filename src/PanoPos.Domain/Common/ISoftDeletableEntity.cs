namespace PanoPos.Domain.Common;

public interface ISoftDeletableEntity
{
    bool SilindiMi { get; set; }
    long? SilenKullaniciId { get; set; }
    DateTime? SilinmeTarihi { get; set; }
}
