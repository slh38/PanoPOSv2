using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Invoice;

public sealed class FaturaServisi : IFaturaServisi
{
    private readonly PanoPosDbContext _dbContext;
    private readonly IOutboxServisi _outboxServisi;

    public FaturaServisi(PanoPosDbContext dbContext)
        : this(dbContext, new BosOutboxServisi())
    {
    }

    public FaturaServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi)
    {
        _dbContext = dbContext;
        _outboxServisi = outboxServisi;
    }

    public async Task<FaturaDto> SiparistenFaturaOlusturAsync(SiparistenFaturaOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SiparisId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SiparisId zorunludur.", "siparis_required");
        }

        var siparis = await _dbContext.Siparisler
            .Include(x => x.Detaylar.Where(y => y.AktifMi))
            .SingleOrDefaultAsync(x => x.Id == request.SiparisId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum != SiparisDurumu.Bekliyor)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Sadece bekleyen siparisten fatura olusturulabilir.", "siparis_not_invoiceable");
        }

        if (siparis.Detaylar.Count == 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Detaysiz siparisten fatura olusturulamaz.", "siparis_has_no_lines");
        }

        FaturaKaynakSiparisKontrolu(siparis);

        var mevcutFaturaVar = await _dbContext.Faturalar.AnyAsync(x => x.SiparisId == siparis.Id && x.Durum != FaturaDurumu.Iptal, cancellationToken);
        if (mevcutFaturaVar)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Bu siparisten zaten fatura olusturulmus.", "invoice_already_exists");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var fatura = new Fatura
        {
            TenantId = siparis.TenantId,
            SubeId = siparis.SubeId,
            FaturaNo = await FaturaNoUretAsync(siparis.TenantId, cancellationToken),
            SiparisId = siparis.Id,
            CariId = siparis.CariId,
            Aciklama = NormalizeOptional(request.Aciklama) ?? siparis.Aciklama,
            ParaBirimKodu = siparis.ParaBirimKodu,
            Kur = siparis.Kur,
            AraToplam = siparis.AraToplam,
            GenelIndirimOrani = siparis.GenelIndirimOrani,
            GenelIndirimTutari = siparis.GenelIndirimTutari,
            NetToplam = siparis.NetToplam,
            OdenenTutar = 0m,
            KalanTutar = siparis.NetToplam,
            ToplamTutar = siparis.ToplamTutar,
            Durum = FaturaDurumu.Acik,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Faturalar.Add(fatura);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var detay in siparis.Detaylar)
        {
            _dbContext.FaturaDetaylari.Add(new FaturaDetay
            {
                TenantId = detay.TenantId,
                SubeId = detay.SubeId,
                FaturaId = fatura.Id,
                UrunId = detay.UrunId,
                UrunVaryantId = detay.UrunVaryantId,
                Miktar = detay.Miktar,
                BirimFiyat = detay.BirimFiyat,
                SatirAraToplam = detay.SatirAraToplam,
                IndirimOrani = detay.IndirimOrani,
                IndirimTutari = detay.IndirimTutari,
                SatirNetToplam = detay.SatirNetToplam,
                SatirToplam = detay.SatirToplam,
                Aciklama = detay.Aciklama,
                AktifMi = true,
                SilindiMi = false
            });
        }

        siparis.Durum = SiparisDurumu.Tamamlandi;
        siparis.AktifMi = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = fatura.TenantId,
            SubeId = fatura.SubeId,
            CihazId = 1,
            OlayTipi = "FaturaSiparistenOlusturuldu",
            KaynakTablo = nameof(Fatura),
            KaynakId = fatura.Id,
            PayloadJson = JsonSerializer.Serialize(new
            {
                fatura.Id,
                fatura.FaturaNo,
                fatura.SiparisId,
                fatura.CariId,
                fatura.ParaBirimKodu,
                fatura.Kur,
                fatura.NetToplam
            })
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return await FaturaGetirAsync(fatura.Id, cancellationToken);
    }

    public async Task<FaturaDto> FaturaGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var fatura = await _dbContext.Faturalar
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.Urun)
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.UrunVaryant)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");

        return new FaturaDto
        {
            Id = fatura.Id,
            FaturaNo = fatura.FaturaNo,
            SiparisId = fatura.SiparisId,
            CariId = fatura.CariId,
            Aciklama = fatura.Aciklama,
            ParaBirimKodu = fatura.ParaBirimKodu,
            Kur = fatura.Kur,
            AraToplam = fatura.AraToplam,
            GenelIndirimOrani = fatura.GenelIndirimOrani,
            GenelIndirimTutari = fatura.GenelIndirimTutari,
            NetToplam = fatura.NetToplam,
            OdenenTutar = fatura.OdenenTutar,
            KalanTutar = fatura.KalanTutar,
            ToplamTutar = fatura.ToplamTutar,
            Durum = fatura.Durum,
            KapanisTarihi = fatura.KapanisTarihi,
            KapatanKullaniciId = fatura.KapatanKullaniciId,
            AktifMi = fatura.AktifMi,
            Detaylar = fatura.Detaylar.OrderBy(x => x.Id).Select(x => new FaturaDetayDto
            {
                Id = x.Id,
                UrunId = x.UrunId,
                UrunAd = x.Urun.Ad,
                UrunVaryantId = x.UrunVaryantId,
                VaryantKodu = x.UrunVaryant != null ? x.UrunVaryant.VaryantKodu : null,
                Miktar = x.Miktar,
                BirimFiyat = x.BirimFiyat,
                SatirAraToplam = x.SatirAraToplam,
                IndirimOrani = x.IndirimOrani,
                IndirimTutari = x.IndirimTutari,
                SatirNetToplam = x.SatirNetToplam,
                SatirToplam = x.SatirToplam,
                Aciklama = x.Aciklama
            }).ToList()
        };
    }

    public async Task<SayfaliSonucDto<FaturaListeItemDto>> FaturaListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (page <= 0 || pageSize <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var tenantId = await _dbContext.Subeler.Where(x => x.Id == subeId).Select(x => x.TenantId).SingleOrDefaultAsync(cancellationToken);
        if (tenantId == Guid.Empty)
        {
            throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
        }

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var countSql = @"SELECT COUNT(1)
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, FaturaNo, SiparisId, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OdenenTutar, KalanTutar, ToplamTutar, Durum, KapanisTarihi
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, FaturaNo, SiparisId, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OdenenTutar, KalanTutar, ToplamTutar, Durum, KapanisTarihi
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Durum = durum, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<FaturaListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<FaturaListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<FaturaDto> FaturaKapatAsync(long id, FaturaKapatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.KapatanKullaniciId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "KapatanKullaniciId zorunludur.", "closing_user_required");
        }

        var fatura = await _dbContext.Faturalar.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");

        if (fatura.Durum != FaturaDurumu.Acik)
        {
            throw new UygulamaHatasi(409, "Fatura kapatilamadi", "Sadece acik fatura kapatilabilir.", "invoice_not_open");
        }

        fatura.Durum = FaturaDurumu.Kapali;
        fatura.KalanTutar = 0m;
        fatura.OdenenTutar = fatura.NetToplam;
        fatura.KapanisTarihi = DateTime.UtcNow;
        fatura.KapatanKullaniciId = request.KapatanKullaniciId;
        fatura.AktifMi = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await FaturaGetirAsync(id, cancellationToken);
    }

    public async Task<FaturaDto> FaturaIptalAsync(long id, FaturaIptalRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        var fatura = await _dbContext.Faturalar.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");

        if (fatura.Durum == FaturaDurumu.Iptal)
        {
            return await FaturaGetirAsync(id, cancellationToken);
        }

        if (fatura.Durum == FaturaDurumu.Iade)
        {
            throw new UygulamaHatasi(409, "Fatura iptal edilemedi", "Iade durumundaki fatura iptal edilemez.", "invoice_refund_state");
        }

        fatura.Durum = FaturaDurumu.Iptal;
        fatura.AktifMi = false;
        if (!string.IsNullOrWhiteSpace(request?.Aciklama))
        {
            fatura.Aciklama = request!.Aciklama!.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await FaturaGetirAsync(id, cancellationToken);
    }

    private async Task<string> FaturaNoUretAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var bugun = DateTime.UtcNow.ToString("yyyyMMdd");
        var oncekiler = await _dbContext.Faturalar
            .Where(x => x.TenantId == tenantId && x.FaturaNo.StartsWith($"FTR-{bugun}-"))
            .Select(x => x.FaturaNo)
            .ToListAsync(cancellationToken);

        var sonraki = oncekiler
            .Select(x => x.Split('-').LastOrDefault())
            .Select(x => int.TryParse(x, out var sayi) ? sayi : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"FTR-{bugun}-{sonraki:000000}";
    }

    private static void FaturaKaynakSiparisKontrolu(Siparis siparis)
    {
        if (string.IsNullOrWhiteSpace(siparis.ParaBirimKodu))
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis para birimi bos olamaz.", "invoice_currency_required");
        }

        if (siparis.Kur <= 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis kuru 0'dan buyuk olmalidir.", "invoice_currency_rate_invalid");
        }

        if (siparis.NetToplam < 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis net toplam negatif olamaz.", "invoice_net_total_negative");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
