using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using PanoPos.Infrastructure.Auth;

namespace PanoPos.WebApi.Authentication;

public sealed class OturumMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, OturumDogrulamaServisi service)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() != null &&
            endpoint.Metadata.GetMetadata<IAllowAnonymous>() == null)
        {
            var header = context.Request.Headers.Authorization.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) throw IslemBaglami.Yetkisiz();
            await service.DogrulaAsync(header[7..].Trim(), context.RequestAborted);
        }
        await next(context);
    }
}
