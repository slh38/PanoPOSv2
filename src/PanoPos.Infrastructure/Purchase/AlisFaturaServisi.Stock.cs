using Dapper;
using PanoPos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Stock;

namespace PanoPos.Infrastructure.Purchase;

public sealed partial class AlisFaturaServisi
{
    private async Task ValidateDepoAsync(Guid tenantId, long subeId, long depoId, CancellationToken ct)
    {
        if (depoId <= 0 || !await db.Depolar.SubeKapsami(db).AnyAsync(x =>
            x.Id == depoId && x.TenantId == tenantId && x.SubeId == subeId && x.AktifMi, ct))
            throw Error("Ayni tenant ve sube icinde aktif, silinmemis DepoId zorunludur.");
    }

    private async Task LockInvoiceAsync(long id, long subeId, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer()) return;
        var tenant = await TenantAsync(subeId, ct);
        // All invoice mutations take the same row lock before reading status/details.
        await db.Database.GetDbConnection().ExecuteScalarAsync<long?>(new CommandDefinition(
            "SELECT Id FROM AlisFatura WITH (UPDLOCK, HOLDLOCK) WHERE Id=@Id AND TenantId=@TenantId AND SubeId=@SubeId AND SilindiMi=0",
            new { Id = id, TenantId = tenant, SubeId = subeId },
            db.Database.CurrentTransaction!.GetDbTransaction(), cancellationToken: ct));
    }

    private async Task CreatePurchaseStockAsync(AlisFatura f, CancellationToken ct)
    {
        if (!f.AktifMi) throw Error("Pasif alis faturasi kesinlestirilemez.");
        await ValidateDepoAsync(f.TenantId, f.SubeId, f.DepoId, ct);
        var fis = new StokFis {
            TenantId = f.TenantId, SubeId = f.SubeId, DepoId = f.DepoId,
            AlisFaturaId = f.Id, StokFisTipi = StokFisTipi.Alis,
            FisNo = $"ALIS-{f.Id}", FisTarihi = f.FaturaTarihi,
            Aciklama = $"Alis faturasi: {f.FaturaNo}"
        };
        foreach (var line in f.Detaylar.Where(x => !x.SilindiMi).OrderBy(x => x.Id))
        {
            if (line.TenantId != f.TenantId || line.SubeId != f.SubeId || !line.AktifMi ||
                line.Miktar <= 0 || line.Katsayi <= 0 ||
                decimal.Round(line.Miktar, 4) != line.Miktar || decimal.Round(line.Katsayi, 4) != line.Katsayi ||
                line.Miktar > 99999999999999.9999m || line.Katsayi > 99999999999999.9999m ||
                string.IsNullOrWhiteSpace(line.BirimKodu) || string.IsNullOrWhiteSpace(line.BirimAdi))
                throw Error("Alis satirinin miktar, katsayi veya birim snapshot bilgisi gecersiz.");
            _ = StokServisi.ToBaseQuantity(line.Miktar, line.Katsayi);
            if (!await db.StokKartler.TenantKapsami(db).AnyAsync(x => x.Id == line.StokKartId && x.TenantId == f.TenantId && x.AktifMi, ct) ||
                !await db.StokKartSatisBirimleri.TenantKapsami(db).AnyAsync(x => x.Id == line.StokKartSatisBirimiId &&
                    x.StokKartId == line.StokKartId && x.TenantId == f.TenantId && x.AktifMi, ct))
                throw Error("Alis satirinin stok karti veya satis birimi gecersiz.");
            if (line.StokKartVaryantId.HasValue && !await db.StokKartVaryantlari.TenantKapsami(db).AnyAsync(x =>
                x.Id == line.StokKartVaryantId && x.StokKartId == line.StokKartId && x.TenantId == f.TenantId && x.AktifMi, ct))
                throw Error("Alis satirinin varyanti gecersiz.");

            // Validate master ownership, but never replace the invoice's unit snapshot.
            fis.Detaylar.Add(new StokFisDetay {
                TenantId = f.TenantId, SubeId = f.SubeId, StokKartId = line.StokKartId,
                StokKartVaryantId = line.StokKartVaryantId, StokKartSatisBirimiId = line.StokKartSatisBirimiId,
                BirimKodu = line.BirimKodu, BirimAdi = line.BirimAdi, Katsayi = line.Katsayi, Miktar = line.Miktar
            });
        }
        // Read the old ledger balance and update both costs before adding this receipt.
        await (maliyet ?? new StokMaliyetServisi(db)).AlistanGuncelleAsync(f, ct);
        db.StokFisleri.Add(fis);
        await db.SaveChangesAsync(ct);
        foreach (var line in fis.Detaylar)
        {
            db.StokHareketleri.Add(new StokHareket {
                TenantId = f.TenantId, SubeId = f.SubeId, DepoId = f.DepoId,
                StokKartId = line.StokKartId, StokKartVaryantId = line.StokKartVaryantId,
                StokFisId = fis.Id, StokFisDetayId = line.Id, StokHareketTipi = StokHareketTipi.Alis,
                Miktar = StokServisi.ToBaseQuantity(line.Miktar, line.Katsayi),
                HareketTarihi = f.FaturaTarihi, Aciklama = fis.Aciklama
            });
        }
        await db.SaveChangesAsync(ct);
        await new IslemLogServisi(db).LogEkleAsync(new() {
            TenantId = f.TenantId, SubeId = f.SubeId, ModulAdi = "AlisFatura",
            IslemTipi = "KesinlestirStokGirisi", HedefTablo = "AlisFatura", HedefId = f.Id,
            Aciklama = $"StokFisId={fis.Id}; {f.FaturaNo}", BasariliMi = true
        }, ct);
    }
}
