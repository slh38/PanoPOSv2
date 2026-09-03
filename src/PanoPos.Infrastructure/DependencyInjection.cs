using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PanoPos.Application.Audit;
using PanoPos.Application.Auth;
using PanoPos.Application.Cash;
using PanoPos.Application.Customer;
using PanoPos.Application.Invoice;
using PanoPos.Application.Order;
using PanoPos.Application.Outbox;
using PanoPos.Application.Payment;
using PanoPos.Application.Product;
using PanoPos.Application.Restaurant;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Cash;
using PanoPos.Infrastructure.Customer;
using PanoPos.Infrastructure.Invoice;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Payment;
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
        services.AddScoped<IIslemLogServisi, IslemLogServisi>();
        services.AddScoped<IOutboxServisi, OutboxServisi>();
        services.AddScoped<IAuthServisi, AuthServisi>();
        services.AddScoped<IAuthIslemLogServisi, AuthIslemLogServisi>();
        services.AddScoped<IKasaServisi, KasaServisi>();
        services.AddScoped<IVardiyaServisi, VardiyaServisi>();
        services.AddScoped<ICariServisi, CariServisi>();
        services.AddScoped<IMasaServisi, MasaServisi>();
        services.AddScoped<IMasaGrupServisi, MasaGrupServisi>();
        services.AddScoped<IAdisyonServisi, AdisyonServisi>();
        services.AddScoped<IFaturaServisi, FaturaServisi>();
        services.AddScoped<ISiparisServisi, SiparisServisi>();
        services.AddScoped<ITahsilatServisi, TahsilatServisi>();
        services.AddScoped<IBankaServisi, BankaServisi>();
        services.AddScoped<IStokKartServisi, StokKartServisi>();
        services.AddScoped<IBarkodServisi, BarkodServisi>();
        services.AddScoped<IRenkServisi, RenkServisi>();
        services.AddScoped<IBedenServisi, BedenServisi>();
        services.AddScoped<IStokKategoriServisi, StokKategoriServisi>();
        services.AddScoped<IStokGrupServisi, StokGrupServisi>();

        return services;
    }
}
