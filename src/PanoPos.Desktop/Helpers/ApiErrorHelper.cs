using System.Net;
using System.Text.Json;
using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Helpers;

internal static class ApiErrorHelper
{
    public static string BuildMessage(HttpStatusCode statusCode, string? responseBody)
    {
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                var problem = JsonSerializer.Deserialize<ApiProblemDetails>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                {
                    return problem.Detail!;
                }

                if (!string.IsNullOrWhiteSpace(problem?.Title))
                {
                    return problem.Title!;
                }
            }
            catch (JsonException)
            {
            }
        }

        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Gonderilen bilgi gecersiz.",
            HttpStatusCode.Unauthorized => "Giris bilgileri dogrulanamadi.",
            HttpStatusCode.Forbidden => "Bu islem icin yetkiniz yok.",
            HttpStatusCode.NotFound => "Istenen servis bulunamadi.",
            HttpStatusCode.RequestTimeout => "Sunucu zamaninda yanit vermedi.",
            HttpStatusCode.InternalServerError => "Sunucuda beklenmeyen bir hata olustu.",
            HttpStatusCode.ServiceUnavailable => "Servis su anda kullanilamiyor.",
            _ => "Islem sirasinda bir hata olustu."
        };
    }
}
