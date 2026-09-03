using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class UrunSatisBirimiConfiguration : IEntityTypeConfiguration<UrunSatisBirimi>
{
    public void Configure(EntityTypeBuilder<UrunSatisBirimi> builder)
    {
        builder.ToTable("UrunSatisBirimi");
        PanoPosDbContext.ConfigureBaseEntity(builder);
        builder.Property(x => x.BirimKodu).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BirimAdi).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Katsayi).HasColumnType("decimal(18,3)").IsRequired();
        builder.HasIndex(x => new { x.UrunId, x.BirimKodu }).IsUnique();
        builder.HasOne(x => x.Urun).WithMany(x => x.SatisBirimleri).HasForeignKey(x => x.UrunId).OnDelete(DeleteBehavior.Restrict);
    }
}

