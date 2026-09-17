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
        builder.Property(x => x.KdvOrani).HasPrecision(5, 2);
        builder.Property(x => x.Matrah).HasPrecision(18, 2);
        builder.Property(x => x.KdvTutari).HasPrecision(18, 2);
        builder.Property(x => x.GenelIndirimPayi).HasPrecision(18, 2);
        builder.HasOne<Kdv>().WithMany().HasForeignKey(x => x.KdvId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.BirimAdi).HasMaxLength(100);
        builder.Property(x => x.FiyatTipiAdi).HasMaxLength(100);
        builder.HasOne<FiyatTipi>().WithMany().HasForeignKey(x => x.FiyatTipiId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.BirimKodu).HasMaxLength(50);
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

        builder.HasOne(x => x.Siparis)
            .WithMany(x => x.Detaylar)
            .HasForeignKey(x => x.SiparisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StokKart)
            .WithMany()
            .HasForeignKey(x => x.StokKartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StokKartVaryant)
            .WithMany()
            .HasForeignKey(x => x.StokKartVaryantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SiparisId);

        builder.HasOne(x => x.StokKartSatisBirimi)
            .WithMany()
            .HasForeignKey(x => x.StokKartSatisBirimiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
