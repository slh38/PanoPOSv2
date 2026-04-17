using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class UrunServisi : IUrunServisi
{
    private readonly PanoPosDbContext _dbContext;

    public UrunServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UrunDto> UrunOlusturAsync(UrunOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Urun adi bos olamaz.", "urun_ad_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        await UrunKoduTekrarKontroluAsync(sube.TenantId, request.UrunKodu, null, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.UrunKategoriId, request.UrunGrupId, cancellationToken);

        var urun = new Urun
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            UrunKodu = NormalizeOptional(request.UrunKodu),
            Ad = request.Ad.Trim(),
            Aciklama = NormalizeOptional(request.Aciklama),
            UrunTipi = request.UrunTipi,
            UrunKategoriId = request.UrunKategoriId,
            UrunGrupId = request.UrunGrupId,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Urunler.Add(urun);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await UrunDetayGetirAsync(urun.Id, cancellationToken);
    }

    public async Task<UrunDto> UrunGuncelleAsync(long id, UrunGuncelleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Urun adi bos olamaz.", "urun_ad_required");
        }

        var urun = await _dbContext.Urunler.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");

        await UrunKoduTekrarKontroluAsync(urun.TenantId, request.UrunKodu, urun.Id, cancellationToken);
        await KategoriVeGrupKontroluAsync(request.UrunKategoriId, request.UrunGrupId, cancellationToken);

        urun.UrunKodu = NormalizeOptional(request.UrunKodu);
        urun.Ad = request.Ad.Trim();
        urun.Aciklama = NormalizeOptional(request.Aciklama);
        urun.UrunTipi = request.UrunTipi;
        urun.UrunKategoriId = request.UrunKategoriId;
        urun.UrunGrupId = request.UrunGrupId;
        urun.AktifMi = request.AktifMi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await UrunDetayGetirAsync(urun.Id, cancellationToken);
    }

    public async Task<UrunDto> UrunDetayGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var urun = await _dbContext.Urunler
            .Include(x => x.UrunKategori)
            .Include(x => x.UrunGrup)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Renk)
            .Include(x => x.Varyantlar.Where(y => y.AktifMi)).ThenInclude(x => x.Beden)
            .Include(x => x.Barkodlar.Where(y => y.AktifMi))
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");

        return new UrunDto
        {
            Id = urun.Id,
            UrunKodu = urun.UrunKodu,
            Ad = urun.Ad,
            Aciklama = urun.Aciklama,
            UrunTipi = urun.UrunTipi,
            UrunKategoriId = urun.UrunKategoriId,
            UrunKategoriAd = urun.UrunKategori?.Ad,
            UrunGrupId = urun.UrunGrupId,
            UrunGrupAd = urun.UrunGrup?.Ad,
            AktifMi = urun.AktifMi,
            Barkodlar = urun.Barkodlar.OrderBy(x => x.BarkodNo).Select(x => new BarkodDto
            {
                Id = x.Id,
                BarkodNo = x.BarkodNo,
                BarkodTipi = x.BarkodTipi,
                UrunId = x.UrunId,
                UrunVaryantId = x.UrunVaryantId,
                UrunAd = urun.Ad
            }).ToList(),
            Varyantlar = urun.Varyantlar.OrderBy(x => x.VaryantKodu).Select(x => new UrunVaryantDto
            {
                Id = x.Id,
                UrunId = x.UrunId,
                RenkId = x.RenkId,
                RenkAd = x.Renk != null ? x.Renk.Ad : null,
                BedenId = x.BedenId,
                BedenAd = x.Beden != null ? x.Beden.Ad : null,
                VaryantKodu = x.VaryantKodu,
                BarkodluMu = x.BarkodluMu
            }).ToList()
        };
    }

    public async Task<SayfaliSonucDto<UrunListeItemDto>> UrunListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
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
FROM Urun u
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.UrunKodu LIKE @Search);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT u.Id, u.UrunKodu, u.Ad, u.UrunTipi, u.UrunKategoriId, uk.Ad AS UrunKategoriAd, u.UrunGrupId, ug.Ad AS UrunGrupAd, u.AktifMi
FROM Urun u
LEFT JOIN UrunKategori uk ON uk.Id = u.UrunKategoriId AND uk.SilindiMi = 0
LEFT JOIN UrunGrup ug ON ug.Id = u.UrunGrupId AND ug.SilindiMi = 0
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.UrunKodu LIKE @Search)
ORDER BY u.Ad
LIMIT @Take OFFSET @Skip;"
            : @"SELECT u.Id, u.UrunKodu, u.Ad, u.UrunTipi, u.UrunKategoriId, uk.Ad AS UrunKategoriAd, u.UrunGrupId, ug.Ad AS UrunGrupAd, u.AktifMi
FROM Urun u
LEFT JOIN UrunKategori uk ON uk.Id = u.UrunKategoriId AND uk.SilindiMi = 0
LEFT JOIN UrunGrup ug ON ug.Id = u.UrunGrupId AND ug.SilindiMi = 0
WHERE u.SilindiMi = 0
  AND (@Search IS NULL OR u.Ad LIKE @Search OR u.UrunKodu LIKE @Search)
ORDER BY u.Ad
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { Search = pattern, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<UrunListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<UrunListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<UrunVaryantDto> UrunVaryantOlusturAsync(long urunId, UrunVaryantOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        var urun = await _dbContext.Urunler.SingleOrDefaultAsync(x => x.Id == urunId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");

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

        var ayniKombinasyonVar = await _dbContext.UrunVaryantlari.AnyAsync(x => x.UrunId == urunId && x.RenkId == request.RenkId && x.BedenId == request.BedenId, cancellationToken);
        if (ayniKombinasyonVar)
        {
            throw new UygulamaHatasi(409, "Varyant hatasi", "Ayni urun altinda ayni varyant kombinasyonu tekrar edemez.", "variant_duplicate");
        }

        var varyant = new UrunVaryant
        {
            TenantId = urun.TenantId,
            SubeId = urun.SubeId,
            UrunId = urun.Id,
            RenkId = request.RenkId,
            BedenId = request.BedenId,
            VaryantKodu = request.VaryantKodu.Trim(),
            BarkodluMu = request.BarkodluMu,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.UrunVaryantlari.Add(varyant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await UrunVaryantlariGetirAsync(urunId, cancellationToken)).Single(x => x.Id == varyant.Id);
    }

    public async Task<List<UrunVaryantDto>> UrunVaryantlariGetirAsync(long urunId, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Urunler.AnyAsync(x => x.Id == urunId, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");
        }

        return await _dbContext.UrunVaryantlari
            .Where(x => x.UrunId == urunId)
            .Include(x => x.Renk)
            .Include(x => x.Beden)
            .OrderBy(x => x.VaryantKodu)
            .Select(x => new UrunVaryantDto
            {
                Id = x.Id,
                UrunId = x.UrunId,
                RenkId = x.RenkId,
                RenkAd = x.Renk != null ? x.Renk.Ad : null,
                BedenId = x.BedenId,
                BedenAd = x.Beden != null ? x.Beden.Ad : null,
                VaryantKodu = x.VaryantKodu,
                BarkodluMu = x.BarkodluMu
            })
            .ToListAsync(cancellationToken);
    }

    private async Task UrunKoduTekrarKontroluAsync(Guid tenantId, string? urunKodu, long? urunId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptional(urunKodu);
        if (normalized is null)
        {
            return;
        }

        var exists = await _dbContext.Urunler.AnyAsync(x => x.TenantId == tenantId && x.UrunKodu == normalized && (!urunId.HasValue || x.Id != urunId.Value), cancellationToken);
        if (exists)
        {
            throw new UygulamaHatasi(409, "Urun hatasi", "Ayni tenant icinde UrunKodu tekrar etmesin.", "urun_kodu_duplicate");
        }
    }

    private async Task KategoriVeGrupKontroluAsync(long? urunKategoriId, long? urunGrupId, CancellationToken cancellationToken)
    {
        if (urunKategoriId.HasValue && !await _dbContext.UrunKategorileri.AnyAsync(x => x.Id == urunKategoriId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Kategori bulunamadi", "Urun kategorisi bulunamadi.", "urun_kategori_not_found");
        }

        if (urunGrupId.HasValue && !await _dbContext.UrunGruplari.AnyAsync(x => x.Id == urunGrupId.Value, cancellationToken))
        {
            throw new UygulamaHatasi(404, "Grup bulunamadi", "Urun grubu bulunamadi.", "urun_grup_not_found");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
