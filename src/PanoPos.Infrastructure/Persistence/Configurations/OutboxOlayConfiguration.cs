using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class OutboxOlayConfiguration : IEntityTypeConfiguration<OutboxOlay>
{
    public void Configure(EntityTypeBuilder<OutboxOlay> builder)
    {
        builder.ToTable("OutboxOlay");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.SubeId).IsRequired();
        builder.Property(x => x.CihazId).IsRequired();
        builder.Property(x => x.OlayTipi).HasMaxLength(100).IsRequired();
        builder.Property(x => x.KaynakTablo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.Durum).HasColumnType("smallint").IsRequired();
        builder.Property(x => x.DenemeSayisi).IsRequired();
        builder.Property(x => x.OlusturmaTarihi).IsRequired();
        builder.Property(x => x.SonHataMesaji).HasMaxLength(1000);

        builder.HasIndex(x => new { x.TenantId, x.SubeId, x.Durum, x.OlusturmaTarihi });
        builder.HasIndex(x => new { x.KaynakTablo, x.KaynakId });

        builder.HasOne(x => x.Cihaz)
            .WithMany()
            .HasForeignKey(x => x.CihazId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



