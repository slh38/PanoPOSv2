using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokKartConfiguration : IEntityTypeConfiguration<StokKart>
{
    public void Configure(EntityTypeBuilder<StokKart> builder)
    {
        builder.ToTable("StokKart");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.StokKartKodu).HasMaxLength(50);
        builder.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.StokKartTipi).IsRequired();

        builder.HasOne(x => x.StokKategori)
            .WithMany()
            .HasForeignKey(x => x.StokKategoriId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StokGrup)
            .WithMany()
            .HasForeignKey(x => x.StokGrupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.SilindiMi });
        builder.HasIndex(x => new { x.TenantId, x.StokKartKodu }).IsUnique().HasFilter("[StokKartKodu] IS NOT NULL");
    }
}
