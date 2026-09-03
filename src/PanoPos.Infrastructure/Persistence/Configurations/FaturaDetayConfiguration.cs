using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class FaturaDetayConfiguration : IEntityTypeConfiguration<FaturaDetay>
{
    public void Configure(EntityTypeBuilder<FaturaDetay> builder)
    {
        builder.ToTable("FaturaDetay");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.BirimAdi).HasMaxLength(100);
        builder.Property(x => x.BirimKatsayi).HasColumnType("decimal(18,3)");
        builder.Property(x => x.Miktar).HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.BirimFiyat).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.FiyatParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.Property(x => x.FiyatKur).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.SatirAraToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IndirimOrani).HasColumnType("decimal(5,2)");
        builder.Property(x => x.IndirimTutari).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.SatirNetToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.SatirToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasOne(x => x.Fatura)
            .WithMany(x => x.Detaylar)
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Urun)
            .WithMany()
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UrunVaryant)
            .WithMany()
            .HasForeignKey(x => x.UrunVaryantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FaturaId);

        builder.HasOne(x => x.UrunSatisBirimi)
            .WithMany()
            .HasForeignKey(x => x.UrunSatisBirimiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
