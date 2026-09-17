using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PanoPos.Application.Common;
using PanoPos.Application.Stock;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Stock;

public sealed class StokMaliyetServisi(PanoPosDbContext db) : IStokMaliyetServisi
{
    // Until tenant base-currency configuration is introduced, conversion has one boundary.
    private const string AnaParaBirimi = "TRY";
    private const decimal MaxCost = 999999999999.999999m;

    public async Task AlistanGuncelleAsync(AlisFatura f, CancellationToken ct = default)
    {
        RequireTransaction();
        if (f.Durum != AlisFaturaDurumu.Taslak ||
            await db.StokFisleri.SubeKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.AlisFaturaId == f.Id, ct))
            throw Error("Maliyet yalniz ilk alis kesinlestirmesinde guncellenebilir.");
        var currency = f.ParaBirimKodu.Trim().ToUpperInvariant();
        var rate = currency == AnaParaBirimi ? 1m : f.Kur;
        if (rate <= 0 || currency.Length == 0) throw Error("Alis snapshot kuru gecersiz.");

        var groups = f.Detaylar.Where(x => !x.SilindiMi)
            .GroupBy(x => new { x.StokKartId, x.StokKartVaryantId })
            .OrderBy(x => x.Key.StokKartId).ThenBy(x => x.Key.StokKartVaryantId);
        foreach (var group in groups)
        {
            await ValidateKeyAsync(f.TenantId, f.SubeId, f.DepoId, group.Key.StokKartId, group.Key.StokKartVaryantId, ct);
            await LockCostAsync(f.TenantId, f.SubeId, f.DepoId, group.Key.StokKartId, group.Key.StokKartVaryantId, ct);
            var oldQuantity = await OldQuantityAsync(f.TenantId, f.SubeId, f.DepoId, group.Key.StokKartId, group.Key.StokKartVaryantId, ct);
            var cost = await CostQuery(f.TenantId, f.SubeId, f.DepoId, group.Key.StokKartId, group.Key.StokKartVaryantId).SingleOrDefaultAsync(ct);
            if (cost != null) await db.Entry(cost).ReloadAsync(ct);
            decimal quantity = 0, baseAmount = 0, last, average;
            try
            {
                foreach (var line in group)
                {
                    if (line.TenantId != f.TenantId || line.SubeId != f.SubeId || !line.AktifMi ||
                        line.Miktar <= 0 || line.Katsayi <= 0 || line.Matrah < 0 ||
                        line.FiyatParaBirimKodu.Trim().ToUpperInvariant() != currency ||
                        line.FiyatKur != rate)
                        throw Error("Alis maliyet snapshot bilgileri gecersiz.");
                    quantity = checked(quantity + StokServisi.ToBaseQuantity(line.Miktar, line.Katsayi));
                    // Matrah already includes all discounts and excludes VAT, in document currency.
                    baseAmount = checked(baseAmount + checked(line.Matrah * rate));
                }
                last = CostRound(baseAmount / quantity);
                average = oldQuantity <= 0 ? last : CostRound(
                    checked(oldQuantity * (cost?.AgirlikliOrtalamaMaliyet ?? 0m) + baseAmount) /
                    checked(oldQuantity + quantity));
            }
            catch (OverflowException) { throw Error("Maliyet sayisal siniri asiyor."); }

            var now = DateTime.UtcNow;
            if (cost == null)
            {
                cost = new StokMaliyet { TenantId = f.TenantId, SubeId = f.SubeId, DepoId = f.DepoId,
                    StokKartId = group.Key.StokKartId, StokKartVaryantId = group.Key.StokKartVaryantId, OlusturmaTarihi = now };
                db.StokMaliyetleri.Add(cost);
            }
            cost.SonAlisMaliyeti = last;
            cost.AgirlikliOrtalamaMaliyet = average;
            cost.SonAlisTarihi = f.FaturaTarihi;
            cost.SonGuncellemeTarihi = now;
            cost.GuncellemeTarihi = now;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task SatisSnapshotAsync(Fatura f, IReadOnlyCollection<FaturaDetay> lines, CancellationToken ct = default)
    {
        RequireTransaction();
        var method = await MethodAsync(f.TenantId, ct);
        var selected = new Dictionary<(long Stock, long? Variant), decimal>();
        foreach (var line in lines.OrderBy(x => x.StokKartId).ThenBy(x => x.StokKartVaryantId))
        {
            if (line.TenantId != f.TenantId || line.SubeId != f.SubeId || line.BirimKatsayi is null or <= 0)
                throw Error("Satis birim snapshot bilgisi gecersiz.");
            var key = (line.StokKartId, line.StokKartVaryantId);
            if (!selected.TryGetValue(key, out var value))
            {
                // Use the purchase lock order, including missing cost rows.
                await LockCostAsync(f.TenantId, f.SubeId, f.DepoId, key.StokKartId, key.StokKartVaryantId, ct);
                var cost = await CostQuery(f.TenantId, f.SubeId, f.DepoId, key.StokKartId, key.StokKartVaryantId)
                    .AsNoTracking().SingleOrDefaultAsync(ct);
                value = Select(cost, method);
                selected.Add(key, value);
            }
            try { line.BirimMaliyet = CostRound(checked(value * line.BirimKatsayi.Value)); }
            catch (OverflowException) { throw Error("Satis birim maliyeti sayisal siniri asiyor."); }
            line.MaliyetYontemi = method;
        }
    }

    public async Task<StokMaliyetDto> GetAsync(long subeId, long depoId, long stokKartId, long? varyantId, CancellationToken ct = default)
    {
        var tenant = await TenantAsync(subeId, ct);
        await ValidateKeyAsync(tenant, subeId, depoId, stokKartId, varyantId, ct);
        var method = await MethodAsync(tenant, ct);
        var cost = await CostQuery(tenant, subeId, depoId, stokKartId, varyantId).AsNoTracking().SingleOrDefaultAsync(ct);
        return new() { DepoId = depoId, StokKartId = stokKartId, StokKartVaryantId = varyantId,
            ParaBirimKodu = AnaParaBirimi, SonAlisMaliyeti = cost?.SonAlisMaliyeti ?? 0,
            AgirlikliOrtalamaMaliyet = cost?.AgirlikliOrtalamaMaliyet ?? 0,
            MaliyetYontemi = method, SeciliMaliyet = Select(cost, method) };
    }

    public async Task<MaliyetYontemi> YontemDegistirAsync(long subeId, MaliyetYontemi method, CancellationToken ct = default)
    {
        ValidateMethod(method);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var tenant = await TenantAsync(subeId, ct);
        if (db.Database.IsSqlServer())
            await db.Database.GetDbConnection().ExecuteScalarAsync<long>(new CommandDefinition(
                "SELECT Id FROM Tenant WITH (UPDLOCK,HOLDLOCK) WHERE TenantId=@TenantId",
                new { TenantId = tenant }, tx.GetDbTransaction(), cancellationToken: ct));
        var settings = await db.TenantAyarlari.TenantKapsami(db).IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenant, ct);
        if (settings?.SilindiMi == true) throw Error("Tenant ayari silinmis.");
        if (settings == null)
        {
            settings = new TenantAyar { TenantId = tenant, SubeId = subeId };
            db.TenantAyarlari.Add(settings);
        }
        settings.MaliyetYontemi = method;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return method;
    }

    private IQueryable<StokMaliyet> CostQuery(Guid tenant, long sube, long depo, long stock, long? variant) =>
        db.StokMaliyetleri.SubeKapsami(db).Where(x => x.TenantId == tenant && x.SubeId == sube && x.DepoId == depo &&
            x.StokKartId == stock && x.StokKartVaryantId == variant);

    private async Task<MaliyetYontemi> MethodAsync(Guid tenant, CancellationToken ct)
    {
        var method = await db.TenantAyarlari.TenantKapsami(db).Where(x => x.TenantId == tenant)
            .Select(x => (MaliyetYontemi?)x.MaliyetYontemi).SingleOrDefaultAsync(ct) ?? MaliyetYontemi.AgirlikliOrtalama;
        ValidateMethod(method);
        return method;
    }

    private static decimal Select(StokMaliyet? cost, MaliyetYontemi method) =>
        method == MaliyetYontemi.SonAlis ? cost?.SonAlisMaliyeti ?? 0 : cost?.AgirlikliOrtalamaMaliyet ?? 0;

    private static decimal CostRound(decimal value)
    {
        var rounded = decimal.Round(value, 6, MidpointRounding.AwayFromZero);
        if (rounded < 0 || rounded > MaxCost) throw Error("Maliyet decimal(18,6) sinirinda olmalidir.");
        return rounded;
    }

    private void RequireTransaction()
    {
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Maliyet belge transaction'i icinde islenmelidir.");
    }

    private static void ValidateMethod(MaliyetYontemi method)
    {
        if (method is not (MaliyetYontemi.SonAlis or MaliyetYontemi.AgirlikliOrtalama))
            throw Error("Maliyet yontemi gecersiz.");
    }

    private async Task<Guid> TenantAsync(long sube, CancellationToken ct)
    {
        var tenant = (await db.Subeler.YetkiliSube(db).AsNoTracking().SingleOrDefaultAsync(x => x.Id == sube && x.AktifMi, ct)
            ?? throw Error("Aktif sube bulunamadi.")).TenantId;
        if (!await db.Tenantler.TenantKapsami(db).AnyAsync(x => x.TenantId == tenant && x.AktifMi, ct))
            throw Error("Aktif tenant bulunamadi.");
        return tenant;
    }

    private async Task ValidateKeyAsync(Guid tenant, long sube, long depo, long stock, long? variant, CancellationToken ct)
    {
        if (!await db.Depolar.SubeKapsami(db).AnyAsync(x => x.Id == depo && x.TenantId == tenant && x.SubeId == sube && x.AktifMi, ct) ||
            !await db.StokKartler.TenantKapsami(db).AnyAsync(x => x.Id == stock && x.TenantId == tenant && x.AktifMi, ct))
            throw Error("Depo/stok tenant ve sube kapsami gecersiz.");
        if (variant.HasValue && !await db.StokKartVaryantlari.TenantKapsami(db).AnyAsync(x =>
            x.Id == variant && x.StokKartId == stock && x.TenantId == tenant && x.AktifMi, ct))
            throw Error("Varyant stok kartina/tenant'a ait degil.");
    }

    private async Task LockCostAsync(Guid tenant, long sube, long depo, long stock, long? variant, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer()) return;
        var sql = "SELECT Id FROM StokMaliyet WITH (UPDLOCK,HOLDLOCK) WHERE TenantId=@TenantId AND SubeId=@SubeId AND DepoId=@DepoId AND StokKartId=@StokKartId AND " +
            (variant.HasValue ? "StokKartVaryantId=@VariantId" : "StokKartVaryantId IS NULL");
        await db.Database.GetDbConnection().ExecuteScalarAsync<long?>(new CommandDefinition(sql,
            new { TenantId = tenant, SubeId = sube, DepoId = depo, StokKartId = stock, VariantId = variant },
            db.Database.CurrentTransaction!.GetDbTransaction(), cancellationToken: ct));
    }

    private async Task<decimal> OldQuantityAsync(Guid tenant, long sube, long depo, long stock, long? variant, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer())
            return (await db.StokHareketleri.SubeKapsami(db).Where(x => x.TenantId == tenant && x.SubeId == sube &&
                x.DepoId == depo && x.StokKartId == stock && x.StokKartVaryantId == variant).Select(x => x.Miktar).ToListAsync(ct)).Sum();
        var sql = "SELECT COALESCE(SUM(Miktar),0) FROM StokHareket WITH (UPDLOCK,HOLDLOCK) WHERE TenantId=@TenantId AND SubeId=@SubeId AND DepoId=@DepoId AND StokKartId=@StokKartId AND " +
            (variant.HasValue ? "StokKartVaryantId=@VariantId" : "StokKartVaryantId IS NULL");
        return await db.Database.GetDbConnection().ExecuteScalarAsync<decimal>(new CommandDefinition(sql,
            new { TenantId = tenant, SubeId = sube, DepoId = depo, StokKartId = stock, VariantId = variant },
            db.Database.CurrentTransaction!.GetDbTransaction(), cancellationToken: ct));
    }

    private static UygulamaHatasi Error(string message) => new(400, "Stok maliyet hatasi", message, "stock_cost_invalid");
}
