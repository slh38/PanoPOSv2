using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class UrunVaryantConfiguration : IEntityTypeConfiguration<UrunVaryant>
{
    public void Configure(EntityTypeBuilder<UrunVaryant> builder)
    {
        builder.ToTable("UrunVaryant");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.VaryantKodu).HasMaxLength(80).IsRequired();
        builder.Property(x => x.BarkodluMu).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.UrunId, x.RenkId, x.BedenId }).IsUnique();
        builder.ToTable(x => x.HasCheckConstraint("CK_UrunVaryant_RenkVeyaBeden", "[RenkId] IS NOT NULL OR [BedenId] IS NOT NULL"));

        builder.HasOne(x => x.Urun)
            .WithMany(x => x.Varyantlar)
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Renk)
            .WithMany(x => x.UrunVaryantlari)
            .HasForeignKey(x => x.RenkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Beden)
            .WithMany(x => x.UrunVaryantlari)
            .HasForeignKey(x => x.BedenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
