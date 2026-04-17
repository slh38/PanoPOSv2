using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class UrunKategoriConfiguration : IEntityTypeConfiguration<UrunKategori>
{
    public void Configure(EntityTypeBuilder<UrunKategori> builder)
    {
        builder.ToTable("UrunKategori");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.SilindiMi });
        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique().HasFilter("[Kod] IS NOT NULL");
    }
}
