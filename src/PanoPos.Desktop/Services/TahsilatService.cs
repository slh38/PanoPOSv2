using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Services;

public sealed class TahsilatService : ITahsilatService
{
    private readonly IApiClient _apiClient;

    public TahsilatService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<TahsilatResponseModel?> TahsilatYapAsync(TahsilatRequestModel request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<TahsilatRequestModel, TahsilatResponseModel>("/api/v1/tahsilat", request, cancellationToken);
    }
}
