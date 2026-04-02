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
        builder.Property(x => x.PinHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.PinSonDegistirmeTarihi);
        builder.Property(x => x.SonGirisTarihi);
        builder.Property(x => x.BasarisizGirisSayisi).IsRequired();
        builder.Property(x => x.KilitliMi).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.AktifMi });
    }
}
