using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.WebApi.Authentication;

// Legacy DTO context fields remain compatible, but cannot select another identity.
public sealed class IslemBaglamiFiltresi(IIslemBaglami context) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext action, ActionExecutionDelegate next)
    {
        if (action.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next(); return;
        }
        foreach (var parameter in action.ActionDescriptor.Parameters)
            if (!action.ActionArguments.ContainsKey(parameter.Name) && TryIdentity(parameter.Name, out var missing))
                action.ActionArguments[parameter.Name] = missing;
        foreach (var argument in action.ActionArguments.ToArray())
        {
            if (argument.Value == null) continue;
            if (TryIdentity(argument.Key, out var trusted))
                action.ActionArguments[argument.Key] = Bind(argument.Value, trusted);
            else if (argument.Value.GetType().Namespace?.StartsWith("PanoPos.Application.", StringComparison.Ordinal) == true)
                foreach (var property in argument.Value.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite))
                    if (TryIdentity(property.Name, out trusted))
                        property.SetValue(argument.Value, Bind(property.GetValue(argument.Value), trusted));
        }
        await next();
    }

    private bool TryIdentity(string name, out object trusted)
    {
        trusted = name.ToLowerInvariant() switch
        {
            "tenantid" => context.TenantId,
            "subeid" => context.SubeId,
            "kullaniciid" or "acankullaniciid" or "kapatankullaniciid" => context.KullaniciId,
            "cihazid" or "acancihazid" => context.CihazId,
            "kullanicioturumid" => context.KullaniciOturumId,
            _ => null!
        };
        return trusted != null;
    }

    private static object Bind(object? supplied, object trusted)
    {
        if (trusted is long id) return IslemKapsami.Kimlik(supplied is long value ? value : 0, id);
        if (supplied is Guid tenant && tenant != Guid.Empty && tenant != (Guid)trusted)
            throw new UygulamaHatasi(403, "Yetkisiz baglam", "Tenant oturumla uyusmuyor.", "context_mismatch");
        return trusted;
    }
}
