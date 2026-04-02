using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class CihazConfiguration : IEntityTypeConfiguration<Cihaz>
{
    public void Configure(EntityTypeBuilder<Cihaz> builder)
    {
        builder.ToTable("Cihaz");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique();

        builder.HasOne(x => x.Sube)
            .WithMany(x => x.Cihazlar)
            .HasForeignKey(x => x.SubeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VarsayilanKasa)
            .WithMany(x => x.Cihazlar)
            .HasForeignKey(x => x.VarsayilanKasaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
