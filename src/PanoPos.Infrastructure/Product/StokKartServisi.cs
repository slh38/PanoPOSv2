using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class StokKartServisi : IStokKartServisi
{
    private readonly PanoPosDbContext _dbContext;

    public StokKartServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StokKartDto> StokKartOlusturAsync(StokKartOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKart adi bos olamaz.", "stok_kart_ad_required");
        }

        var sube = await _dbContext.Subeler.YetkiliSube(_dbContext).SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        await StokKartKoduTekrarKontroluAsync(sube.TenantId, request.StokKartKodu, null, cancellationToken);
        await KdvKontroluAsync(request.KdvId, sube.TenantId, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.StokKategoriId, request.StokGrupId, cancellationToken);

        var stokKart = new StokKart
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            StokKartKodu = NormalizeOptional(request.StokKartKodu),
            Ad = request.Ad.Trim(),
            Aciklama = NormalizeOptional(request.Aciklama),
            StokKartTipi = request.StokKartTipi,
            StokKategoriId = request.StokKategoriId,
            StokGrupId = request.StokGrupId,
            KdvId = request.KdvId,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKartler.Add(stokKart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await StokKartDetayGetirAsync(stokKart.Id, cancellationToken);
    }

    public async Task<StokKartDto> StokKartGuncelleAsync(long id, StokKartGuncelleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKart adi bos olamaz.", "stok_kart_ad_required");
        }

        var stokKart = await _dbContext.StokKartler.TenantKapsami(_dbContext).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "stok_kart_not_found");

        await StokKartKoduTekrarKontroluAsync(stokKart.TenantId, request.StokKartKodu, stokKart.Id, cancellationToken);
        await KdvKontroluAsync(request.KdvId, stokKart.TenantId, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.StokKategoriId, request.StokGrupId, cancellationToken);

        stokKart.StokKartKodu = NormalizeOptional(request.StokKartKodu);
        stokKart.Ad = request.Ad.Trim();
        stokKart.Aciklama = NormalizeOptional(request.Aciklama);
        stokKart.StokKartTipi = request.StokKartTipi;
        stokKart.StokKategoriId = request.StokKategoriId;
        stokKart.StokGrupId = request.StokGrupId;
        stokKart.KdvId = request.KdvId;
        stokKart.AktifMi = request.AktifMi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await StokKartDetayGetirAsync(stokKart.Id, cancellationToken);
    }

    public async Task<StokKartDto> StokKartDetayGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var stokKart = await _dbContext.StokKartler.TenantKapsami(_dbContext)
            .Include(x => x.Kdv)
            .Include(x => x.StokKategori)
            .Include(x => x.StokGrup)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Renk)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Beden)
            .Include(x => x.Barkodlar.Where(y => y.AktifMi))
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "stok_kart_not_found");

        return new StokKartDto
        {
            Id = stokKart.Id,
            KdvId = stokKart.KdvId,
            KdvOrani = stokKart.Kdv.Oran,
            StokKartKodu = stokKart.StokKartKodu,
            Ad = stokKart.Ad,
            Aciklama = stokKart.Aciklama,
            StokKartTipi = stokKart.StokKartTipi,
            StokKategoriId = stokKart.StokKategoriId,
            StokKategoriAd = stokKart.StokKategori?.Ad,
            StokGrupId = stokKart.StokGrupId,
            StokGrupAd = stokKart.StokGrup?.Ad,
            AktifMi = stokKart.AktifMi,
            Barkodlar = stokKart.Barkodlar.OrderBy(x => x.BarkodNo).Select(x => new BarkodDto
            {
                Id = x.Id,
                BarkodNo = x.BarkodNo,
                BarkodTipi = x.BarkodTipi,
                StokKartId = x.StokKartId,
                StokKartVaryantId = x.StokKartVaryantId,
                StokKartAd = stokKart.Ad
            }).ToList(),
            Varyantlar = stokKart.Varyantlar.OrderBy(x => x.VaryantKodu).Select(x => new StokKartVaryantDto
            {
                Id = x.Id,
                StokKartId = x.StokKartId,
                RenkId = x.RenkId,
                RenkAd = x.Renk != null ? x.Renk.Ad : null,
                BedenId = x.BedenId,
                BedenAd = x.Beden != null ? x.Beden.Ad : null,
                VaryantKodu = x.VaryantKodu,
                BarkodluMu = x.BarkodluMu
            }).ToList()
        };
    }

    public async Task<SayfaliSonucDto<StokKartListeItemDto>> StokKartListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default, long? kategoriId = null, long? grupId = null, bool? aktifMi = null)
    {
        if (page <= 0 || pageSize is < 1 or > 200 || (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var pattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var countSql = @"SELECT COUNT(1)
FROM StokKart u
JOIN Kdv k ON k.Id=u.KdvId AND k.TenantId=u.TenantId
WHERE u.SilindiMi = 0
  AND (@TenantId IS NULL OR u.TenantId = @TenantId)
  AND (@KategoriId IS NULL OR u.StokKategoriId = @KategoriId)
  AND (@GrupId IS NULL OR u.StokGrupId = @GrupId)
  AND (@AktifMi IS NULL OR u.AktifMi = @AktifMi)
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT u.Id, u.KdvId, k.Oran AS KdvOrani, u.StokKartKodu, u.Ad, u.StokKartTipi, u.StokKategoriId, uk.Ad AS StokKategoriAd, u.StokGrupId, ug.Ad AS StokGrupAd, u.AktifMi
FROM StokKart u
JOIN Kdv k ON k.Id=u.KdvId AND k.TenantId=u.TenantId
LEFT JOIN StokKategori uk ON uk.Id = u.StokKategoriId AND uk.SilindiMi = 0 AND uk.TenantId = u.TenantId
LEFT JOIN StokGrup ug ON ug.Id = u.StokGrupId AND ug.SilindiMi = 0 AND ug.TenantId = u.TenantId
WHERE u.SilindiMi = 0
  AND (@TenantId IS NULL OR u.TenantId = @TenantId)
  AND (@KategoriId IS NULL OR u.StokKategoriId = @KategoriId)
  AND (@GrupId IS NULL OR u.StokGrupId = @GrupId)
  AND (@AktifMi IS NULL OR u.AktifMi = @AktifMi)
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search)
ORDER BY u.Ad, u.Id
LIMIT @Take OFFSET @Skip;"
            : @"SELECT u.Id, u.KdvId, k.Oran AS KdvOrani, u.StokKartKodu, u.Ad, u.StokKartTipi, u.StokKategoriId, uk.Ad AS StokKategoriAd, u.StokGrupId, ug.Ad AS StokGrupAd, u.AktifMi
FROM StokKart u
JOIN Kdv k ON k.Id=u.KdvId AND k.TenantId=u.TenantId
LEFT JOIN StokKategori uk ON uk.Id = u.StokKategoriId AND uk.SilindiMi = 0 AND uk.TenantId = u.TenantId
LEFT JOIN StokGrup ug ON ug.Id = u.StokGrupId AND ug.SilindiMi = 0 AND ug.TenantId = u.TenantId
WHERE u.SilindiMi = 0
  AND (@TenantId IS NULL OR u.TenantId = @TenantId)
  AND (@KategoriId IS NULL OR u.StokKategoriId = @KategoriId)
  AND (@GrupId IS NULL OR u.StokGrupId = @GrupId)
  AND (@AktifMi IS NULL OR u.AktifMi = @AktifMi)
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search)
ORDER BY u.Ad, u.Id
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = _dbContext.Baglam()?.TenantId, Search = pattern, KategoriId = kategoriId, GrupId = grupId, AktifMi = aktifMi, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<StokKartListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<StokKartListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<StokKartVaryantDto> StokKartVaryantOlusturAsync(long stokKartId, StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        var stokKart = await _dbContext.StokKartler.TenantKapsami(_dbContext).SingleOrDefaultAsync(x => x.Id == stokKartId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "stok_kart_not_found");

        if (request.RenkId is null && request.BedenId is null)
        {
            throw new UygulamaHatasi(400, "Gecersiz varyant", "Varyantta renk ve beden ikisi birden bos olamaz.", "variant_empty");
        }

        if (string.IsNullOrWhiteSpace(request.VaryantKodu))
        {
            throw new UygulamaHatasi(400, "Gecersiz varyant", "VaryantKodu bos olamaz.", "variant_code_required");
        }

        if (request.RenkId.HasValue && !await _dbContext.Renkler.TenantKapsami(_dbContext).AnyAsync(x => x.Id == request.RenkId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Renk bulunamadi", "Renk bulunamadi.", "renk_not_found");
        }

        if (request.BedenId.HasValue && !await _dbContext.Bedenler.TenantKapsami(_dbContext).AnyAsync(x => x.Id == request.BedenId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Beden bulunamadi", "Beden bulunamadi.", "beden_not_found");
        }

        var ayniKombinasyonVar = await _dbContext.StokKartVaryantlari.TenantKapsami(_dbContext).AnyAsync(x => x.StokKartId == stokKartId && x.RenkId == request.RenkId && x.BedenId == request.BedenId, cancellationToken);
        if (ayniKombinasyonVar)
        {
            throw new UygulamaHatasi(409, "Varyant hatasi", "Ayni urun altinda ayni varyant kombinasyonu tekrar edemez.", "variant_duplicate");
        }

        var varyant = new StokKartVaryant
        {
            TenantId = stokKart.TenantId,
            SubeId = stokKart.SubeId,
            StokKartId = stokKart.Id,
            RenkId = request.RenkId,
            BedenId = request.BedenId,
            VaryantKodu = request.VaryantKodu.Trim(),
            BarkodluMu = request.BarkodluMu,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKartVaryantlari.Add(varyant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await StokKartVaryantlariGetirAsync(stokKartId, cancellationToken)).Single(x => x.Id == varyant.Id);
    }

    public async Task<List<StokKartVaryantDto>> StokKartVaryantlariGetirAsync(long stokKartId, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.StokKartler.TenantKapsami(_dbContext).AnyAsync(x => x.Id == stokKartId, cancellationToken))
        {
            throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "stok_kart_not_found");
        }

        return await _dbContext.StokKartVaryantlari.TenantKapsami(_dbContext)
            .Where(x => x.StokKartId == stokKartId)
            .Include(x => x.Renk)
            .Include(x => x.Beden)
            .OrderBy(x => x.VaryantKodu)
            .Select(x => new StokKartVaryantDto
            {
                Id = x.Id,
                StokKartId = x.StokKartId,
                RenkId = x.RenkId,
                RenkAd = x.Renk != null ? x.Renk.Ad : null,
                BedenId = x.BedenId,
                BedenAd = x.Beden != null ? x.Beden.Ad : null,
                VaryantKodu = x.VaryantKodu,
                BarkodluMu = x.BarkodluMu
            })
            .ToListAsync(cancellationToken);
    }

    private async Task StokKartKoduTekrarKontroluAsync(Guid tenantId, string? stokKartKodu, long? stokKartId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptional(stokKartKodu);
        if (normalized is null)
        {
            return;
        }

        var exists = await _dbContext.StokKartler.TenantKapsami(_dbContext).AnyAsync(x => x.TenantId == tenantId && x.StokKartKodu == normalized && (!stokKartId.HasValue || x.Id != stokKartId.Value), cancellationToken);
        if (exists)
        {
            throw new UygulamaHatasi(409, "StokKart hatasi", "Ayni tenant icinde StokKartKodu tekrar etmesin.", "stok_kart_kodu_duplicate");
        }
    }

    private async Task KategoriVeGrupKontroluAsync(long? stokKategoriId, long? stokGrupId, CancellationToken cancellationToken)
    {
        if (stokKategoriId.HasValue && !await _dbContext.StokKategorileri.TenantKapsami(_dbContext).AnyAsync(x => x.Id == stokKategoriId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Kategori bulunamadi", "StokKart kategorisi bulunamadi.", "stok_kategori_not_found");
        }

        if (stokGrupId.HasValue && !await _dbContext.StokGruplari.TenantKapsami(_dbContext).AnyAsync(x => x.Id == stokGrupId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Grup bulunamadi", "StokKart grubu bulunamadi.", "stok_grup_not_found");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private async Task KdvKontroluAsync(long id, Guid tenant, CancellationToken ct)
    {
        if (!await _dbContext.Kdvler.TenantKapsami(_dbContext).AnyAsync(x => x.Id == id && x.TenantId == tenant && x.AktifMi, ct))
            throw new UygulamaHatasi(400, "Gecersiz KDV", "Ayni tenant icinde aktif KDV secilmelidir.", "kdv_invalid");
    }
}
