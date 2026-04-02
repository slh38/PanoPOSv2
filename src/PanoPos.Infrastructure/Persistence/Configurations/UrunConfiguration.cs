using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class UrunConfiguration : IEntityTypeConfiguration<Urun>
{
    public void Configure(EntityTypeBuilder<Urun> builder)
    {
        builder.ToTable("Urun");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.UrunKodu).HasMaxLength(50);
        builder.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.UrunTipi).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.SilindiMi });
        builder.HasIndex(x => new { x.TenantId, x.UrunKodu }).IsUnique().HasFilter("[UrunKodu] IS NOT NULL");
    }
}
