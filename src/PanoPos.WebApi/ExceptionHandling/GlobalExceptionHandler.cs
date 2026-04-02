using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Common;

namespace PanoPos.WebApi.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var hata = exception as UygulamaHatasi;
        var statusCode = hata?.StatusCode ?? StatusCodes.Status500InternalServerError;

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = hata?.Title ?? "Beklenmeyen hata",
            Detail = hata?.Detail ?? "Beklenmeyen bir hata olustu.",
            Type = hata?.ErrorCode is null ? null : $"https://panopos/errors/{hata.ErrorCode}",
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
