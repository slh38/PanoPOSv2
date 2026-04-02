using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> builder)
    {
        builder.ToTable("Kullanici");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.Ad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Soyad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Pin).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Pin }).IsUnique();
    }
}
