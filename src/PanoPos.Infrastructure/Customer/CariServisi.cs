using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Customer;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Customer;

public sealed class CariServisi : ICariServisi
{
    private readonly PanoPosDbContext _dbContext;

    public CariServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CariDto> CariOlusturAsync(CariOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.SubeId, request.Ad);

        var sube = await GetSubeAsync(request.SubeId, cancellationToken);
        await CariKoduTekrarKontroluAsync(sube.TenantId, request.CariKodu, null, cancellationToken);

        var cari = new Cari
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            CariKodu = NormalizeOptional(request.CariKodu),
            Ad = request.Ad.Trim(),
            Tip = request.Tip,
            Telefon = NormalizeOptional(request.Telefon),
            Email = NormalizeOptional(request.Email),
            VergiNo = NormalizeOptional(request.VergiNo),
            AktifMi = request.AktifMi,
            SilindiMi = false
        };

        _dbContext.Cariler.Add(cari);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await CariGetirAsync(cari.Id, cari.SubeId, cancellationToken);
    }

    public async Task<CariDto> CariGuncelleAsync(long id, CariGuncelleRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.SubeId, request.Ad);

        var cari = await _dbContext.Cariler.SingleOrDefaultAsync(x => x.Id == id && x.SubeId == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_not_found");

        await CariKoduTekrarKontroluAsync(cari.TenantId, request.CariKodu, cari.Id, cancellationToken);

        cari.CariKodu = NormalizeOptional(request.CariKodu);
        cari.Ad = request.Ad.Trim();
        cari.Tip = request.Tip;
        cari.Telefon = NormalizeOptional(request.Telefon);
        cari.Email = NormalizeOptional(request.Email);
        cari.VergiNo = NormalizeOptional(request.VergiNo);
        cari.AktifMi = request.AktifMi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await CariGetirAsync(cari.Id, cari.SubeId, cancellationToken);
    }

    public async Task<CariDto> CariGetirAsync(long id, long subeId, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        var cari = await _dbContext.Cariler.SingleOrDefaultAsync(x => x.Id == id && x.SubeId == subeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_not_found");

        return MapDto(cari);
    }

    public async Task<SayfaliSonucDto<CariListeItemDto>> CariListeleAsync(long subeId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (page <= 0 || pageSize <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var tenantId = await _dbContext.Subeler
            .Where(x => x.Id == subeId)
            .Select(x => x.TenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (tenantId == Guid.Empty)
        {
            throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
        }

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var pattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var countSql = @"SELECT COUNT(1)
FROM Cari
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Search IS NULL OR Ad LIKE @Search OR CariKodu LIKE @Search);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, CariKodu, Ad, Tip, Telefon, AktifMi
FROM Cari
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Search IS NULL OR Ad LIKE @Search OR CariKodu LIKE @Search)
ORDER BY Ad
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, CariKodu, Ad, Tip, Telefon, AktifMi
FROM Cari
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Search IS NULL OR Ad LIKE @Search OR CariKodu LIKE @Search)
ORDER BY Ad
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new
        {
            TenantId = tenantId,
            SubeId = subeId,
            Search = pattern,
            Skip = (page - 1) * pageSize,
            Take = pageSize
        };

        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<CariListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<CariListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    private async Task<Sube> GetSubeAsync(long subeId, CancellationToken cancellationToken)
    {
        return await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == subeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
    }

    private async Task CariKoduTekrarKontroluAsync(Guid tenantId, string? cariKodu, long? cariId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptional(cariKodu);
        if (normalized is null)
        {
            return;
        }

        var exists = await _dbContext.Cariler.AnyAsync(
            x => x.TenantId == tenantId && x.CariKodu == normalized && (!cariId.HasValue || x.Id != cariId.Value),
            cancellationToken);

        if (exists)
        {
            throw new UygulamaHatasi(409, "Cari hatasi", "Ayni tenant icinde CariKodu tekrar edemez.", "cari_kodu_duplicate");
        }
    }

    private static CariDto MapDto(Cari cari)
    {
        return new CariDto
        {
            Id = cari.Id,
            SubeId = cari.SubeId,
            CariKodu = cari.CariKodu,
            Ad = cari.Ad,
            Tip = cari.Tip,
            Telefon = cari.Telefon,
            Email = cari.Email,
            VergiNo = cari.VergiNo,
            AktifMi = cari.AktifMi
        };
    }

    private static void ValidateRequest(long subeId, string ad)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Cari adi bos olamaz.", "cari_ad_required");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
