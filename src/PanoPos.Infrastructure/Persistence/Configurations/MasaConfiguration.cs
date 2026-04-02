using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class MasaConfiguration : IEntityTypeConfiguration<Masa>
{
    public void Configure(EntityTypeBuilder<Masa> builder)
    {
        builder.ToTable("Masa");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Ad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.MasaDurumId).IsRequired();

        builder.HasOne(x => x.MasaDurum)
            .WithMany(x => x.Masalar)
            .HasForeignKey(x => x.MasaDurumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.SilindiMi });
    }
}
