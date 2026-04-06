using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class TahsilatConfiguration : IEntityTypeConfiguration<Tahsilat>
{
    public void Configure(EntityTypeBuilder<Tahsilat> builder)
    {
        builder.ToTable("Tahsilat");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.TahsilatFisNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OdemeTipi).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Kur).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.Tutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.YerelTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.TahsilatTarihi).IsRequired();

        builder.HasOne(x => x.Fatura)
            .WithMany()
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.TahsilatTarihi });
        builder.HasIndex(x => x.FaturaId);
        builder.HasIndex(x => new { x.TenantId, x.TahsilatFisNo }).IsUnique();
    }
}
