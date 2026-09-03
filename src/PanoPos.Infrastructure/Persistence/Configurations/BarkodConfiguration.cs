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
        builder.ToTable(x => x.HasCheckConstraint("CK_Barkod_Hedef", "([UrunId] IS NOT NULL AND [UrunVaryantId] IS NULL) OR ([UrunId] IS NULL AND [UrunVaryantId] IS NOT NULL)"));

        builder.HasOne(x => x.Urun)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UrunVaryant)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.UrunVaryantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UrunSatisBirimi)
            .WithMany(x => x.Barkodlar)
            .HasForeignKey(x => x.UrunSatisBirimiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
