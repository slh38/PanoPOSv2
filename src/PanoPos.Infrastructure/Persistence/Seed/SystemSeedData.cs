using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Seed;

public static class SystemSeedData
{
    public static readonly Guid TenantGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly DateTime SeedDate = new(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);
    private const string AdminPinHash = "100000.AQIDBAUGBwgJCgsMDQ4PEA==.zXEe8seaNwtLmNvAYvfpAmiMOk6AXt6Jn4slCkkKXHE=";

    public static Tenant Tenant => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        Ad = "Pano Demo Tenant",
        Kod = "PANO",
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static Sube Sube => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        Ad = "Merkez Sube",
        Kod = "MRKZ",
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static Cihaz Cihaz => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        Ad = "Ana Kasa Cihaz",
        Kod = "CIHAZ-001",
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static Kullanici AdminKullanici => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        Ad = "Admin",
        Soyad = "Kullanici",
        PinHash = AdminPinHash,
        PinSonDegistirmeTarihi = SeedDate,
        SonGirisTarihi = null,
        BasarisizGirisSayisi = 0,
        KilitliMi = false,
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static Rol AdminRol => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        Ad = "Admin",
        Kod = "ADMIN",
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static KullaniciRol AdminKullaniciRol => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        KullaniciId = 1,
        RolId = 1,
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };

    public static KullaniciSube AdminKullaniciSube => new()
    {
        Id = 1,
        TenantId = TenantGuid,
        SubeId = 1,
        KullaniciId = 1,
        BagliSubeId = 1,
        OlusturmaTarihi = SeedDate,
        GuncellemeTarihi = SeedDate,
        AktifMi = true,
        SilindiMi = false
    };
}
