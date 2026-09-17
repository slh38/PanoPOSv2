using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Purchase;
using PanoPos.Application.Tax;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;
namespace PanoPos.Infrastructure.Purchase;

public sealed partial class AlisFaturaServisi(PanoPosDbContext db, IVergiHesaplamaServisi vergi,
    PanoPos.Application.Stock.IStokMaliyetServisi? maliyet = null) : IAlisFaturaServisi
{
    public async Task<AlisFaturaDto> CreateAsync(AlisFaturaKaydetRequest r, CancellationToken ct = default)
    {
        var tenant = await TenantAsync(r.SubeId, ct);
        var dahil = await db.TenantAyarlari.Where(x => x.TenantId == tenant).Select(x => (bool?)x.AlisFiyatlariKdvDahilMi).SingleOrDefaultAsync(ct) ?? false;
        var f = new AlisFatura { TenantId = tenant, SubeId = r.SubeId, KdvDahilMi = dahil, Durum = AlisFaturaDurumu.Taslak };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await ApplyAsync(f, r, ct);
        db.AlisFaturalar.Add(f);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetByIdAsync(f.Id, f.SubeId, ct);
    }
    public async Task<AlisFaturaDto> UpdateAsync(long id, AlisFaturaKaydetRequest r, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockInvoiceAsync(id, r.SubeId, ct);
        var f = await FindAsync(id, r.SubeId, ct);
        EnsureDraft(f);
        await ApplyAsync(f, r, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetByIdAsync(id, r.SubeId, ct);
    }
    private async Task ApplyAsync(AlisFatura f, AlisFaturaKaydetRequest r, CancellationToken ct)
    {
        await ValidateDepoAsync(f.TenantId, f.SubeId, r.DepoId, ct);
        if (string.IsNullOrWhiteSpace(r.FaturaNo) || r.FaturaNo.Trim().Length > 50 ||
            r.FaturaTarihi == default || r.Detaylar is null || r.Detaylar.Count == 0 ||
            r.Aciklama?.Length > 500 || (r.GenelIndirimOrani.HasValue && r.GenelIndirimTutari.HasValue))
            throw Error("Fatura bilgileri veya genel indirim gecersiz.");
        if (!await db.Cariler.AnyAsync(x => x.Id == r.CariId && x.TenantId == f.TenantId && x.AktifMi, ct))
            throw Error("Ayni tenant icinde aktif cari secilmelidir.");
        var currency = Currency(r.ParaBirimKodu);
        var rate = Rate(currency, r.Kur);
        var eski = f.Detaylar.Where(x => !x.SilindiMi).ToDictionary(x => x.Id);
        var lines = new List<AlisFaturaDetay>();
        var used = new HashSet<long>();
        foreach (var item in r.Detaylar)
        {
            if (item.Miktar <= 0 || item.BirimFiyat < 0 ||
                decimal.Round(item.Miktar, 4) != item.Miktar || decimal.Round(item.BirimFiyat, 4) != item.BirimFiyat ||
                (item.IndirimOrani.HasValue && item.IndirimTutari.HasValue))
                throw Error("Satir bilgileri veya indirim gecersiz.");
            AlisFaturaDetay line;
            if (item.Id.HasValue)
            {
                if (!used.Add(item.Id.Value) || !eski.TryGetValue(item.Id.Value, out var old))
                    throw Error("Satir bu faturaya ait degil.");
                if (old.StokKartId != item.StokKartId || old.StokKartSatisBirimiId != item.StokKartSatisBirimiId || old.StokKartVaryantId != item.StokKartVaryantId)
                    throw Error("Stok veya birim degisikligi icin yeni satir ekleyin.");
                line = old;
            }
            else
            {
                var stok = await db.StokKartler.SingleOrDefaultAsync(x => x.Id == item.StokKartId && x.TenantId == f.TenantId && x.AktifMi, ct)
                    ?? throw Error("Aktif stok karti bulunamadi.");
                var kdv = await db.Kdvler.SingleOrDefaultAsync(x => x.Id == stok.KdvId && x.TenantId == f.TenantId && x.AktifMi, ct)
                    ?? throw Error("Aktif KDV bulunamadi.");
                var birim = await db.StokKartSatisBirimleri.SingleOrDefaultAsync(x => x.Id == item.StokKartSatisBirimiId &&
                    x.StokKartId == stok.Id && x.TenantId == f.TenantId && x.AktifMi, ct) ?? throw Error("Satis birimi bulunamadi.");
                if (item.StokKartVaryantId.HasValue && !await db.StokKartVaryantlari.AnyAsync(x =>
                    x.Id == item.StokKartVaryantId && x.StokKartId == stok.Id && x.TenantId == f.TenantId && x.AktifMi, ct))
                    throw Error("Varyant bulunamadi.");
                line = new AlisFaturaDetay { TenantId = f.TenantId, SubeId = f.SubeId, StokKartId = stok.Id,
                    StokKartVaryantId = item.StokKartVaryantId, StokKartSatisBirimiId = birim.Id,
                    BirimKodu = birim.BirimKodu, BirimAdi = birim.BirimAdi, Katsayi = birim.Katsayi,
                    KdvId = kdv.Id, KdvOrani = kdv.Oran, KdvDahilMi = f.KdvDahilMi };
            }
            line.Miktar = item.Miktar;
            line.BirimFiyat = item.BirimFiyat;
            line.FiyatParaBirimKodu = Currency(item.FiyatParaBirimKodu);
            line.FiyatKur = Rate(line.FiyatParaBirimKodu, item.FiyatKur);
            // Mixed-currency line conversion belongs to a later task.
            if (line.FiyatParaBirimKodu != currency || line.FiyatKur != rate)
                throw Error("Satir ve fatura para birimi/kur bilgileri ayni olmalidir.");
            line.IndirimOrani = item.IndirimOrani;
            line.IndirimTutari = item.IndirimTutari ?? 0m;
            lines.Add(line);
        }
        var sonuc = vergi.Hesapla(lines.Select(x => new VergiSatir(x.Miktar, x.BirimFiyat, x.KdvOrani,
            x.KdvDahilMi, x.IndirimOrani, x.IndirimTutari)).ToList(), r.GenelIndirimOrani, r.GenelIndirimTutari ?? 0m);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i]; var s = sonuc.Satirlar[i];
            line.SatirAraToplam = s.AraToplam; line.IndirimTutari = s.IndirimTutari; line.GenelIndirimPayi = s.GenelIndirimPayi;
            line.Matrah = s.Matrah; line.KdvTutari = s.KdvTutari; line.SatirNetToplam = s.NetToplam;
            if (line.Id == 0) f.Detaylar.Add(line);
        }
        foreach (var old in eski.Values.Where(x => !used.Contains(x.Id))) db.AlisFaturaDetaylari.Remove(old);
        f.DepoId = r.DepoId;
        f.CariId = r.CariId; f.FaturaNo = r.FaturaNo.Trim(); f.FaturaTarihi = r.FaturaTarihi;
        f.Aciklama = r.Aciklama?.Trim(); f.ParaBirimKodu = currency; f.Kur = rate;
        f.GenelIndirimOrani = r.GenelIndirimOrani; f.GenelIndirimTutari = sonuc.GenelIndirimTutari;
        f.AraToplam = sonuc.AraToplam; f.ToplamMatrah = sonuc.ToplamMatrah;
        f.ToplamKdv = sonuc.ToplamKdv; f.NetToplam = sonuc.NetToplam;
    }
    public async Task<AlisFaturaDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default) => Map(await FindAsync(id, subeId, ct));
    public async Task DeleteAsync(long id, long subeId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockInvoiceAsync(id, subeId, ct);
        var f = await FindAsync(id, subeId, ct);
        EnsureDraft(f);
        db.AlisFaturaDetaylari.RemoveRange(f.Detaylar.Where(x => !x.SilindiMi));
        db.AlisFaturalar.Remove(f);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
    public async Task<AlisFaturaDto> KesinlestirAsync(long id, long subeId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockInvoiceAsync(id, subeId, ct);
        var f = await FindAsync(id, subeId, ct);
        var hasStock = await db.StokFisleri.IgnoreQueryFilters().AnyAsync(x => x.AlisFaturaId == f.Id, ct);
        if (f.Durum != AlisFaturaDurumu.Kesinlesti)
        {
            EnsureDraft(f);
            if (hasStock) throw Error("Bu faturanin stok fisi zaten mevcut; tekrar stok uretilemez.");
            if (!f.Detaylar.Any(x => !x.SilindiMi)) throw Error("Detaysiz fatura kesinlestirilemez.");
            await CreatePurchaseStockAsync(f, ct);
            f.Durum = AlisFaturaDurumu.Kesinlesti;
            await db.SaveChangesAsync(ct);
        }
        else if (!hasStock)
            throw Error("Kesinlesmis faturanin stok fisi eksik; otomatik tekrar stok uretilemez.");
        await tx.CommitAsync(ct);
        return Map(f);
    }
    public async Task<AlisFaturaDto> IptalAsync(long id, long subeId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockInvoiceAsync(id, subeId, ct);
        var f = await FindAsync(id, subeId, ct);
        if (await db.StokFisleri.IgnoreQueryFilters().AnyAsync(x => x.AlisFaturaId == f.Id, ct))
            throw new UygulamaHatasi(409, "Stoklanmis fatura iptal edilemez",
                "Stok girisi yapilmis alis faturasi ters hareket olmadan iptal edilemez.", "purchase_stock_reversal_required");
        // TODO: Purchase returns/reversals require a separate domain operation.
        f.Durum = AlisFaturaDurumu.Iptal;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Map(f);
    }
    public async Task<SayfaliSonucDto<AlisFaturaListeDto>> GetPagedAsync(AlisFaturaFiltre r, CancellationToken ct = default)
    {
        if (r.Page < 1 || r.PageSize < 1 || r.PageSize > 500) throw Error("Sayfalama gecersiz.");
        var tenant = await TenantAsync(r.SubeId, ct);
        const string where = @" FROM AlisFatura f JOIN Cari c ON c.Id=f.CariId AND c.TenantId=f.TenantId
WHERE f.TenantId=@TenantId AND f.SubeId=@SubeId AND f.SilindiMi=0
AND (@CariId IS NULL OR f.CariId=@CariId) AND (@Durum IS NULL OR f.Durum=@Durum)
AND (@Baslangic IS NULL OR f.FaturaTarihi>=@Baslangic) AND (@Bitis IS NULL OR f.FaturaTarihi<=@Bitis)
AND (@Search IS NULL OR f.FaturaNo LIKE @Search OR c.CariKodu LIKE @Search OR c.Ad LIKE @Search)";
        var args = new { TenantId = tenant, r.SubeId, r.CariId, Durum = (int?)r.Durum,
            Baslangic = r.BaslangicTarihi, Bitis = r.BitisTarihi, Search = string.IsNullOrWhiteSpace(r.Arama) ? null : "%" + r.Arama.Trim() + "%",
            Skip = (long)(r.Page - 1) * r.PageSize, Take = r.PageSize };
        var paging = (db.Database.ProviderName ?? "").Contains("Sqlite") ? " LIMIT @Take OFFSET @Skip" : " OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
        var conn = db.Database.GetDbConnection();
        var count = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT COUNT(1)" + where, args, cancellationToken: ct));
        var rows = await conn.QueryAsync<AlisFaturaListeDto>(new CommandDefinition(
            "SELECT f.Id, f.DepoId, f.CariId, c.Ad AS CariAd, f.FaturaNo, f.FaturaTarihi, f.ParaBirimKodu, f.Kur, f.AraToplam, f.GenelIndirimOrani, f.GenelIndirimTutari, f.ToplamMatrah, f.ToplamKdv, f.NetToplam, f.Durum" +
            where + " ORDER BY f.Id DESC" + paging, args, cancellationToken: ct));
        return new() { Kayitlar = rows.ToList(), ToplamKayit = count, Sayfa = r.Page, SayfaBoyutu = r.PageSize };
    }
    private async Task<Guid> TenantAsync(long subeId, CancellationToken ct) =>
        (await db.Subeler.SingleOrDefaultAsync(x => x.Id == subeId && x.AktifMi, ct) ?? throw Error("Sube bulunamadi.")).TenantId;
    private async Task<AlisFatura> FindAsync(long id, long subeId, CancellationToken ct)
    {
        var tenant = await TenantAsync(subeId, ct);
        return await db.AlisFaturalar.IgnoreQueryFilters().Include(x => x.Cari).Include(x => x.Detaylar.Where(d => !d.SilindiMi))
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenant && x.SubeId == subeId && !x.SilindiMi, ct)
            ?? throw new UygulamaHatasi(404, "Alis faturasi bulunamadi", "Alis faturasi bulunamadi.", "purchase_not_found");
    }
    private static void EnsureDraft(AlisFatura f) { if (f.Durum != AlisFaturaDurumu.Taslak) throw new UygulamaHatasi(409, "Belge kilitli", "Yalniz taslak belge duzenlenebilir.", "purchase_not_draft"); }
    private static string Currency(string value)
    {
        var kod = value?.Trim().ToUpperInvariant() ?? "";
        if (kod.Length is < 1 or > 10) throw Error("Para birimi gecersiz.");
        return kod;
    }
    private static decimal Rate(string currency, decimal value)
    {
        if (value <= 0 || decimal.Round(value, 6) != value) throw Error("Kur gecersiz.");
        return currency == "TRY" ? 1m : value;
    }
    private static UygulamaHatasi Error(string message) => new(400, "Alis faturasi hatasi", message, "purchase_invalid");
    private static AlisFaturaDto Map(AlisFatura f) => new()
    {
        Id = f.Id, DepoId = f.DepoId, CariId = f.CariId, CariAd = f.Cari.Ad, FaturaNo = f.FaturaNo, FaturaTarihi = f.FaturaTarihi,
        ParaBirimKodu = f.ParaBirimKodu, Kur = f.Kur, KdvDahilMi = f.KdvDahilMi, Aciklama = f.Aciklama,
        AraToplam = f.AraToplam, GenelIndirimOrani = f.GenelIndirimOrani, GenelIndirimTutari = f.GenelIndirimTutari,
        ToplamMatrah = f.ToplamMatrah, ToplamKdv = f.ToplamKdv, NetToplam = f.NetToplam, Durum = f.Durum,
        Detaylar = f.Detaylar.Where(x => !x.SilindiMi).OrderBy(x => x.Id).Select(x => new AlisFaturaDetayDto {
            Id = x.Id, StokKartId = x.StokKartId, StokKartVaryantId = x.StokKartVaryantId, StokKartSatisBirimiId = x.StokKartSatisBirimiId,
            BirimKodu = x.BirimKodu, BirimAdi = x.BirimAdi, Katsayi = x.Katsayi, Miktar = x.Miktar, BirimFiyat = x.BirimFiyat,
            FiyatParaBirimKodu = x.FiyatParaBirimKodu, FiyatKur = x.FiyatKur, SatirAraToplam = x.SatirAraToplam,
            IndirimOrani = x.IndirimOrani, IndirimTutari = x.IndirimTutari, GenelIndirimPayi = x.GenelIndirimPayi,
            KdvId = x.KdvId, KdvOrani = x.KdvOrani, KdvDahilMi = x.KdvDahilMi, Matrah = x.Matrah, KdvTutari = x.KdvTutari, SatirNetToplam = x.SatirNetToplam
        }).ToList()
    };
}
