using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class IslemLogConfiguration : IEntityTypeConfiguration<IslemLog>
{
    public void Configure(EntityTypeBuilder<IslemLog> builder)
    {
        builder.ToTable("IslemLog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.SubeId).IsRequired();
        builder.Property(x => x.ModulAdi).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EkranAdi).HasMaxLength(100);
        builder.Property(x => x.ButonAdi).HasMaxLength(100);
        builder.Property(x => x.IslemTipi).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HedefTablo).HasMaxLength(100);
        builder.Property(x => x.Aciklama).HasMaxLength(500);
        builder.Property(x => x.HataKodu).HasMaxLength(100);
        builder.Property(x => x.HataMesaji).HasMaxLength(1000);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.OlusturmaTarihi).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.OlusturmaTarihi }).IsDescending(false, false, true);
        builder.HasIndex(x => new { x.TenantId, x.KullaniciId, x.OlusturmaTarihi }).IsDescending(false, false, true);
        builder.HasIndex(x => new { x.TenantId, x.BasariliMi, x.OlusturmaTarihi }).IsDescending(false, false, true);

        builder.HasOne(x => x.Cihaz)
            .WithMany()
            .HasForeignKey(x => x.CihazId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Kullanici)
            .WithMany()
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
