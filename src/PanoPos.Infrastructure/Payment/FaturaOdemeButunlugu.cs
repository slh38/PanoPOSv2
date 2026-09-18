using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Payment;

internal static class FaturaOdemeButunlugu
{
    // Call inside the same transaction as payment/close writes.
    internal static async Task<Fatura> KilitleAsync(PanoPosDbContext db, long id, CancellationToken ct)
    {
        var scope = await db.Faturalar.SubeKapsami(db).AsNoTracking()
            .Where(x => x.Id == id).Select(x => new { x.TenantId, x.SubeId }).SingleOrDefaultAsync(ct)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");
        if (db.Database.IsSqlServer())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Fatura WITH (UPDLOCK,HOLDLOCK) WHERE Id={id} AND TenantId={scope.TenantId} AND SubeId={scope.SubeId}", ct);
        var fatura = await db.Faturalar.SubeKapsami(db).SingleAsync(x => x.Id == id, ct);
        await db.Entry(fatura).ReloadAsync(ct);
        if (fatura.SilindiMi)
            throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");
        return fatura;
    }

    internal static async Task<decimal> ToplamAsync(PanoPosDbContext db, Fatura fatura, CancellationToken ct)
    {
        var query = db.Tahsilatlar.AsNoTracking().Where(x => x.FaturaId == fatura.Id &&
            x.TenantId == fatura.TenantId && x.SubeId == fatura.SubeId && x.AktifMi);
        // SQLite cannot aggregate decimal exactly; keep its relational test path decimal-only.
        return db.Database.IsSqlServer() ? await query.SumAsync(x => x.Tutar, ct)
            : (await query.Select(x => x.Tutar).ToListAsync(ct)).Sum();
    }

    internal static void Guncelle(Fatura fatura, decimal toplam, DateTime tarih, long kullaniciId)
    {
        if (toplam < 0 || toplam > fatura.NetToplam)
            throw new UygulamaHatasi(409, "Odeme hatasi", "Tahsilat toplami fatura net toplamini gecemez.", "payment_total_exceeds_invoice");
        fatura.OdenenTutar = toplam;
        fatura.KalanTutar = fatura.NetToplam - toplam;
        if (fatura.KalanTutar == 0)
        {
            fatura.Durum = FaturaDurumu.Kapali;
            fatura.KapanisTarihi ??= tarih;
            fatura.KapatanKullaniciId ??= kullaniciId;
            fatura.AktifMi = false;
        }
        else
        {
            fatura.Durum = FaturaDurumu.Acik;
            fatura.KapanisTarihi = null; fatura.KapatanKullaniciId = null;
            fatura.AktifMi = true;
        }
    }
}
