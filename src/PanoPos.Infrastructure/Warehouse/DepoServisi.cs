using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Warehouse;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Warehouse;

public sealed class DepoServisi : IDepoServisi
{
    private readonly PanoPosDbContext _dbContext;

    public DepoServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DepoDto> CreateAsync(DepoKaydetRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var sube = await GetSubeAsync(request.SubeId, cancellationToken);
        var depoKodu = NormalizeDepoKodu(request.DepoKodu);
        await EnsureCodeAvailableAsync(sube.TenantId, sube.Id, depoKodu, null, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (request.VarsayilanMi)
        {
            await ClearDefaultAsync(sube.TenantId, sube.Id, cancellationToken);
        }

        var depo = new Depo
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            DepoKodu = depoKodu,
            Ad = request.Ad.Trim(),
            VarsayilanMi = request.VarsayilanMi,
            AktifMi = request.AktifMi
        };

        _dbContext.Depolar.Add(depo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return MapDto(depo);
    }

    public async Task<DepoDto> UpdateAsync(long id, DepoKaydetRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var depo = await GetDepoAsync(id, request.SubeId, cancellationToken);
        var depoKodu = NormalizeDepoKodu(request.DepoKodu);
        await EnsureCodeAvailableAsync(depo.TenantId, depo.SubeId, depoKodu, depo.Id, cancellationToken);

        if (depo.VarsayilanMi && !request.AktifMi)
        {
            throw new UygulamaHatasi(409, "Depo hatasi", "Varsayilan depo pasif yapilamaz.", "default_depo_must_be_active");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (request.VarsayilanMi)
        {
            await ClearDefaultAsync(depo.TenantId, depo.SubeId, cancellationToken, depo.Id);
        }

        depo.DepoKodu = depoKodu;
        depo.Ad = request.Ad.Trim();
        depo.AktifMi = request.AktifMi;
        depo.VarsayilanMi = request.VarsayilanMi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return MapDto(depo);
    }

    public async Task<DepoDto> GetByIdAsync(long id, long subeId, CancellationToken cancellationToken = default)
    {
        return MapDto(await GetDepoAsync(id, subeId, cancellationToken));
    }

    public async Task<SayfaliSonucDto<DepoDto>> GetPagedAsync(long subeId, string? arama, bool? aktifMi, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0 || pageSize <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var sube = await GetSubeAsync(subeId, cancellationToken);
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var parameters = new
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Search = string.IsNullOrWhiteSpace(arama) ? null : $"%{arama.Trim()}%",
            AktifMi = aktifMi,
            Skip = (page - 1) * pageSize,
            Take = pageSize
        };
        const string countSql = @"SELECT COUNT(1)
FROM Depo
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@AktifMi IS NULL OR AktifMi = @AktifMi)
  AND (@Search IS NULL OR DepoKodu LIKE @Search OR Ad LIKE @Search);";
        var listSql = (_dbContext.Database.ProviderName ?? string.Empty).Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, TenantId, SubeId, DepoKodu, Ad, VarsayilanMi, AktifMi
FROM Depo
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@AktifMi IS NULL OR AktifMi = @AktifMi)
  AND (@Search IS NULL OR DepoKodu LIKE @Search OR Ad LIKE @Search)
ORDER BY DepoKodu
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, TenantId, SubeId, DepoKodu, Ad, VarsayilanMi, AktifMi
FROM Depo
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@AktifMi IS NULL OR AktifMi = @AktifMi)
  AND (@Search IS NULL OR DepoKodu LIKE @Search OR Ad LIKE @Search)
ORDER BY DepoKodu
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<DepoListRow>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).Select(x => new DepoDto { Id = x.Id, TenantId = Guid.Parse(x.TenantId), SubeId = x.SubeId, DepoKodu = x.DepoKodu, Ad = x.Ad, VarsayilanMi = x.VarsayilanMi, AktifMi = x.AktifMi }).ToList();
        return new SayfaliSonucDto<DepoDto> { ToplamKayit = toplamKayit, Sayfa = page, SayfaBoyutu = pageSize, Kayitlar = kayitlar };
    }

    public async Task DeleteAsync(long id, long subeId, CancellationToken cancellationToken = default)
    {
        var depo = await GetDepoAsync(id, subeId, cancellationToken);
        if (depo.VarsayilanMi)
        {
            throw new UygulamaHatasi(409, "Depo hatasi", "Varsayilan depo silinemez.", "default_depo_cannot_be_deleted");
        }

        _dbContext.Depolar.Remove(depo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DepoDto> SetDefaultAsync(long id, long subeId, CancellationToken cancellationToken = default)
    {
        var depo = await GetDepoAsync(id, subeId, cancellationToken);
        if (!depo.AktifMi)
        {
            throw new UygulamaHatasi(409, "Depo hatasi", "Pasif depo varsayilan yapilamaz.", "inactive_depo_cannot_be_default");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await ClearDefaultAsync(depo.TenantId, depo.SubeId, cancellationToken, depo.Id);
        depo.VarsayilanMi = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return MapDto(depo);
    }

    public async Task EnsureDefaultDepoAsync(Guid tenantId, long subeId, CancellationToken cancellationToken = default)
    {
        var sube = await _dbContext.Subeler.YetkiliSube(_dbContext).SingleOrDefaultAsync(x => x.Id == subeId && x.TenantId == tenantId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
        if (await _dbContext.Depolar.SubeKapsami(_dbContext).AnyAsync(x => x.TenantId == tenantId && x.SubeId == subeId && x.VarsayilanMi && x.AktifMi, cancellationToken))
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var merkez = await _dbContext.Depolar.SubeKapsami(_dbContext).SingleOrDefaultAsync(x => x.TenantId == tenantId && x.SubeId == subeId && x.DepoKodu == "MERKEZ", cancellationToken);
        if (merkez is null)
        {
            merkez = new Depo { TenantId = tenantId, SubeId = sube.Id, DepoKodu = "MERKEZ", Ad = "Merkez Depo", VarsayilanMi = true, AktifMi = true };
            _dbContext.Depolar.Add(merkez);
        }
        else
        {
            if (!merkez.AktifMi)
            {
                merkez.AktifMi = true;
            }

            await ClearDefaultAsync(tenantId, subeId, cancellationToken, merkez.Id);
            merkez.VarsayilanMi = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Sube> GetSubeAsync(long subeId, CancellationToken cancellationToken)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        return await _dbContext.Subeler.YetkiliSube(_dbContext).SingleOrDefaultAsync(x => x.Id == subeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
    }

    private async Task<Depo> GetDepoAsync(long id, long subeId, CancellationToken cancellationToken)
    {
        await GetSubeAsync(subeId, cancellationToken);
        return await _dbContext.Depolar.SubeKapsami(_dbContext).SingleOrDefaultAsync(x => x.Id == id && x.SubeId == subeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Depo bulunamadi", "Depo bulunamadi.", "depo_not_found");
    }

    private async Task EnsureCodeAvailableAsync(Guid tenantId, long subeId, string depoKodu, long? excludedId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Depolar.SubeKapsami(_dbContext).AnyAsync(x => x.TenantId == tenantId && x.SubeId == subeId && x.DepoKodu == depoKodu && x.AktifMi && (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);
        if (exists)
        {
            throw new UygulamaHatasi(409, "Depo hatasi", "Depo kodu tekrar edemez.", "depo_duplicate");
        }
    }

    private async Task ClearDefaultAsync(Guid tenantId, long subeId, CancellationToken cancellationToken, long? excludedId = null)
    {
        var defaults = await _dbContext.Depolar.SubeKapsami(_dbContext).Where(x => x.TenantId == tenantId && x.SubeId == subeId && x.VarsayilanMi && (!excludedId.HasValue || x.Id != excludedId.Value)).ToListAsync(cancellationToken);
        foreach (var depo in defaults)
        {
            depo.VarsayilanMi = false;
        }
    }

    private static void ValidateRequest(DepoKaydetRequestDto request)
    {
        if (request.SubeId <= 0 || string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Sube, depo kodu ve adi zorunludur.", "depo_invalid");
        }

        _ = NormalizeDepoKodu(request.DepoKodu);
        if (request.VarsayilanMi && !request.AktifMi)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Pasif depo varsayilan olamaz.", "inactive_depo_cannot_be_default");
        }
    }

    private static string NormalizeDepoKodu(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > 30)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Depo kodu 1 ile 30 karakter arasynda olmalidir.", "depo_kodu_invalid");
        }

        return normalized;
    }

    private sealed class DepoListRow
    {
        public long Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public long SubeId { get; set; }
        public string DepoKodu { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public bool VarsayilanMi { get; set; }
        public bool AktifMi { get; set; }
    }
    private static DepoDto MapDto(Depo depo) => new()
    {
        Id = depo.Id,
        TenantId = depo.TenantId,
        SubeId = depo.SubeId,
        DepoKodu = depo.DepoKodu,
        Ad = depo.Ad,
        VarsayilanMi = depo.VarsayilanMi,
        AktifMi = depo.AktifMi
    };
}

