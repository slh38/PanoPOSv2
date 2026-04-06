using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Order;

public sealed class SiparisServisi : ISiparisServisi
{
    private readonly PanoPosDbContext _dbContext;

    public SiparisServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        if (request.SiparisTipi == SiparisTipi.Masa && !request.AdisyonId.HasValue)
        {
            throw new UygulamaHatasi(400, "Gecersiz siparis", "Masa siparisinde AdisyonId zorunludur.", "adisyon_required_for_table_order");
        }

        if (request.AdisyonId.HasValue)
        {
            var adisyonVar = await _dbContext.Adisyonlar.AnyAsync(x => x.Id == request.AdisyonId.Value && x.Durum == AdisyonDurumu.Acik, cancellationToken);
            if (!adisyonVar)
            {
                throw new UygulamaHatasi(404, "Adisyon bulunamadi", "Acik adisyon bulunamadi.", "open_adisyon_not_found");
            }
        }

        if (request.CariId.HasValue)
        {
            var cariVar = await _dbContext.Cariler.AnyAsync(x => x.Id == request.CariId.Value && x.SubeId == request.SubeId, cancellationToken);
            if (!cariVar)
            {
                throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_not_found");
            }
        }

        var siparis = new Siparis
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            SiparisNo = await SiparisNoUretAsync(sube.TenantId, cancellationToken),
            SiparisTipi = request.SiparisTipi,
            AdisyonId = request.AdisyonId,
            CariId = request.CariId,
            Aciklama = NormalizeOptional(request.Aciklama),
            ToplamTutar = 0,
            Durum = SiparisDurumu.Bekliyor,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Siparisler.Add(siparis);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await SiparisGetirAsync(siparis.Id, cancellationToken);
    }

    public async Task<SiparisDto> SiparisSatirEkleAsync(long id, SiparisSatirEkleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.UrunId <= 0 || request.Miktar <= 0 || request.BirimFiyat < 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "UrunId, Miktar ve BirimFiyat gecersiz.", "siparis_line_invalid");
        }

        var siparis = await _dbContext.Siparisler.Include(x => x.Detaylar).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum != SiparisDurumu.Bekliyor)
        {
            throw new UygulamaHatasi(409, "Siparis guncellenemedi", "Sadece bekleyen siparise satir eklenebilir.", "siparis_not_editable");
        }

        var urun = await _dbContext.Urunler.SingleOrDefaultAsync(x => x.Id == request.UrunId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");

        if (request.UrunVaryantId.HasValue)
        {
            var varyantVar = await _dbContext.UrunVaryantlari.AnyAsync(x => x.Id == request.UrunVaryantId.Value && x.UrunId == request.UrunId, cancellationToken);
            if (!varyantVar)
            {
                throw new UygulamaHatasi(404, "Varyant bulunamadi", "Varyant bulunamadi.", "variant_not_found");
            }
        }

        var satirToplam = request.Miktar * request.BirimFiyat;
        var detay = new SiparisDetay
        {
            TenantId = siparis.TenantId,
            SubeId = siparis.SubeId,
            SiparisId = siparis.Id,
            UrunId = request.UrunId,
            UrunVaryantId = request.UrunVaryantId,
            Miktar = request.Miktar,
            BirimFiyat = request.BirimFiyat,
            SatirToplam = satirToplam,
            Aciklama = NormalizeOptional(request.Aciklama),
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.SiparisDetaylari.Add(detay);
        await _dbContext.SaveChangesAsync(cancellationToken);

        siparis.ToplamTutar = (await _dbContext.SiparisDetaylari
            .Where(x => x.SiparisId == siparis.Id)
            .Select(x => x.SatirToplam)
            .ToListAsync(cancellationToken))
            .Sum();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await SiparisGetirAsync(siparis.Id, cancellationToken);
    }

    public async Task<SiparisDto> SiparisGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var siparis = await _dbContext.Siparisler
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.Urun)
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.UrunVaryant)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        return new SiparisDto
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            SiparisTipi = siparis.SiparisTipi,
            AdisyonId = siparis.AdisyonId,
            CariId = siparis.CariId,
            Aciklama = siparis.Aciklama,
            ToplamTutar = siparis.ToplamTutar,
            Durum = siparis.Durum,
            AktifMi = siparis.AktifMi,
            Detaylar = siparis.Detaylar.OrderBy(x => x.Id).Select(x => new SiparisDetayDto
            {
                Id = x.Id,
                UrunId = x.UrunId,
                UrunAd = x.Urun.Ad,
                UrunVaryantId = x.UrunVaryantId,
                VaryantKodu = x.UrunVaryant != null ? x.UrunVaryant.VaryantKodu : null,
                Miktar = x.Miktar,
                BirimFiyat = x.BirimFiyat,
                SatirToplam = x.SatirToplam,
                Aciklama = x.Aciklama
            }).ToList()
        };
    }

    public async Task<SayfaliSonucDto<SiparisListeItemDto>> SiparisListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default)
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
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, SiparisNo, SiparisTipi, AdisyonId, ToplamTutar, Durum
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, SiparisNo, SiparisTipi, AdisyonId, ToplamTutar, Durum
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Durum = durum, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<SiparisListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<SiparisListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<SiparisDto> SiparisIptalAsync(long id, SiparisIptalRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        var siparis = await _dbContext.Siparisler.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum == SiparisDurumu.Iptal)
        {
            return await SiparisGetirAsync(id, cancellationToken);
        }

        if (siparis.Durum == SiparisDurumu.Tamamlandi)
        {
            throw new UygulamaHatasi(409, "Siparis iptal edilemedi", "Tamamlanmis siparis iptal edilemez.", "siparis_completed");
        }

        siparis.Durum = SiparisDurumu.Iptal;
        siparis.AktifMi = false;
        if (!string.IsNullOrWhiteSpace(request?.Aciklama))
        {
            siparis.Aciklama = request!.Aciklama!.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await SiparisGetirAsync(id, cancellationToken);
    }

    private async Task<string> SiparisNoUretAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var bugun = DateTime.UtcNow.ToString("yyyyMMdd");
        var oncekiSayac = await _dbContext.Siparisler
            .Where(x => x.TenantId == tenantId && x.SiparisNo.StartsWith($"SIP-{bugun}-"))
            .Select(x => x.SiparisNo)
            .ToListAsync(cancellationToken);

        var sonraki = oncekiSayac
            .Select(x => x.Split('-').LastOrDefault())
            .Select(x => int.TryParse(x, out var sayi) ? sayi : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"SIP-{bugun}-{sonraki:000000}";
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

