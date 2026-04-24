using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PanoPos.Desktop.Config;
using PanoPos.Desktop.Helpers;

namespace PanoPos.Desktop.Services;

public sealed class ApiClient : IApiClient, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ApiClient(AppSettings settings)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.BaseApiUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        return (await ReadResponseAsync<T>(response, cancellationToken))!;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, request, JsonOptions, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Servis zaman asimina ugradi.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Servise baglanilamadi.", ex);
        }
    }

    private static async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var responseBody = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = ApiErrorHelper.BuildMessage(response.StatusCode, responseBody);
            throw new InvalidOperationException(message);
        }

        if (response.StatusCode == HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        var result = JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        if (result is null)
        {
            throw new InvalidOperationException("Servisten gecersiz veri dondu.");
        }

        return result;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
