using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PanoPos.Infrastructure.Persistence;

public sealed class PanoPosDbContextFactory : IDesignTimeDbContextFactory<PanoPosDbContext>
{
    public PanoPosDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("PanoPos")
            ?? throw new InvalidOperationException("Connection string 'PanoPos' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<PanoPosDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PanoPosDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var webApiPath = Path.Combine(currentDirectory, "src", "PanoPos.WebApi");

        if (!Directory.Exists(webApiPath))
        {
            webApiPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "PanoPos.WebApi"));
        }

        return new ConfigurationBuilder()
            .SetBasePath(webApiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
