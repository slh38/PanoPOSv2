using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class VardiyaConfiguration : IEntityTypeConfiguration<Vardiya>
{
    public void Configure(EntityTypeBuilder<Vardiya> builder)
    {
        builder.ToTable("Vardiya");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.AcilisTarihi).IsRequired();
        builder.Property(x => x.AcilisNakit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.KapanisTarihi);

        builder.HasIndex(x => new { x.KasaId, x.AktifMi });
        builder.HasIndex(x => new { x.CihazId, x.AktifMi });

        builder.HasOne(x => x.Kasa)
            .WithMany(x => x.Vardiyalar)
            .HasForeignKey(x => x.KasaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cihaz)
            .WithMany(x => x.Vardiyalar)
            .HasForeignKey(x => x.CihazId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Kullanici)
            .WithMany()
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
