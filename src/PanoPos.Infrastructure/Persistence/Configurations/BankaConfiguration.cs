using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class BankaConfiguration : IEntityTypeConfiguration<Banka>
{
    public void Configure(EntityTypeBuilder<Banka> builder)
    {
        builder.ToTable("Banka");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.Kod }).IsUnique();
    }
}
