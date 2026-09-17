using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokKartFiyatConfiguration : IEntityTypeConfiguration<StokKartFiyat>
{
    public void Configure(EntityTypeBuilder<StokKartFiyat> builder)
    {
        builder.ToTable("StokKartFiyat");
        PanoPosDbContext.ConfigureBaseEntity(builder);
        builder.Property(x => x.Fiyat).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.HasIndex(x => new { x.StokKartSatisBirimiId, x.FiyatTipiId }).IsUnique();
        builder.HasOne(x => x.StokKartSatisBirimi).WithMany(x => x.Fiyatlar).HasForeignKey(x => x.StokKartSatisBirimiId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FiyatTipi).WithMany(x => x.StokKartFiyatlari).HasForeignKey(x => x.FiyatTipiId).OnDelete(DeleteBehavior.Restrict);
    }
}

