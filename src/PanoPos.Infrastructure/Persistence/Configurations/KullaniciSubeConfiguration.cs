using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KullaniciSubeConfiguration : IEntityTypeConfiguration<KullaniciSube>
{
    public void Configure(EntityTypeBuilder<KullaniciSube> builder)
    {
        builder.ToTable("KullaniciSube");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.HasIndex(x => new { x.KullaniciId, x.BagliSubeId }).IsUnique();

        builder.HasOne(x => x.Kullanici)
            .WithMany(x => x.KullaniciSubeler)
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sube)
            .WithMany(x => x.KullaniciSubeler)
            .HasForeignKey(x => x.BagliSubeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
