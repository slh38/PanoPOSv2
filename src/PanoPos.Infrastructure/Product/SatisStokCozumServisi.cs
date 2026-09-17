using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class SatisStokCozumServisi(PanoPosDbContext db) : ISatisStokCozumServisi
{
    private Guid Tenant => db.Baglam()?.TenantId ?? throw IslemBaglami.Yetkisiz();

    public async Task<SatisStokDto> BarkodCozAsync(string barkodNo, long fiyatTipiId, CancellationToken ct = default)
    {
        var tenant = Tenant;
        var barkod = await db.Barkodlar.AsNoTracking().Where(x => x.TenantId == tenant && x.AktifMi &&
            x.BarkodNo == barkodNo.Trim()).Select(x => new { x.BarkodNo, x.StokKartId, x.StokKartVaryantId, x.StokKartSatisBirimiId }).SingleOrDefaultAsync(ct)
            ?? throw Hata("barkod_not_found", "Aktif barkod bulunamadi.", 404);
        if (!barkod.StokKartSatisBirimiId.HasValue)
            throw Hata("sales_unit_required", "Barkodun satis birimi tanimlanmamis.");
        if (barkod.StokKartId.HasValue == barkod.StokKartVaryantId.HasValue)
            throw Hata("barcode_relation_invalid", "Barkod stok baglantisi gecersiz.");
        var result = await CozAsync(tenant, barkod.StokKartSatisBirimiId.Value, fiyatTipiId, barkod.StokKartVaryantId, ct);
        if (barkod.StokKartId.HasValue && barkod.StokKartId != result.StokKartId)
            throw Hata("sales_unit_invalid", "Barkodun satis birimi stok kartiyla uyusmuyor.");
        result.BarkodNo = barkod.BarkodNo;
        return result;
    }

    public Task<SatisStokDto> FiyatCozAsync(long satisBirimiId, long fiyatTipiId, long? varyantId = null, CancellationToken ct = default)
        => CozAsync(Tenant, satisBirimiId, fiyatTipiId, varyantId, ct);

    // The order supplies its already scoped document tenant, never a client tenant.
    internal async Task<SatisStokDto> CozAsync(Guid tenant, long birimId, long fiyatTipiId, long? varyantId, CancellationToken ct)
    {
        var context = db.Baglam();
        if (context != null && context.TenantId != tenant) throw IslemBaglami.Yetkisiz();
        if (fiyatTipiId <= 0) throw Hata("price_type_required", "FiyatTipiId zorunludur.");
        var query = from birim in db.StokKartSatisBirimleri.AsNoTracking()
                    join stok in db.StokKartler on birim.StokKartId equals stok.Id
                    join kdv in db.Kdvler on stok.KdvId equals kdv.Id
                    where birim.Id == birimId && birim.TenantId == tenant && birim.AktifMi &&
                        stok.TenantId == tenant && stok.AktifMi && kdv.TenantId == tenant && kdv.AktifMi
                    select new SatisStokDto {
                        StokKartId = stok.Id, StokKartAdi = stok.Ad, StokKartSatisBirimiId = birim.Id,
                        BirimKodu = birim.BirimKodu, BirimAdi = birim.BirimAdi, Katsayi = birim.Katsayi,
                        KdvId = kdv.Id, KdvOrani = kdv.Oran
                    };
        var result = await query.SingleOrDefaultAsync(ct)
            ?? throw Hata("sales_unit_invalid", "Aktif stok karti, satis birimi veya KDV bulunamadi.");
        if (result.Katsayi <= 0) throw Hata("sales_unit_invalid", "Satis birimi katsayisi gecersiz.");
        if (varyantId.HasValue)
        {
            result.VaryantAdi = await db.StokKartVaryantlari.AsNoTracking().Where(x => x.Id == varyantId &&
                x.StokKartId == result.StokKartId && x.TenantId == tenant && x.AktifMi).Select(x => x.VaryantKodu).SingleOrDefaultAsync(ct)
                ?? throw Hata("variant_not_found", "Aktif varyant bulunamadi.", 404);
            result.StokKartVaryantId = varyantId;
        }
        var price = await (from fiyat in db.StokKartFiyatlari.AsNoTracking()
                           join tip in db.FiyatTipleri on fiyat.FiyatTipiId equals tip.Id
                           where fiyat.TenantId == tenant && fiyat.AktifMi && fiyat.StokKartSatisBirimiId == birimId &&
                               tip.TenantId == tenant && tip.AktifMi && tip.Id == fiyatTipiId
                           select new { fiyat.Fiyat, fiyat.ParaBirimKodu, tip.Ad }).SingleOrDefaultAsync(ct)
            ?? throw Hata("price_not_found", "Secilen satis birimi ve fiyat tipi icin aktif fiyat bulunamadi.", 404);
        result.FiyatTipiId = fiyatTipiId;
        result.FiyatTipiAdi = price.Ad;
        result.Fiyat = price.Fiyat;
        result.FiyatParaBirimKodu = ParaBirimi(price.ParaBirimKodu);
        if (price.Fiyat < 0 || price.Fiyat > 99999999999999.9999m || decimal.Round(price.Fiyat, 4) != price.Fiyat)
            throw Hata("price_invalid", "Kayitli fiyat hassasiyeti veya tutari gecersiz.");
        return result;
    }

    internal static (decimal BirimFiyat, decimal Kur) BelgeFiyati(SatisStokDto fiyat, string belgeParaBirimi, decimal? kur)
    {
        var oran = fiyat.FiyatParaBirimKodu == ParaBirimi(belgeParaBirimi) ? 1m : kur;
        if (!oran.HasValue || oran <= 0 || oran > 999999999999.999999m || decimal.Round(oran.Value, 6) != oran)
            throw Hata("price_currency_invalid", "Farkli para biriminde pozitif, en fazla alti ondalikli satis kuru zorunludur.");
        decimal tutar;
        try { tutar = decimal.Round(fiyat.Fiyat * oran.Value, 4, MidpointRounding.AwayFromZero); }
        catch (OverflowException) { throw Hata("price_invalid", "Donusturulmus fiyat tutari cok buyuk."); }
        if (tutar > 99999999999999.9999m) throw Hata("price_invalid", "Donusturulmus fiyat tutari cok buyuk.");
        return (tutar, oran.Value);
    }

    private static string ParaBirimi(string kod)
    {
        var result = kod.Trim().ToUpperInvariant();
        if (result.Length is < 1 or > 10) throw Hata("price_currency_invalid", "Para birimi gecersiz.");
        return result;
    }

    public async Task<SayfaliSonucDto<FiyatTipiListeDto>> FiyatTipleriAsync(int page, int pageSize, CancellationToken ct = default)
    {
        SayfaKontrol(page, pageSize);
        var tenant = Tenant;
        var query = db.FiyatTipleri.AsNoTracking().Where(x => x.TenantId == tenant && x.AktifMi);
        return new() { Sayfa = page, SayfaBoyutu = pageSize, ToplamKayit = await query.CountAsync(ct),
            Kayitlar = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new FiyatTipiListeDto(x.Id, x.Kod, x.Ad)).ToListAsync(ct) };
    }

    public async Task<SayfaliSonucDto<SatisBirimiListeDto>> SatisBirimleriAsync(long stokKartId, int page, int pageSize, CancellationToken ct = default)
    {
        SayfaKontrol(page, pageSize);
        var tenant = Tenant;
        if (!await db.StokKartler.AnyAsync(x => x.Id == stokKartId && x.TenantId == tenant && x.AktifMi, ct))
            throw Hata("stok_kart_not_found", "Aktif stok karti bulunamadi.", 404);
        var query = db.StokKartSatisBirimleri.AsNoTracking().Where(x => x.StokKartId == stokKartId && x.TenantId == tenant && x.AktifMi);
        return new() { Sayfa = page, SayfaBoyutu = pageSize, ToplamKayit = await query.CountAsync(ct),
            Kayitlar = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SatisBirimiListeDto(x.Id, x.BirimKodu, x.BirimAdi, x.Katsayi, x.VarsayilanMi)).ToListAsync(ct) };
    }

    private static void SayfaKontrol(int page, int size)
    {
        if (page <= 0 || size is < 1 or > 200 || (long)(page - 1) * size > int.MaxValue)
            throw Hata("pagination_invalid", "Gecerli sayfa ve 1-200 arasi sayfa boyutu gereklidir.");
    }
    private static UygulamaHatasi Hata(string code, string message, int status = 400) => new(status, "Satis bilgisi cozumlenemedi", message, code);
}
