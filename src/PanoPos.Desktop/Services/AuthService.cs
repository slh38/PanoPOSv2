using PanoPos.Desktop.Config;
using PanoPos.Desktop.Models;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly AppSettings _settings;
    private readonly AppSession _session;

    public AuthService(IApiClient apiClient, AppSettings settings, AppSession session)
    {
        _apiClient = apiClient;
        _settings = settings;
        _session = session;
    }

    public async Task<LoginResponseModel> LoginAsync(string pin, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequestModel
        {
            Pin = pin.Trim(),
            CihazId = _settings.CihazId
        };

        var response = await _apiClient.PostAsync<LoginRequestModel, LoginResponseModel>("/api/v1/auth/login", request, cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("Giris yaniti alinamadi.");
        }

        _session.Fill(response);
        return response;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsAuthenticated)
        {
            _session.Clear();
            return;
        }

        var request = new LogoutRequestModel
        {
            KullaniciOturumId = _session.OturumId
        };

        await _apiClient.PostAsync<LogoutRequestModel, object>("/api/v1/auth/logout", request, cancellationToken);
        _session.Clear();
    }
}
