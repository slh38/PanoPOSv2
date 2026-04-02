using PanoPos.Application.Auth;

namespace PanoPos.Infrastructure.Auth;

public sealed class BosAuthIslemLogServisi : IAuthIslemLogServisi
{
    public Task LoginBasariliAsync(long kullaniciId, long cihazId, long oturumId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LoginBasarisizAsync(long? kullaniciId, long cihazId, string neden, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LogoutAsync(long kullaniciId, long kullaniciOturumId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
