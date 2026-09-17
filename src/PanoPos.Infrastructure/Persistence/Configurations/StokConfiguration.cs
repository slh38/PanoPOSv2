using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokFisConfiguration : IEntityTypeConfiguration<StokFis>
{
    public void Configure(EntityTypeBuilder<StokFis> b)
    {
        b.ToTable("StokFis", t => t.HasCheckConstraint("CK_StokFis_Depo",
            "([StokFisTipi] = 7 AND [DepoId] IS NULL AND [KaynakDepoId] IS NOT NULL AND [HedefDepoId] IS NOT NULL AND [KaynakDepoId] <> [HedefDepoId]) OR " +
            "([StokFisTipi] <> 7 AND [DepoId] IS NOT NULL AND [KaynakDepoId] IS NULL AND [HedefDepoId] IS NULL)"));
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.HasOne<AlisFatura>().WithMany().HasForeignKey(x => x.AlisFaturaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.AlisFaturaId).IsUnique().HasFilter("[AlisFaturaId] IS NOT NULL");
        b.ToTable("StokFis", t => t.HasCheckConstraint("CK_StokFis_AlisFatura",
            "[AlisFaturaId] IS NULL OR [StokFisTipi] = 1"));
        b.Property(x => x.FisNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(500);
        b.HasIndex(x => new { x.TenantId, x.FisNo }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.SubeId, x.FisTarihi, x.StokFisTipi });
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sube>().WithMany().HasForeignKey(x => x.SubeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.DepoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.KaynakDepoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.HedefDepoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class StokFisDetayConfiguration : IEntityTypeConfiguration<StokFisDetay>
{
    public void Configure(EntityTypeBuilder<StokFisDetay> b)
    {
        b.ToTable("StokFisDetay");
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.Property(x => x.BirimKodu).HasMaxLength(30).IsRequired();
        b.Property(x => x.BirimAdi).HasMaxLength(100).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(500);
        b.Property(x => x.Katsayi).HasPrecision(18, 4);
        b.Property(x => x.Miktar).HasPrecision(18, 4);
        b.Property(x => x.SistemMiktari).HasPrecision(18, 4);
        b.Property(x => x.SayilanMiktar).HasPrecision(18, 4);
        b.Property(x => x.FarkMiktari).HasPrecision(18, 4);
        b.HasOne(x => x.StokFis).WithMany(x => x.Detaylar).HasForeignKey(x => x.StokFisId).OnDelete(DeleteBehavior.Restrict);
        b.HasAlternateKey(x => new { x.Id, x.StokFisId });
        b.HasOne<StokKart>().WithMany().HasForeignKey(x => x.StokKartId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartVaryant>().WithMany().HasForeignKey(x => x.StokKartVaryantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartSatisBirimi>().WithMany().HasForeignKey(x => x.StokKartSatisBirimiId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class StokHareketConfiguration : IEntityTypeConfiguration<StokHareket>
{
    public void Configure(EntityTypeBuilder<StokHareket> b)
    {
        b.ToTable("StokHareket", t => t.HasCheckConstraint("CK_StokHareket_Miktar",
            "[Miktar] <> 0 AND ([StokHareketTipi] <> 3 OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4,5) OR [Miktar] > 0)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Miktar).HasPrecision(18, 4);
        b.Property(x => x.Aciklama).HasMaxLength(500);
        b.HasIndex(x => new { x.TenantId, x.SubeId, x.DepoId, x.StokKartId, x.StokKartVaryantId });
        b.HasIndex(x => new { x.StokFisDetayId, x.StokHareketTipi }).IsUnique();
        b.HasOne(x => x.StokFisDetay).WithMany()
            .HasForeignKey(x => new { x.StokFisDetayId, x.StokFisId })
            .HasPrincipalKey(x => new { x.Id, x.StokFisId }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokFis>().WithMany().HasForeignKey(x => x.StokFisId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sube>().WithMany().HasForeignKey(x => x.SubeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.DepoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKart>().WithMany().HasForeignKey(x => x.StokKartId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartVaryant>().WithMany().HasForeignKey(x => x.StokKartVaryantId).OnDelete(DeleteBehavior.Restrict);
    }
}
