using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Product;

namespace PanoPos.Infrastructure.Order;

public sealed partial class SiparisServisi
{
    public async Task<SiparisDto> HizliSatisKaydetAsync(long? id, HizliSatisKaydetRequestDto request, CancellationToken cancellationToken = default)
    {
        var context = _dbContext.Baglam() ?? throw IslemBaglami.Yetkisiz();
        if (request.Satirlar == null || request.Satirlar.Count is < 1 or > 500 || request.Satirlar.Any(x => x == null))
            throw PosHata("cart_invalid", "Sepet 1-500 satir icermelidir.");
        var currency = request.BelgeParaBirimKodu?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(currency) || currency.Length > 10 || request.Aciklama?.Length > 500)
            throw PosHata("cart_invalid", "Para birimi veya aciklama gecersiz.");
        if (request.Kur <= 0 || request.Kur > 999999999999.999999m || decimal.Round(request.Kur, 6) != request.Kur)
            throw PosHata("currency_rate_invalid", "Belge kuru pozitif ve en fazla alti ondalikli olmalidir.");
        SiparisGenelIndirimKontrolu(request.GenelIndirimOrani, request.GenelIndirimTutari);
        if (!await _dbContext.FiyatTipleri.TenantKapsami(_dbContext).AnyAsync(x => x.Id == request.FiyatTipiId && x.AktifMi, cancellationToken))
            throw PosHata("price_type_not_found", "Aktif fiyat tipi bulunamadi.", 404);
        if (request.CariId.HasValue && !await _dbContext.CariKartlar.SubeKapsami(_dbContext)
                .AnyAsync(x => x.Id == request.CariId && x.AktifMi, cancellationToken))
            throw PosHata("cari_kart_not_found", "Aktif cari bulunamadi.", 404);
        var ids = request.Satirlar.Where(x => x.SiparisDetayId.HasValue).Select(x => x.SiparisDetayId!.Value).ToList();
        if (ids.Any(x => x <= 0) || ids.Distinct().Count() != ids.Count || (!id.HasValue && ids.Count != 0))
            throw PosHata("cart_line_invalid", "Sepet satir kimlikleri gecersiz.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            Siparis order;
            if (id.HasValue)
            {
                if (_dbContext.Database.IsSqlServer())
                    await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Siparis WITH (UPDLOCK,HOLDLOCK) WHERE Id={id.Value} AND TenantId={context.TenantId} AND SubeId={context.SubeId}", cancellationToken);
                order = await _dbContext.Siparisler.SubeKapsami(_dbContext).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    ?? throw PosHata("siparis_not_found", "Bekleyen satis bulunamadi.", 404);
                await _dbContext.Entry(order).ReloadAsync(cancellationToken);
                BekleyenKontrol(order);
                if (!request.Surum.HasValue || request.Surum != order.Surum)
                    throw PosHata("cart_version_conflict", "Satis baska bir islemle degisti. Yeniden aciniz.", 409);
            }
            else
            {
                var created = await SiparisOlusturAsync(new() { SubeId = context.SubeId,
                    SiparisTipi = SiparisTipi.HizliSatisBekleyen, ParaBirimKodu = currency, Kur = request.Kur,
                    CariId = request.CariId, Aciklama = request.Aciklama }, cancellationToken);
                order = await _dbContext.Siparisler.SingleAsync(x => x.Id == created.Id, cancellationToken);
            }
            var existing = await _dbContext.SiparisDetaylari.SubeKapsami(_dbContext)
                .Where(x => x.SiparisId == order.Id && x.AktifMi).ToListAsync(cancellationToken);
            if (ids.Except(existing.Select(x => x.Id)).Any())
                throw PosHata("cart_line_invalid", "Satir bu bekleyen satisa ait degil.");
            order.CariId = request.CariId;
            order.FiyatTipiId = request.FiyatTipiId;
            order.ParaBirimKodu = currency;
            order.Kur = currency == "TRY" ? 1m : request.Kur;
            order.Aciklama = NormalizeOptional(request.Aciklama);
            order.GenelIndirimOrani = request.GenelIndirimOrani;
            order.GenelIndirimTutari = request.GenelIndirimTutari ?? 0;
            order.Surum = Guid.NewGuid();
            var resolver = new SatisStokCozumServisi(_dbContext);
            var prices = new Dictionary<(long, long, long?), SatisStokDto>();
            var final = new List<SiparisDetay>();
            foreach (var input in request.Satirlar)
            {
                if (input.Miktar <= 0 || input.Miktar > 999999999999999.999m || decimal.Round(input.Miktar, 3) != input.Miktar)
                    throw PosHata("siparis_line_invalid", "Miktar pozitif ve en fazla uc ondalikli olmalidir.");
                SiparisSatirIndirimKontrolu(input.IndirimOrani, input.IndirimTutari);
                var key = (input.StokKartSatisBirimiId, input.FiyatTipiId ?? request.FiyatTipiId, input.StokKartVaryantId);
                if (!prices.TryGetValue(key, out var price))
                    prices[key] = price = await resolver.CozAsync(context.TenantId, key.Item1, key.Item2, key.Item3, cancellationToken);
                var converted = SatisStokCozumServisi.BelgeFiyati(price, currency, input.FiyatKur);
                var line = input.SiparisDetayId.HasValue ? existing.Single(x => x.Id == input.SiparisDetayId) : new SiparisDetay {
                    TenantId = context.TenantId, SubeId = context.SubeId, SiparisId = order.Id };
                line.StokKartId = price.StokKartId; line.StokKartVaryantId = input.StokKartVaryantId;
                line.StokKartSatisBirimiId = price.StokKartSatisBirimiId;
                line.BirimKodu = price.BirimKodu; line.BirimAdi = price.BirimAdi; line.BirimKatsayi = price.Katsayi;
                line.FiyatTipiId = price.FiyatTipiId; line.FiyatTipiAdi = price.FiyatTipiAdi;
                line.BirimFiyat = converted.BirimFiyat; line.FiyatKur = converted.Kur;
                line.FiyatParaBirimKodu = price.FiyatParaBirimKodu;
                line.KdvId = price.KdvId; line.KdvOrani = price.KdvOrani; line.KdvDahilMi = order.KdvDahilMi;
                line.Miktar = input.Miktar; line.IndirimOrani = input.IndirimOrani; line.IndirimTutari = input.IndirimTutari ?? 0;
                if (!input.SiparisDetayId.HasValue) _dbContext.SiparisDetaylari.Add(line);
                final.Add(line);
            }
            foreach (var removed in existing.Where(x => !ids.Contains(x.Id)))
                removed.SoftDelete(context.KullaniciId, DateTime.UtcNow);
            SiparisToplamlariniHesapla(order, final);
            await _dbContext.SaveChangesAsync(cancellationToken);
            // Query before commit: the returned version and totals belong to this write.
            var result = await SiparisGetirAsync(order.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw PosHata("cart_version_conflict", "Satis baska bir islemle degisti. Yeniden aciniz.", 409);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<SiparisDto> BekleyenHizliSatisGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        _ = _dbContext.Baglam() ?? throw IslemBaglami.Yetkisiz();
        var result = await SiparisGetirAsync(id, cancellationToken);
        if (result.SiparisTipi != SiparisTipi.HizliSatisBekleyen || result.AdisyonId.HasValue ||
            result.Durum != SiparisDurumu.Bekliyor || !result.AktifMi)
            throw PosHata("siparis_not_editable", "Yalniz bekleyen hizli satis acilabilir.", 409);
        return result;
    }

    public async Task<SayfaliSonucDto<BekleyenHizliSatisDto>> BekleyenHizliSatisListeleAsync(string? arama, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var context = _dbContext.Baglam() ?? throw IslemBaglami.Yetkisiz();
        if (page <= 0 || pageSize is < 1 or > 200 || (long)(page - 1) * pageSize > int.MaxValue || arama?.Length > 500)
            throw PosHata("pagination_invalid", "Sayfa ve arama degerleri gecersiz.");
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        const string from = @" FROM Siparis s
LEFT JOIN CariKart c ON c.Id=s.CariId AND c.TenantId=s.TenantId AND c.SubeId=s.SubeId AND c.SilindiMi=0
WHERE s.TenantId=@TenantId AND s.SubeId=@SubeId AND s.SilindiMi=0 AND s.AktifMi=1
AND s.SiparisTipi=@Tip AND s.Durum=@Durum AND s.AdisyonId IS NULL
AND (@Arama IS NULL OR s.SiparisNo LIKE @Arama OR s.Aciklama LIKE @Arama OR c.Ad LIKE @Arama)";
        var args = new { context.TenantId, context.SubeId, Tip = (int)SiparisTipi.HizliSatisBekleyen,
            Durum = (int)SiparisDurumu.Bekliyor, Arama = string.IsNullOrWhiteSpace(arama) ? null : "%" + arama.Trim() + "%",
            Skip = (page - 1) * pageSize, Take = pageSize };
        var sql = @"SELECT s.Id,s.SiparisNo,s.OlusturmaTarihi AS Tarih,s.CariId,c.Ad AS CariAdi,
s.NetToplam,s.ParaBirimKodu,s.Aciklama,
(SELECT COUNT(1) FROM SiparisDetay d WHERE d.SiparisId=s.Id AND d.TenantId=s.TenantId AND d.SubeId=s.SubeId AND d.AktifMi=1 AND d.SilindiMi=0) AS SatirSayisi" + from +
            " ORDER BY s.Id DESC " + (_dbContext.Database.ProviderName?.Contains("Sqlite") == true ? "LIMIT @Take OFFSET @Skip" : "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY");
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT COUNT(1)" + from, args, cancellationToken: cancellationToken));
        var rows = await connection.QueryAsync<BekleyenHizliSatisDto>(new CommandDefinition(sql, args, cancellationToken: cancellationToken));
        return new() { Sayfa = page, SayfaBoyutu = pageSize, ToplamKayit = total, Kayitlar = rows.ToList() };
    }

    private static void BekleyenKontrol(Siparis order)
    {
        if (order.SiparisTipi != SiparisTipi.HizliSatisBekleyen || order.AdisyonId.HasValue ||
            order.Durum != SiparisDurumu.Bekliyor || !order.AktifMi || order.SilindiMi)
            throw PosHata("siparis_not_editable", "Yalniz bekleyen hizli satis duzenlenebilir.", 409);
    }
    private static UygulamaHatasi PosHata(string code, string message, int status = 400)
        => new(status, "Hizli satis islemi yapilamadi", message, code);
}
