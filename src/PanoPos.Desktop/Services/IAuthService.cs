using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Services;

public interface IAuthService
{
    Task<LoginResponseModel> LoginAsync(string pin, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
