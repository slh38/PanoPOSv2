using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class BarkodConfiguration : IEntityTypeConfiguration<Barkod>
{
    public void Configure(EntityTypeBuilder<Barkod> builder)
    {
        builder.ToTable("Barkod");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.BarkodNo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BarkodTipi).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.BarkodNo }).IsUnique();
        builder.ToTable(x => x.HasCheckConstraint("CK_Barkod_Hedef", "([StokKartId] IS NOT NULL AND [StokKartVaryantId] IS NULL) OR ([StokKartId] IS NULL AND [StokKartVaryantId] IS NOT NULL)"));

        builder.HasOne(x => x.StokKart)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.StokKartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StokKartVaryant)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.StokKartVaryantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StokKartSatisBirimi)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.StokKartSatisBirimiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
