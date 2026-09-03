using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class SiparisConfiguration : IEntityTypeConfiguration<Siparis>
{
    public void Configure(EntityTypeBuilder<Siparis> builder)
    {
        builder.ToTable("Siparis");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.SiparisNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SiparisTipi).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Kur).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.AraToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.GenelIndirimOrani).HasColumnType("decimal(5,2)");
        builder.Property(x => x.GenelIndirimTutari).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.NetToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ToplamTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Durum).HasColumnType("smallint").IsRequired();

        builder.HasOne(x => x.Adisyon)
            .WithMany()
            .HasForeignKey(x => x.AdisyonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cari)
            .WithMany()
            .HasForeignKey(x => x.CariId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.Durum });
        builder.HasIndex(x => new { x.TenantId, x.SiparisNo }).IsUnique();
        builder.HasIndex(x => new { x.AdisyonId, x.Durum });
    }
}
