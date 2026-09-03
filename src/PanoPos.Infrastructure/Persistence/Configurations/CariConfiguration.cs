using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class CariConfiguration : IEntityTypeConfiguration<Cari>
{
    public void Configure(EntityTypeBuilder<Cari> builder)
    {
        builder.ToTable("Cari");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.CariKodu).HasMaxLength(50);
        builder.Property(x => x.Ad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Tip).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Telefon).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.VergiNo).HasMaxLength(20);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.SilindiMi });
        builder.HasIndex(x => new { x.TenantId, x.CariKodu }).IsUnique().HasFilter("[CariKodu] IS NOT NULL");
    }
}
