using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PanoPos.Application.Auth;
using PanoPos.Application.Cash;
using PanoPos.Application.Customer;
using PanoPos.Application.Product;
using PanoPos.Application.Restaurant;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Cash;
using PanoPos.Infrastructure.Customer;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Product;
using PanoPos.Infrastructure.Restaurant;

namespace PanoPos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PanoPos")
            ?? throw new InvalidOperationException("Connection string 'PanoPos' was not found.");

        services.AddDbContext<PanoPosDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IPinHashServisi, PinHashServisi>();
        services.AddScoped<IAuthServisi, AuthServisi>();
        services.AddScoped<IAuthIslemLogServisi, BosAuthIslemLogServisi>();
        services.AddScoped<IKasaServisi, KasaServisi>();
        services.AddScoped<IVardiyaServisi, VardiyaServisi>();
        services.AddScoped<ICariServisi, CariServisi>();
        services.AddScoped<IMasaServisi, MasaServisi>();
        services.AddScoped<IAdisyonServisi, AdisyonServisi>();
        services.AddScoped<IUrunServisi, UrunServisi>();
        services.AddScoped<IBarkodServisi, BarkodServisi>();
        services.AddScoped<IRenkServisi, RenkServisi>();
        services.AddScoped<IBedenServisi, BedenServisi>();

        return services;
    }
}
