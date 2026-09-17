using System.Data;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Audit;
using PanoPos.Application.Common;
using PanoPos.Application.Stock;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Stock;

public sealed partial class StokServisi(PanoPosDbContext db, IIslemLogServisi audit) : IStokServisi
{
    private const decimal MaxQuantity = 99999999999999.9999m;

    public Task<StokFisDto> DevirAsync(StokFisOlusturRequest request, CancellationToken ct = default)
        => CreateAsync(request, StokFisTipi.Devir, ct);
    public Task<StokFisDto> SayimAsync(StokFisOlusturRequest request, CancellationToken ct = default)
        => CreateAsync(request, StokFisTipi.Sayim, ct);
    public Task<StokFisDto> TransferAsync(StokFisOlusturRequest request, CancellationToken ct = default)
        => CreateAsync(request, StokFisTipi.DepoTransfer, ct);

    private async Task<StokFisDto> CreateAsync(StokFisOlusturRequest r, StokFisTipi tip, CancellationToken ct)
    {
        if (r.Detaylar is null || r.Detaylar.Count is < 1 or > 500 || r.Detaylar.Any(x => x is null) ||
            r.FisTarihi == default || r.FisNo?.Trim().Length > 50 || r.Aciklama?.Length > 500)
            throw Error("Fis tarihi, numarasi veya detaylari gecersiz.");
        if (tip == StokFisTipi.DepoTransfer)
        {
            if (r.DepoId.HasValue || !r.KaynakDepoId.HasValue || !r.HedefDepoId.HasValue || r.KaynakDepoId == r.HedefDepoId)
                throw Error("Transfer icin farkli kaynak ve hedef depolar secin; DepoId bos olmalidir.");
        }
        else if (!r.DepoId.HasValue || r.KaynakDepoId.HasValue || r.HedefDepoId.HasValue)
            throw Error("Devir ve sayim icin yalniz DepoId kullanilmalidir.");

        // Serializable protects the count's read-and-adjust operation against concurrent inserts.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var tenant = await TenantAsync(r.SubeId, ct);
        foreach (var depoId in new[] { r.DepoId, r.KaynakDepoId, r.HedefDepoId }.Where(x => x.HasValue))
            await ValidateDepoAsync(tenant, r.SubeId, depoId!.Value, ct);

        var fisNo = string.IsNullOrWhiteSpace(r.FisNo) ? await NextNumberAsync(tenant, ct) : r.FisNo.Trim();
        if (await db.StokFisleri.TenantKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.FisNo == fisNo, ct))
            throw new UygulamaHatasi(409, "Stok fisi mevcut", "Bu fis numarasi daha once kullanildi.", "stock_document_duplicate");
        var fis = new StokFis {
            TenantId = tenant, SubeId = r.SubeId, StokFisTipi = tip, FisNo = fisNo,
            FisTarihi = r.FisTarihi, DepoId = r.DepoId, KaynakDepoId = r.KaynakDepoId,
            HedefDepoId = r.HedefDepoId, Aciklama = r.Aciklama?.Trim()
        };
        var counted = new HashSet<(long, long?)>();
        foreach (var item in r.Detaylar.OrderBy(x => x.StokKartId).ThenBy(x => x.StokKartVaryantId))
        {
            ValidateQuantity(item.Miktar);
            if (item.Miktar < 0 || (tip != StokFisTipi.Sayim && item.Miktar == 0) || item.Aciklama?.Length > 500)
                throw Error("Devir/transfer miktari pozitif, sayilan miktar sifir veya pozitif olmalidir.");
            await ValidateStockAsync(tenant, item.StokKartId, item.StokKartVaryantId, ct);
            var unit = await db.StokKartSatisBirimleri.TenantKapsami(db).AsNoTracking().SingleOrDefaultAsync(x =>
                x.Id == item.StokKartSatisBirimiId && x.TenantId == tenant &&
                x.StokKartId == item.StokKartId && x.AktifMi, ct)
                ?? throw Error("Aktif satis birimi bu stok kartina ve tenant'a ait olmalidir.");
            ValidateQuantity(unit.Katsayi);
            if (unit.Katsayi <= 0) throw Error("Satis birimi katsayisi pozitif olmalidir.");
            var baseQuantity = ToBaseQuantity(item.Miktar, unit.Katsayi);
            var detail = new StokFisDetay {
                TenantId = tenant, SubeId = r.SubeId, StokKartId = item.StokKartId,
                StokKartVaryantId = item.StokKartVaryantId, StokKartSatisBirimiId = unit.Id,
                BirimKodu = unit.BirimKodu, BirimAdi = unit.BirimAdi, Katsayi = unit.Katsayi,
                Miktar = item.Miktar, Aciklama = item.Aciklama?.Trim()
            };
            if (tip == StokFisTipi.Sayim)
            {
                if (!counted.Add((item.StokKartId, item.StokKartVaryantId)))
                    throw Error("Ayni stok/varyant bir sayimda bir kez bulunabilir.");
                detail.SistemMiktari = await SumAsync(tenant, r.SubeId, r.DepoId!.Value, item.StokKartId, item.StokKartVaryantId, ct);
                detail.SayilanMiktar = baseQuantity;
                detail.FarkMiktari = baseQuantity - detail.SistemMiktari.Value;
                ValidateQuantity(detail.SistemMiktari.Value);
                ValidateQuantity(detail.FarkMiktari.Value);
            }
            fis.Detaylar.Add(detail);
        }

        db.StokFisleri.Add(fis);
        await db.SaveChangesAsync(ct);
        foreach (var detail in fis.Detaylar)
        {
            var quantity = ToBaseQuantity(detail.Miktar, detail.Katsayi);
            if (tip == StokFisTipi.DepoTransfer)
            {
                AddMovement(fis, detail, r.KaynakDepoId!.Value, -quantity, StokHareketTipi.DepoTransferCikis);
                AddMovement(fis, detail, r.HedefDepoId!.Value, quantity, StokHareketTipi.DepoTransferGiris);
            }
            else
            {
                var effect = tip == StokFisTipi.Sayim ? detail.FarkMiktari!.Value : quantity;
                if (effect != 0)
                    AddMovement(fis, detail, r.DepoId!.Value, effect, tip == StokFisTipi.Sayim ? StokHareketTipi.Sayim : StokHareketTipi.Devir);
            }
        }
        await db.SaveChangesAsync(ct);
        await audit.LogEkleAsync(new() {
            TenantId = tenant, SubeId = fis.SubeId, ModulAdi = "Stok", IslemTipi = tip.ToString(),
            HedefTablo = "StokFis", HedefId = fis.Id, Aciklama = fis.FisNo, BasariliMi = true
        }, ct);
        await tx.CommitAsync(ct);
        return Map(fis);
    }

    private void AddMovement(StokFis fis, StokFisDetay detail, long depoId, decimal quantity, StokHareketTipi tip)
    {
        db.StokHareketleri.Add(new StokHareket {
            TenantId = fis.TenantId, SubeId = fis.SubeId, DepoId = depoId,
            StokKartId = detail.StokKartId, StokKartVaryantId = detail.StokKartVaryantId,
            StokFisId = fis.Id, StokFisDetayId = detail.Id, StokHareketTipi = tip,
            Miktar = quantity, HareketTarihi = fis.FisTarihi, Aciklama = detail.Aciklama
        });
    }

    private async Task<Guid> TenantAsync(long subeId, CancellationToken ct)
    {
        var sube = await db.Subeler.YetkiliSube(db).AsNoTracking().SingleOrDefaultAsync(x => x.Id == subeId && x.AktifMi, ct)
            ?? throw Error("Aktif sube bulunamadi.");
        if (!await db.Tenantler.TenantKapsami(db).AnyAsync(x => x.TenantId == sube.TenantId && x.AktifMi, ct))
            throw Error("Aktif tenant bulunamadi.");
        return sube.TenantId;
    }

    private async Task ValidateDepoAsync(Guid tenant, long subeId, long depoId, CancellationToken ct)
    {
        if (!await db.Depolar.SubeKapsami(db).AnyAsync(x => x.Id == depoId && x.TenantId == tenant && x.SubeId == subeId && x.AktifMi, ct))
            throw Error("Depo aktif olmali ve islemin tenant/subesine ait olmalidir.");
    }

    private async Task ValidateStockAsync(Guid tenant, long stockId, long? variantId, CancellationToken ct)
    {
        if (!await db.StokKartler.TenantKapsami(db).AnyAsync(x => x.Id == stockId && x.TenantId == tenant && x.AktifMi, ct))
            throw Error("Aktif stok karti bu tenant icinde bulunamadi.");
        if (variantId.HasValue && !await db.StokKartVaryantlari.TenantKapsami(db).AnyAsync(x =>
            x.Id == variantId && x.StokKartId == stockId && x.TenantId == tenant && x.AktifMi, ct))
            throw Error("Aktif varyant bu stok kartina ve tenant'a ait olmalidir.");
    }

    private async Task<string> NextNumberAsync(Guid tenant, CancellationToken ct)
    {
        var prefix = $"STK-{DateTime.UtcNow:yyyyMMdd}-";
        var numbers = await db.StokFisleri.TenantKapsami(db).IgnoreQueryFilters().Where(x => x.TenantId == tenant && x.FisNo.StartsWith(prefix))
            .Select(x => x.FisNo).ToListAsync(ct);
        var next = numbers.Select(x => long.TryParse(x[prefix.Length..], out var n) ? n : 0).DefaultIfEmpty().Max() + 1;
        return $"{prefix}{next:000000}";
    }

    internal static decimal ToBaseQuantity(decimal quantity, decimal coefficient)
    {
        decimal result;
        try { result = checked(quantity * coefficient); }
        catch (OverflowException) { throw Error("Temel miktar sayisal siniri asiyor."); }
        // Reject unrepresentable quantities instead of silently losing stock fractions.
        ValidateQuantity(result);
        if (quantity > 0 && result <= 0) throw Error("Temel miktar pozitif olmalidir.");
        return result;
    }

    private static void ValidateQuantity(decimal value)
    {
        if (value < -MaxQuantity || value > MaxQuantity || decimal.Round(value, 4) != value)
            throw Error("Miktar decimal(18,4) sinirlarinda olmalidir.");
    }

    private static UygulamaHatasi Error(string message) => new(400, "Stok islemi gecersiz", message, "stock_invalid");

    private static StokFisDto Map(StokFis f) => new() {
        Id = f.Id, FaturaId = f.FaturaId, AlisFaturaId = f.AlisFaturaId, TenantId = f.TenantId, SubeId = f.SubeId, StokFisTipi = f.StokFisTipi,
        FisNo = f.FisNo, FisTarihi = f.FisTarihi, DepoId = f.DepoId, KaynakDepoId = f.KaynakDepoId,
        HedefDepoId = f.HedefDepoId, Aciklama = f.Aciklama,
        Detaylar = f.Detaylar.OrderBy(x => x.Id).Select(x => new StokFisDetayDto {
            Id = x.Id, StokKartId = x.StokKartId, StokKartVaryantId = x.StokKartVaryantId,
            StokKartSatisBirimiId = x.StokKartSatisBirimiId, BirimKodu = x.BirimKodu, BirimAdi = x.BirimAdi,
            Katsayi = x.Katsayi, Miktar = x.Miktar, SistemMiktari = x.SistemMiktari,
            SayilanMiktar = x.SayilanMiktar, FarkMiktari = x.FarkMiktari, Aciklama = x.Aciklama
        }).ToList()
    };
}
