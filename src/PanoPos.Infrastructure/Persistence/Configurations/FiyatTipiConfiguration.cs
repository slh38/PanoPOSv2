using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class FiyatTipiConfiguration : IEntityTypeConfiguration<FiyatTipi>
{
    public void Configure(EntityTypeBuilder<FiyatTipi> builder)
    {
        builder.ToTable("FiyatTipi");
        PanoPosDbContext.ConfigureBaseEntity(builder);
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Ad).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique();
    }
}

