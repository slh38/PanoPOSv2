using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence.Seed;
namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class KdvConfiguration : IEntityTypeConfiguration<Kdv>
{
    public void Configure(EntityTypeBuilder<Kdv> b)
    {
        b.ToTable("Kdv");
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.Property(x => x.Kod).HasMaxLength(20).IsRequired();
        b.Property(x => x.Ad).HasMaxLength(50).IsRequired();
        b.Property(x => x.Oran).HasPrecision(5, 2);
        b.HasIndex(x => new { x.TenantId, x.Kod }).IsUnique().HasFilter("[AktifMi] = 1 AND [SilindiMi] = 0");
        b.HasIndex(x => new { x.TenantId, x.Oran }).IsUnique().HasFilter("[AktifMi] = 1 AND [SilindiMi] = 0");
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        var rates = new[] { 0m, 1m, 10m, 20m };
        b.HasData(rates.Select((rate, i) => new Kdv { Id = i + 1, TenantId = SystemSeedData.TenantGuid, SubeId = 1,
            Kod = "KDV" + rate, Ad = "%" + rate + " KDV", Oran = rate, AktifMi = true,
            OlusturmaTarihi = SystemSeedData.SeedDate, GuncellemeTarihi = SystemSeedData.SeedDate }));
    }
}
public sealed class TenantAyarConfiguration : IEntityTypeConfiguration<TenantAyar>
{
    public void Configure(EntityTypeBuilder<TenantAyar> b)
    {
        b.ToTable("TenantAyar");
        b.Property(x => x.MaliyetYontemi).HasDefaultValue(PanoPos.Domain.Enums.MaliyetYontemi.AgirlikliOrtalama)
            .HasSentinel(PanoPos.Domain.Enums.MaliyetYontemi.AgirlikliOrtalama);
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.HasIndex(x => x.TenantId).IsUnique();
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasData(new TenantAyar { Id = 1, TenantId = SystemSeedData.TenantGuid, SubeId = 1,
            SatisFiyatlariKdvDahilMi = true, AlisFiyatlariKdvDahilMi = false, AktifMi = true,
            OlusturmaTarihi = SystemSeedData.SeedDate, GuncellemeTarihi = SystemSeedData.SeedDate });
    }
}
public sealed class AlisFaturaConfiguration : IEntityTypeConfiguration<AlisFatura>
{
    public void Configure(EntityTypeBuilder<AlisFatura> b)
    {
        b.ToTable("AlisFatura");
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.HasOne<Depo>().WithMany().HasForeignKey(x => x.DepoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.FaturaNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(500);
        b.Property(x => x.ParaBirimKodu).HasMaxLength(10).IsRequired();
        b.Property(x => x.Kur).HasPrecision(18, 6);
        b.Property(x => x.GenelIndirimOrani).HasPrecision(5, 2);
        foreach (var name in new[] { "AraToplam", "GenelIndirimTutari", "ToplamMatrah", "ToplamKdv", "NetToplam" })
            b.Property<decimal>(name).HasPrecision(18, 2);
        b.HasOne(x => x.CariKart).WithMany().HasForeignKey(x => x.CariId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sube>().WithMany().HasForeignKey(x => x.SubeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantId, x.SubeId, x.FaturaTarihi });
        b.HasIndex(x => new { x.TenantId, x.CariId, x.FaturaNo });
    }
}
public sealed class AlisFaturaDetayConfiguration : IEntityTypeConfiguration<AlisFaturaDetay>
{
    public void Configure(EntityTypeBuilder<AlisFaturaDetay> b)
    {
        b.ToTable("AlisFaturaDetay");
        PanoPosDbContext.ConfigureBaseEntity(b);
        b.Property(x => x.BirimKodu).HasMaxLength(30).IsRequired();
        b.Property(x => x.BirimAdi).HasMaxLength(100).IsRequired();
        b.Property(x => x.FiyatParaBirimKodu).HasMaxLength(10).IsRequired();
        b.Property(x => x.FiyatKur).HasPrecision(18, 6);
        foreach (var name in new[] { "Katsayi", "Miktar", "BirimFiyat" }) b.Property<decimal>(name).HasPrecision(18, 4);
        foreach (var name in new[] { "SatirAraToplam", "IndirimTutari", "GenelIndirimPayi", "Matrah", "KdvTutari", "SatirNetToplam" })
            b.Property<decimal>(name).HasPrecision(18, 2);
        b.Property(x => x.KdvOrani).HasPrecision(5, 2);
        b.Property(x => x.IndirimOrani).HasPrecision(5, 2);
        b.HasOne(x => x.AlisFatura).WithMany(x => x.Detaylar).HasForeignKey(x => x.AlisFaturaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKart>().WithMany().HasForeignKey(x => x.StokKartId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartVaryant>().WithMany().HasForeignKey(x => x.StokKartVaryantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StokKartSatisBirimi>().WithMany().HasForeignKey(x => x.StokKartSatisBirimiId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Kdv>().WithMany().HasForeignKey(x => x.KdvId).OnDelete(DeleteBehavior.Restrict);
    }
}
