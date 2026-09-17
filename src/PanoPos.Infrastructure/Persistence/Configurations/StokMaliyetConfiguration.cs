using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class StokMaliyetConfiguration : IEntityTypeConfiguration<StokMaliyet>
{
    public void Configure(EntityTypeBuilder<StokMaliyet> b)
    {
        b.ToTable("StokMaliyet");
        b.HasKey(x => x.Id);
        b.Property(x => x.SonAlisMaliyeti).HasPrecision(18, 6);
        b.Property(x => x.AgirlikliOrtalamaMaliyet).HasPrecision(18, 6);
        // Separate null/non-null indexes also enforce uniqueness on SQLite.
        b.HasIndex(x => new { x.TenantId, x.SubeId, x.DepoId, x.StokKartId })
            .IsUnique().HasFilter("[StokKartVaryantId] IS NULL");
        b.HasIndex(x => new { x.TenantId, x.SubeId, x.DepoId, x.StokKartId, x.StokKartVaryantId })
            .IsUnique().HasFilter("[StokKartVaryantId] IS NOT NULL");
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sube>().WithMany().HasForeignKey(x => x.SubeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.DepoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKart>().WithMany().HasForeignKey(x => x.StokKartId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartVaryant>().WithMany().HasForeignKey(x => x.StokKartVaryantId).OnDelete(DeleteBehavior.Restrict);
    }
}
