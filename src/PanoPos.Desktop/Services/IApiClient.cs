namespace PanoPos.Desktop.Services;

public interface IApiClient
{
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
}
