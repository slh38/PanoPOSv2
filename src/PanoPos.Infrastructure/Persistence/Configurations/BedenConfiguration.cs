using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class BedenConfiguration : IEntityTypeConfiguration<Beden>
{
    public void Configure(EntityTypeBuilder<Beden> builder)
    {
        builder.ToTable("Beden");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Ad });
        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique();
    }
}
