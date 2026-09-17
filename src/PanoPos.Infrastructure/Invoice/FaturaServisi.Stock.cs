using Dapper;
using PanoPos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Audit;
using PanoPos.Infrastructure.Stock;

namespace PanoPos.Infrastructure.Invoice;

public sealed partial class FaturaServisi
{
    private async Task LockOrderAsync(long id, CancellationToken ct)
    {
        if (!_dbContext.Database.IsSqlServer()) return;
        // Serialize repeated conversions before reading the order state.
        await _dbContext.Database.GetDbConnection().ExecuteScalarAsync<long?>(new CommandDefinition(
            "SELECT Id FROM Siparis WITH (UPDLOCK, HOLDLOCK) WHERE Id=@Id AND SilindiMi=0 AND (@TenantId IS NULL OR (TenantId=@TenantId AND SubeId=@SubeId))",
            new { Id = id, TenantId = _dbContext.Baglam()?.TenantId, SubeId = _dbContext.Baglam()?.SubeId }, _dbContext.Database.CurrentTransaction!.GetDbTransaction(), cancellationToken: ct));
    }

    private async Task<long> ResolveDepoAsync(Siparis order, long? requested, CancellationToken ct)
    {
        if (!await _dbContext.Subeler.YetkiliSube(_dbContext).AnyAsync(x => x.Id == order.SubeId && x.TenantId == order.TenantId && x.AktifMi, ct))
            throw StockError("Siparisin tenant/sube bilgisi gecersiz.");
        var query = _dbContext.Depolar.SubeKapsami(_dbContext).Where(x => x.TenantId == order.TenantId &&
            x.SubeId == order.SubeId && x.AktifMi);
        if (requested.HasValue)
        {
            if (!await query.AnyAsync(x => x.Id == requested.Value, ct))
                throw StockError("Depo ayni tenant/sube icinde aktif ve silinmemis olmalidir.");
            return requested.Value;
        }
        var defaults = await query.Where(x => x.VarsayilanMi).Select(x => x.Id).Take(2).ToListAsync(ct);
        if (defaults.Count != 1)
            throw StockError("Subede tam bir aktif varsayilan depo bulunmalidir.");
        return defaults[0];
    }

    private async Task CreateSalesStockAsync(Fatura invoice, CancellationToken ct)
    {
        if (await _dbContext.StokFisleri.SubeKapsami(_dbContext).IgnoreQueryFilters().AnyAsync(x => x.FaturaId == invoice.Id, ct))
            throw StockError("Fatura icin stok cikisi zaten mevcut.");
        var fis = new StokFis {
            TenantId = invoice.TenantId, SubeId = invoice.SubeId, DepoId = invoice.DepoId,
            FaturaId = invoice.Id, StokFisTipi = StokFisTipi.Satis,
            FisNo = $"SATIS-{invoice.Id}", FisTarihi = invoice.OlusturmaTarihi,
            Aciklama = $"Satis faturasi: {invoice.FaturaNo}"
        };
        var lines = await _dbContext.FaturaDetaylari.SubeKapsami(_dbContext).Where(x => x.FaturaId == invoice.Id).OrderBy(x => x.Id).ToListAsync(ct);
        foreach (var line in lines)
        {
            if (line.TenantId != invoice.TenantId || line.SubeId != invoice.SubeId ||
                !line.AktifMi || line.Miktar <= 0 || line.BirimKatsayi is null or <= 0 ||
                line.StokKartSatisBirimiId is null ||
                string.IsNullOrWhiteSpace(line.BirimKodu) || string.IsNullOrWhiteSpace(line.BirimAdi) ||
                decimal.Round(line.Miktar, 4) != line.Miktar ||
                decimal.Round(line.BirimKatsayi.Value, 4) != line.BirimKatsayi.Value)
                throw StockError("Satir miktar veya birim snapshot bilgisi eksik/gecersiz; siparisi kontrol edin.");
            _ = StokServisi.ToBaseQuantity(line.Miktar, line.BirimKatsayi.Value);
            if (!await _dbContext.StokKartler.TenantKapsami(_dbContext).AnyAsync(x => x.Id == line.StokKartId && x.TenantId == invoice.TenantId && x.AktifMi, ct) ||
                !await _dbContext.StokKartSatisBirimleri.TenantKapsami(_dbContext).AnyAsync(x => x.Id == line.StokKartSatisBirimiId &&
                    x.StokKartId == line.StokKartId && x.TenantId == invoice.TenantId && x.AktifMi, ct))
                throw StockError("Satirin stok karti veya satis birimi gecersiz.");
            if (line.StokKartVaryantId.HasValue && !await _dbContext.StokKartVaryantlari.TenantKapsami(_dbContext).AnyAsync(x =>
                x.Id == line.StokKartVaryantId && x.StokKartId == line.StokKartId && x.TenantId == invoice.TenantId && x.AktifMi, ct))
                throw StockError("Satirin varyanti gecersiz.");

            // Master ownership is validated; the stored snapshot supplies unit values.
            fis.Detaylar.Add(new StokFisDetay {
                TenantId = invoice.TenantId, SubeId = invoice.SubeId, StokKartId = line.StokKartId,
                StokKartVaryantId = line.StokKartVaryantId, StokKartSatisBirimiId = line.StokKartSatisBirimiId.Value,
                BirimKodu = line.BirimKodu, BirimAdi = line.BirimAdi, Katsayi = line.BirimKatsayi.Value, Miktar = line.Miktar
            });
        }
        _dbContext.StokFisleri.Add(fis);
        await _dbContext.SaveChangesAsync(ct);
        foreach (var line in fis.Detaylar)
            _dbContext.StokHareketleri.Add(new StokHareket {
                TenantId = invoice.TenantId, SubeId = invoice.SubeId, DepoId = invoice.DepoId,
                StokKartId = line.StokKartId, StokKartVaryantId = line.StokKartVaryantId,
                StokFisId = fis.Id, StokFisDetayId = line.Id, StokHareketTipi = StokHareketTipi.Satis,
                Miktar = -StokServisi.ToBaseQuantity(line.Miktar, line.Katsayi),
                HareketTarihi = fis.FisTarihi, Aciklama = fis.Aciklama
            });
        await _dbContext.SaveChangesAsync(ct);
        await new IslemLogServisi(_dbContext).LogEkleAsync(new() {
            TenantId = invoice.TenantId, SubeId = invoice.SubeId, ModulAdi = "Fatura",
            IslemTipi = "OlusturStokCikisi", HedefTablo = "Fatura", HedefId = invoice.Id,
            Aciklama = $"StokFisId={fis.Id}; {invoice.FaturaNo}", BasariliMi = true
        }, ct);
    }

    private static UygulamaHatasi StockError(string message) =>
        new(409, "Satis stok islemi yapilamadi", message, "sales_stock_invalid");
}
