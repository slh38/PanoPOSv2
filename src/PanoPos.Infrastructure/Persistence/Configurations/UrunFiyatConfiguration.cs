using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class UrunFiyatConfiguration : IEntityTypeConfiguration<UrunFiyat>
{
    public void Configure(EntityTypeBuilder<UrunFiyat> builder)
    {
        builder.ToTable("UrunFiyat");
        PanoPosDbContext.ConfigureBaseEntity(builder);
        builder.Property(x => x.Fiyat).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.HasIndex(x => new { x.UrunSatisBirimiId, x.FiyatTipiId }).IsUnique();
        builder.HasOne(x => x.UrunSatisBirimi).WithMany(x => x.Fiyatlar).HasForeignKey(x => x.UrunSatisBirimiId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FiyatTipi).WithMany(x => x.UrunFiyatlari).HasForeignKey(x => x.FiyatTipiId).OnDelete(DeleteBehavior.Restrict);
    }
}

