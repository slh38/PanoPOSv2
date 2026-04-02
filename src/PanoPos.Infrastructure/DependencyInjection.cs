using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PanoPos.Infrastructure.Persistence;

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

        return services;
    }
}
