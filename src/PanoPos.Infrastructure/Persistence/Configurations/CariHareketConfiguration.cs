using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class CariHareketConfiguration : IEntityTypeConfiguration<CariHareket>
{
    public void Configure(EntityTypeBuilder<CariHareket> builder)
    {
        builder.ToTable("CariHareket");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.HareketTipi).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Tutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Kur).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.YerelTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.HareketTarihi).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasOne(x => x.Cari)
            .WithMany()
            .HasForeignKey(x => x.CariId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Fatura)
            .WithMany()
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tahsilat)
            .WithMany()
            .HasForeignKey(x => x.TahsilatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.CariId, x.HareketTarihi });
        builder.HasIndex(x => x.TahsilatId);
    }
}
