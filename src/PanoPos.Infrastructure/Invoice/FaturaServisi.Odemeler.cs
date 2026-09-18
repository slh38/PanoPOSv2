using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Invoice;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;

namespace PanoPos.Infrastructure.Invoice;

public sealed partial class FaturaServisi
{
    private Task<List<FaturaOdemeDto>> FaturaOdemeleriAsync(Fatura fatura, CancellationToken ct)
    {
        var cash = _dbContext.KasaHareketleri.Where(h => h.TenantId == fatura.TenantId &&
            h.SubeId == fatura.SubeId && h.AktifMi && !h.SilindiMi && h.ReferansTip == nameof(Tahsilat));
        var bank = _dbContext.BankaHareketleri.Where(h => h.TenantId == fatura.TenantId &&
            h.SubeId == fatura.SubeId && h.AktifMi && !h.SilindiMi && h.FaturaId == fatura.Id);
        var customer = _dbContext.CariHareketleri.Where(h => h.TenantId == fatura.TenantId &&
            h.SubeId == fatura.SubeId && h.AktifMi && !h.SilindiMi && h.FaturaId == fatura.Id);

        // Scalar subqueries remain in one SQL command; no per-payment round trips or join multiplication.
        return (from p in _dbContext.Tahsilatlar.AsNoTracking()
                where p.FaturaId == fatura.Id && p.TenantId == fatura.TenantId &&
                    p.SubeId == fatura.SubeId && p.AktifMi && !p.SilindiMi
                let kasaId = p.OdemeTipi == OdemeTipi.Nakit
                    ? cash.Where(h => h.ReferansId == p.Id).OrderBy(h => h.Id).Select(h => (long?)h.KasaId).FirstOrDefault() : null
                let bankaId = p.OdemeTipi == OdemeTipi.KrediKarti
                    ? bank.Where(h => h.TahsilatId == p.Id).OrderBy(h => h.Id).Select(h => (long?)h.BankaId).FirstOrDefault() : null
                let cariId = p.OdemeTipi == OdemeTipi.Veresiye
                    ? customer.Where(h => h.TahsilatId == p.Id).OrderBy(h => h.Id).Select(h => (long?)h.CariId).FirstOrDefault() : null
                orderby p.TahsilatTarihi, p.Id
                select new FaturaOdemeDto
                {
                    TahsilatId = p.Id, Tarih = p.TahsilatTarihi, OdemeTipi = p.OdemeTipi,
                    Tutar = p.Tutar, ParaBirimKodu = p.ParaBirimKodu, Aciklama = p.Aciklama,
                    KasaId = kasaId,
                    KasaAdi = _dbContext.Kasalar.IgnoreQueryFilters()
                        .Where(k => k.Id == kasaId && k.TenantId == fatura.TenantId && k.SubeId == fatura.SubeId)
                        .Select(k => k.Ad).FirstOrDefault(),
                    BankaId = bankaId,
                    BankaAdi = _dbContext.Bankalar.IgnoreQueryFilters()
                        .Where(b => b.Id == bankaId && b.TenantId == fatura.TenantId && b.SubeId == fatura.SubeId)
                        .Select(b => b.Ad).FirstOrDefault(),
                    CariId = cariId
                }).ToListAsync(ct);
    }
}
