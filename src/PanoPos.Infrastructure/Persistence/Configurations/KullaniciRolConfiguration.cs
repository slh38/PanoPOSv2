using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KullaniciRolConfiguration : IEntityTypeConfiguration<KullaniciRol>
{
    public void Configure(EntityTypeBuilder<KullaniciRol> builder)
    {
        builder.ToTable("KullaniciRol");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.HasIndex(x => new { x.KullaniciId, x.RolId }).IsUnique();

        builder.HasOne(x => x.Kullanici)
            .WithMany(x => x.KullaniciRoller)
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Rol)
            .WithMany(x => x.KullaniciRoller)
            .HasForeignKey(x => x.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
