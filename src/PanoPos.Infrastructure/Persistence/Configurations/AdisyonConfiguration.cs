using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class AdisyonConfiguration : IEntityTypeConfiguration<Adisyon>
{
    public void Configure(EntityTypeBuilder<Adisyon> builder)
    {
        builder.ToTable("Adisyon");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.MasaId).IsRequired();
        builder.Property(x => x.AcanKullaniciId).IsRequired();
        builder.Property(x => x.AcanCihazId).IsRequired();
        builder.Property(x => x.AcilisTarihi).IsRequired();
        builder.Property(x => x.Durum).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasOne(x => x.Masa)
            .WithMany(x => x.Adisyonlar)
            .HasForeignKey(x => x.MasaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Kullanici>()
            .WithMany()
            .HasForeignKey(x => x.AcanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Cihaz>()
            .WithMany()
            .HasForeignKey(x => x.AcanCihazId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MasaId, x.Durum });
        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.AcilisTarihi });
    }
}
