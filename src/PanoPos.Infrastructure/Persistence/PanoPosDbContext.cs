using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Common;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Infrastructure.Persistence;

public sealed class PanoPosDbContext : DbContext
{
    public PanoPosDbContext(DbContextOptions<PanoPosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenantler => Set<Tenant>();
    public DbSet<Sube> Subeler => Set<Sube>();
    public DbSet<Cihaz> Cihazlar => Set<Cihaz>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Rol> Roller => Set<Rol>();
    public DbSet<KullaniciRol> KullaniciRolleri => Set<KullaniciRol>();
    public DbSet<KullaniciSube> KullaniciSubeleri => Set<KullaniciSube>();
    public DbSet<KullaniciOturum> KullaniciOturumlari => Set<KullaniciOturum>();

    public override int SaveChanges()
    {
        ApplyEntityRules();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyEntityRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PanoPosDbContext).Assembly);
        ApplySeedData(modelBuilder);
    }

    public static void ConfigureBaseEntity<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.SubeId).IsRequired();
        builder.Property(x => x.OlusturmaTarihi).IsRequired();
        builder.Property(x => x.GuncellemeTarihi);
        builder.Property(x => x.AktifMi).IsRequired();
        builder.Property(x => x.SilindiMi).IsRequired();
        builder.Property(x => x.OlusturanKullaniciId);
        builder.Property(x => x.GuncelleyenKullaniciId);
        builder.Property(x => x.SilenKullaniciId);
        builder.Property(x => x.SilinmeTarihi);
        builder.HasQueryFilter(x => !x.SilindiMi);
    }

    private void ApplyEntityRules()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.OlusturmaTarihi = utcNow;
                entry.Entity.GuncellemeTarihi = utcNow;
                entry.Entity.AktifMi = true;
                entry.Entity.SilindiMi = false;
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GuncellemeTarihi = utcNow;
                continue;
            }

            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.SoftDelete(entry.Entity.SilenKullaniciId, utcNow);
        }
    }

    private static void ApplySeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasData(SystemSeedData.Tenant);
        modelBuilder.Entity<Sube>().HasData(SystemSeedData.Sube);
        modelBuilder.Entity<Cihaz>().HasData(SystemSeedData.Cihaz);
        modelBuilder.Entity<Kullanici>().HasData(SystemSeedData.AdminKullanici);
        modelBuilder.Entity<Rol>().HasData(SystemSeedData.AdminRol);
        modelBuilder.Entity<KullaniciRol>().HasData(SystemSeedData.AdminKullaniciRol);
        modelBuilder.Entity<KullaniciSube>().HasData(SystemSeedData.AdminKullaniciSube);
    }
}
