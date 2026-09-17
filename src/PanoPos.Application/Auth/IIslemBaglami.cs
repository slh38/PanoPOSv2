namespace PanoPos.Application.Auth;

public interface IIslemBaglami
{
    bool Dogrulandi { get; }
    Guid TenantId { get; }
    long SubeId { get; }
    long KullaniciId { get; }
    long CihazId { get; }
    long KullaniciOturumId { get; }
}
