using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class VardiyaKapanisConfiguration : IEntityTypeConfiguration<VardiyaKapanis>
{
    public void Configure(EntityTypeBuilder<VardiyaKapanis> builder)
    {
        builder.ToTable("VardiyaKapanis");
        PanoPosDbContext.ConfigureBaseEntity(builder);

        builder.Property(x => x.BeklenenNakit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.SayilanNakit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.FarkTutar).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.KartToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.VeresiyeToplam).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(500);

        builder.HasIndex(x => x.VardiyaId).IsUnique();

        builder.HasOne(x => x.Vardiya)
            .WithOne(x => x.VardiyaKapanis)
            .HasForeignKey<VardiyaKapanis>(x => x.VardiyaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
