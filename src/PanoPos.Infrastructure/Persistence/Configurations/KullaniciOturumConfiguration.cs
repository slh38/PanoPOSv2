using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KullaniciOturumConfiguration : IEntityTypeConfiguration<KullaniciOturum>
{
    public void Configure(EntityTypeBuilder<KullaniciOturum> builder)
    {
        builder.ToTable("KullaniciOturum");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.GirisTarihi).IsRequired();
        builder.HasIndex(x => new { x.KullaniciId, x.AktifMi });

        builder.HasOne(x => x.Kullanici)
            .WithMany(x => x.KullaniciOturumlar)
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cihaz)
            .WithMany(x => x.KullaniciOturumlar)
            .HasForeignKey(x => x.CihazId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
