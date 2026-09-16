using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class DepoConfiguration : IEntityTypeConfiguration<Depo>
{
    public void Configure(EntityTypeBuilder<Depo> builder)
    {
        builder.ToTable("Depo");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.DepoKodu).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Ad).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.DepoKodu })
            .IsUnique()
            .HasFilter("[AktifMi] = 1 AND [SilindiMi] = 0");
        builder.HasIndex(x => new { x.TenantId, x.SubeId })
            .IsUnique()
            .HasFilter("[VarsayilanMi] = 1 AND [AktifMi] = 1 AND [SilindiMi] = 0");
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasPrincipalKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);        builder.HasOne(x => x.Sube)
            .WithMany()
            .HasForeignKey(x => x.SubeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

