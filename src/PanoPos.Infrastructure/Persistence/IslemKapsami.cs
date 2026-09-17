using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Auth;

namespace PanoPos.Infrastructure.Persistence;

// Explicit query scopes; these are not EF model/global filters.
public static class IslemKapsami
{
    public static IIslemBaglami? Baglam(this PanoPosDbContext db)
    {
        var context = db.IslemBaglami;
        // Design-time and standalone regression fixtures have no request scope.
        if (context != null && !context.Dogrulandi) throw IslemBaglami.Yetkisiz();
        return context;
    }

    public static IQueryable<T> TenantKapsami<T>(this IQueryable<T> query, PanoPosDbContext db) where T : class
    {
        var context = db.Baglam();
        if (context == null) return query;
        var tenant = context.TenantId;
        return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenant);
    }

    public static IQueryable<T> SubeKapsami<T>(this IQueryable<T> query, PanoPosDbContext db) where T : class
    {
        var context = db.Baglam();
        if (context == null) return query;
        var tenant = context.TenantId;
        var branch = context.SubeId;
        return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenant && EF.Property<long>(x, "SubeId") == branch);
    }

    public static IQueryable<Sube> YetkiliSube(this IQueryable<Sube> query, PanoPosDbContext db)
    {
        var context = db.Baglam();
        if (context == null) return query;
        var tenant = context.TenantId;
        var branch = context.SubeId;
        return query.Where(x => x.TenantId == tenant && x.Id == branch);
    }

    public static long Kimlik(long supplied, long trusted)
    {
        if (supplied != 0 && supplied != trusted)
            throw new UygulamaHatasi(403, "Yetkisiz baglam", "Istek aktif oturum baglamiyla uyusmuyor.", "context_mismatch");
        return trusted;
    }
}
