namespace PanoPos.Application.Auth;

public interface IAuthServisi
{
    Task<LoginResponseDto> LoginAsync(string pin, long cihazId, CancellationToken cancellationToken = default);
    Task LogoutAsync(long kullaniciOturumId, CancellationToken cancellationToken = default);
}
