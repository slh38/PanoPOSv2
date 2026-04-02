namespace PanoPos.Application.Auth;

public interface IAuthIslemLogServisi
{
    Task LoginBasariliAsync(long kullaniciId, long cihazId, long oturumId, CancellationToken cancellationToken = default);
    Task LoginBasarisizAsync(long? kullaniciId, long cihazId, string neden, CancellationToken cancellationToken = default);
    Task LogoutAsync(long kullaniciId, long kullaniciOturumId, CancellationToken cancellationToken = default);
}
