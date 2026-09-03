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
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKart adi bos olamaz.", "urun_ad_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        await StokKartKoduTekrarKontroluAsync(sube.TenantId, request.StokKartKodu, null, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.StokKategoriId, request.StokGrupId, cancellationToken);

        var urun = new StokKart
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            StokKartKodu = NormalizeOptional(request.StokKartKodu),
            Ad = request.Ad.Trim(),
            Aciklama = NormalizeOptional(request.Aciklama),
            StokKartTipi = request.StokKartTipi,
            StokKategoriId = request.StokKategoriId,
            StokGrupId = request.StokGrupId,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKartler.Add(urun);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await StokKartDetayGetirAsync(urun.Id, cancellationToken);
    }

    public async Task<StokKartDto> StokKartGuncelleAsync(long id, StokKartGuncelleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKart adi bos olamaz.", "urun_ad_required");
        }

        var urun = await _dbContext.StokKartler.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "urun_not_found");

        await StokKartKoduTekrarKontroluAsync(urun.TenantId, request.StokKartKodu, urun.Id, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.StokKategoriId, request.StokGrupId, cancellationToken);

        urun.StokKartKodu = NormalizeOptional(request.StokKartKodu);
        urun.Ad = request.Ad.Trim();
        urun.Aciklama = NormalizeOptional(request.Aciklama);
        urun.StokKartTipi = request.StokKartTipi;
        urun.StokKategoriId = request.StokKategoriId;
        urun.StokGrupId = request.StokGrupId;
        urun.AktifMi = request.AktifMi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await StokKartDetayGetirAsync(urun.Id, cancellationToken);
    }

    public async Task<StokKartDto> StokKartDetayGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var urun = await _dbContext.StokKartler
            .Include(x => x.StokKategori)
            .Include(x => x.StokGrup)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Renk)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Beden)
            .Include(x => x.Barkodlar.Where(y => y.AktifMi))
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "urun_not_found");

        return new StokKartDto
        {
            Id = urun.Id,
            StokKartKodu = urun.StokKartKodu,
            Ad = urun.Ad,
            Aciklama = urun.Aciklama,
            StokKartTipi = urun.StokKartTipi,
            StokKategoriId = urun.StokKategoriId,
            StokKategoriAd = urun.StokKategori?.Ad,
            StokGrupId = urun.StokGrupId,
            StokGrupAd = urun.StokGrup?.Ad,
            AktifMi = urun.AktifMi,
            Barkodlar = urun.Barkodlar.OrderBy(x => x.BarkodNo).Select(x => new BarkodDto
            {
                Id = x.Id,
                BarkodNo = x.BarkodNo,
                BarkodTipi = x.BarkodTipi,
                StokKartId = x.StokKartId,
                StokKartVaryantId = x.StokKartVaryantId,
                StokKartAd = urun.Ad
            }).ToList(),
            Varyantlar = urun.Varyantlar.OrderBy(x => x.VaryantKodu).Select(x => new StokKartVaryantDto
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

    public async Task<SayfaliSonucDto<StokKartListeItemDto>> StokKartListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0 || pageSize <= 0)
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
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT u.Id, u.StokKartKodu, u.Ad, u.StokKartTipi, u.StokKategoriId, uk.Ad AS StokKategoriAd, u.StokGrupId, ug.Ad AS StokGrupAd, u.AktifMi
FROM StokKart u
LEFT JOIN StokKategori uk ON uk.Id = u.StokKategoriId AND uk.SilindiMi = 0
LEFT JOIN StokGrup ug ON ug.Id = u.StokGrupId AND ug.SilindiMi = 0
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search)
ORDER BY u.Ad
LIMIT @Take OFFSET @Skip;"
            : @"SELECT u.Id, u.StokKartKodu, u.Ad, u.StokKartTipi, u.StokKategoriId, uk.Ad AS StokKategoriAd, u.StokGrupId, ug.Ad AS StokGrupAd, u.AktifMi
FROM StokKart u
LEFT JOIN StokKategori uk ON uk.Id = u.StokKategoriId AND uk.SilindiMi = 0
LEFT JOIN StokGrup ug ON ug.Id = u.StokGrupId AND ug.SilindiMi = 0
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.StokKartKodu LIKE @Search)
ORDER BY u.Ad
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { Search = pattern, Skip = (page - 1) * pageSize, Take = pageSize };
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

    public async Task<StokKartVaryantDto> StokKartVaryantOlusturAsync(long urunId, StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        var urun = await _dbContext.StokKartler.SingleOrDefaultAsync(x => x.Id == urunId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "urun_not_found");

        if (request.RenkId is null && request.BedenId is null)
        {
            throw new UygulamaHatasi(400, "Gecersiz varyant", "Varyantta renk ve beden ikisi birden bos olamaz.", "variant_empty");
        }

        if (string.IsNullOrWhiteSpace(request.VaryantKodu))
        {
            throw new UygulamaHatasi(400, "Gecersiz varyant", "VaryantKodu bos olamaz.", "variant_code_required");
        }

        if (request.RenkId.HasValue && !await _dbContext.Renkler.AnyAsync(x => x.Id == request.RenkId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Renk bulunamadi", "Renk bulunamadi.", "renk_not_found");
        }

        if (request.BedenId.HasValue && !await _dbContext.Bedenler.AnyAsync(x => x.Id == request.BedenId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Beden bulunamadi", "Beden bulunamadi.", "beden_not_found");
        }

        var ayniKombinasyonVar = await _dbContext.StokKartVaryantlari.AnyAsync(x => x.StokKartId == urunId && x.RenkId == request.RenkId && x.BedenId == request.BedenId, cancellationToken);
        if (ayniKombinasyonVar)
        {
            throw new UygulamaHatasi(409, "Varyant hatasi", "Ayni urun altinda ayni varyant kombinasyonu tekrar edemez.", "variant_duplicate");
        }

        var varyant = new StokKartVaryant
        {
            TenantId = urun.TenantId,
            SubeId = urun.SubeId,
            StokKartId = urun.Id,
            RenkId = request.RenkId,
            BedenId = request.BedenId,
            VaryantKodu = request.VaryantKodu.Trim(),
            BarkodluMu = request.BarkodluMu,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKartVaryantlari.Add(varyant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await StokKartVaryantlariGetirAsync(urunId, cancellationToken)).Single(x => x.Id == varyant.Id);
    }

    public async Task<List<StokKartVaryantDto>> StokKartVaryantlariGetirAsync(long urunId, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.StokKartler.AnyAsync(x => x.Id == urunId, cancellationToken))
        {
            throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "urun_not_found");
        }

        return await _dbContext.StokKartVaryantlari
            .Where(x => x.StokKartId == urunId)
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

    private async Task StokKartKoduTekrarKontroluAsync(Guid tenantId, string? urunKodu, long? urunId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptional(urunKodu);
        if (normalized is null)
        {
            return;
        }

        var exists = await _dbContext.StokKartler.AnyAsync(x => x.TenantId == tenantId && x.StokKartKodu == normalized && (!urunId.HasValue || x.Id != urunId.Value), cancellationToken);
        if (exists)
        {
            throw new UygulamaHatasi(409, "StokKart hatasi", "Ayni tenant icinde StokKartKodu tekrar etmesin.", "urun_kodu_duplicate");
        }
    }

    private async Task KategoriVeGrupKontroluAsync(long? urunKategoriId, long? urunGrupId, CancellationToken cancellationToken)
    {
        if (urunKategoriId.HasValue && !await _dbContext.StokKategorileri.AnyAsync(x => x.Id == urunKategoriId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Kategori bulunamadi", "StokKart kategorisi bulunamadi.", "urun_kategori_not_found");
        }

        if (urunGrupId.HasValue && !await _dbContext.StokGruplari.AnyAsync(x => x.Id == urunGrupId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Grup bulunamadi", "StokKart grubu bulunamadi.", "urun_grup_not_found");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
