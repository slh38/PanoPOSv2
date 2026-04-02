using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KasaConfiguration : IEntityTypeConfiguration<Kasa>
{
    public void Configure(EntityTypeBuilder<Kasa> builder)
    {
        builder.ToTable("Kasa");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.Ad }).IsUnique();
    }
}
