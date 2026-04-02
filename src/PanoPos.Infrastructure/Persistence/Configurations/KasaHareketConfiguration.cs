using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KasaHareketConfiguration : IEntityTypeConfiguration<KasaHareket>
{
    public void Configure(EntityTypeBuilder<KasaHareket> builder)
    {
        builder.ToTable("KasaHareket");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.IslemTipi).IsRequired();
        builder.Property(x => x.Tutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.ReferansTip).HasMaxLength(100);
        builder.Property(x => x.Tarih).IsRequired();

        builder.HasIndex(x => new { x.KasaId, x.Tarih });
        builder.HasIndex(x => x.VardiyaId);

        builder.HasOne(x => x.Kasa)
            .WithMany(x => x.KasaHareketleri)
            .HasForeignKey(x => x.KasaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vardiya)
            .WithMany(x => x.KasaHareketleri)
            .HasForeignKey(x => x.VardiyaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Kullanici)
            .WithMany()
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cihaz)
            .WithMany(x => x.KasaHareketleri)
            .HasForeignKey(x => x.CihazId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
