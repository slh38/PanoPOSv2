using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Fatura");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.FaturaNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Kur).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.AraToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.GenelIndirimOrani).HasColumnType("decimal(5,2)");
        builder.Property(x => x.GenelIndirimTutari).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.NetToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.OdenenTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.KalanTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ToplamTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Durum).HasColumnType("smallint").IsRequired();

        builder.HasOne(x => x.Siparis)
            .WithMany()
            .HasForeignKey(x => x.SiparisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cari)
            .WithMany()
            .HasForeignKey(x => x.CariId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.Durum });
        builder.HasIndex(x => new { x.TenantId, x.FaturaNo }).IsUnique();
        builder.HasIndex(x => x.SiparisId);
    }
}
