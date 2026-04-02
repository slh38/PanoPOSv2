using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class RenkConfiguration : IEntityTypeConfiguration<Renk>
{
    public void Configure(EntityTypeBuilder<Renk> builder)
    {
        builder.ToTable("Renk");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Ad });
        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique();
    }
}
