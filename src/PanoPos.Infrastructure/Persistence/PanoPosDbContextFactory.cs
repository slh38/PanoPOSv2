using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PanoPos.Infrastructure.Persistence;

public sealed class PanoPosDbContextFactory : IDesignTimeDbContextFactory<PanoPosDbContext>
{
    public PanoPosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PanoPosDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=SLHASUS\\SQLEXPRESS;Database=PanoPosDb;User Id=sa;Password=admin-*741852963;TrustServerCertificate=True;");

        return new PanoPosDbContext(optionsBuilder.Options);
    }
}
