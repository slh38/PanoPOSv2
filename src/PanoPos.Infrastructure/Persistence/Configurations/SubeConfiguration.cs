using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class SubeConfiguration : IEntityTypeConfiguration<Sube>
{
    public void Configure(EntityTypeBuilder<Sube> builder)
    {
        builder.ToTable("Sube");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Subeler)
            .HasForeignKey(x => x.TenantId)
            .HasPrincipalKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
