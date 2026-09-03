using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokKartSatisBirimiConfiguration : IEntityTypeConfiguration<StokKartSatisBirimi>
{
    public void Configure(EntityTypeBuilder<StokKartSatisBirimi> builder)
    {
        builder.ToTable("StokKartSatisBirimi");
        PanoPosDbContext.ConfigureBaseEntity(builder);
        builder.Property(x => x.BirimKodu).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BirimAdi).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Katsayi).HasColumnType("decimal(18,3)").IsRequired();
        builder.HasIndex(x => new { x.StokKartId, x.BirimKodu }).IsUnique();
        builder.HasOne(x => x.StokKart).WithMany(x => x.SatisBirimleri).HasForeignKey(x => x.StokKartId).OnDelete(DeleteBehavior.Restrict);
    }
}

