using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokKartVaryantConfiguration : IEntityTypeConfiguration<StokKartVaryant>
{
    public void Configure(EntityTypeBuilder<StokKartVaryant> builder)
    {
        builder.ToTable("StokKartVaryant");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.VaryantKodu).HasMaxLength(80).IsRequired();
        builder.Property(x => x.BarkodluMu).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.StokKartId, x.RenkId, x.BedenId }).IsUnique();
        builder.ToTable(x => x.HasCheckConstraint("CK_StokKartVaryant_RenkVeyaBeden", "[RenkId] IS NOT NULL OR [BedenId] IS NOT NULL"));

        builder.HasOne(x => x.StokKart)
            .WithMany(x => x.Varyantlar)
            .HasForeignKey(x => x.StokKartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Renk)
            .WithMany(x => x.StokKartVaryantlari)
            .HasForeignKey(x => x.RenkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Beden)
            .WithMany(x => x.StokKartVaryantlari)
            .HasForeignKey(x => x.BedenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
