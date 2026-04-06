using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class SiparisDetayConfiguration : IEntityTypeConfiguration<SiparisDetay>
{
    public void Configure(EntityTypeBuilder<SiparisDetay> builder)
    {
        builder.ToTable("SiparisDetay");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Miktar).HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.BirimFiyat).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.SatirToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasOne(x => x.Siparis)
            .WithMany(x => x.Detaylar)
            .HasForeignKey(x => x.SiparisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Urun)
            .WithMany()
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UrunVaryant)
            .WithMany()
            .HasForeignKey(x => x.UrunVaryantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SiparisId);
    }
}
