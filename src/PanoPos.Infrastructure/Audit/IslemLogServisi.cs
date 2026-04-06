using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Audit;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Audit;

public sealed class IslemLogServisi : IIslemLogServisi
{
    private readonly PanoPosDbContext _dbContext;

    public IslemLogServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IslemLogDto> LogEkleAsync(IslemLogEkleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "TenantId zorunludur.", "tenant_required");
        }

        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(request.ModulAdi))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "ModulAdi zorunludur.", "module_required");
        }

        if (string.IsNullOrWhiteSpace(request.IslemTipi))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "IslemTipi zorunludur.", "action_type_required");
        }

        var log = new IslemLog
        {
            TenantId = request.TenantId,
            SubeId = request.SubeId,
            CihazId = request.CihazId,
            KullaniciId = request.KullaniciId,
            ModulAdi = request.ModulAdi.Trim(),
            EkranAdi = NormalizeOptional(request.EkranAdi),
            ButonAdi = NormalizeOptional(request.ButonAdi),
            IslemTipi = request.IslemTipi.Trim(),
            HedefTablo = NormalizeOptional(request.HedefTablo),
            HedefId = request.HedefId,
            Aciklama = NormalizeOptional(request.Aciklama),
            BasariliMi = request.BasariliMi,
            HataKodu = NormalizeOptional(request.HataKodu),
            HataMesaji = NormalizeOptional(request.HataMesaji),
            SureMs = request.SureMs,
            CorrelationId = NormalizeOptional(request.CorrelationId),
            OlusturmaTarihi = request.OlusturmaTarihi ?? DateTime.UtcNow
        };

        _dbContext.IslemLoglari.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapDto(log);
    }

    public async Task<SayfaliSonucDto<IslemLogListeItemDto>> ListeleAsync(IslemLogListeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var tenantId = await _dbContext.Subeler
            .Where(x => x.Id == request.SubeId)
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

        var countSql = @"SELECT COUNT(1)
FROM IslemLog
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@KullaniciId IS NULL OR KullaniciId = @KullaniciId)
  AND (@IslemTipi IS NULL OR IslemTipi = @IslemTipi)
  AND (@BasariliMi IS NULL OR BasariliMi = @BasariliMi);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, ModulAdi, EkranAdi, ButonAdi, IslemTipi, HedefTablo, HedefId, BasariliMi, HataKodu, KullaniciId, CihazId, OlusturmaTarihi
FROM IslemLog
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@KullaniciId IS NULL OR KullaniciId = @KullaniciId)
  AND (@IslemTipi IS NULL OR IslemTipi = @IslemTipi)
  AND (@BasariliMi IS NULL OR BasariliMi = @BasariliMi)
ORDER BY OlusturmaTarihi DESC, Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, ModulAdi, EkranAdi, ButonAdi, IslemTipi, HedefTablo, HedefId, BasariliMi, HataKodu, KullaniciId, CihazId, OlusturmaTarihi
FROM IslemLog
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@KullaniciId IS NULL OR KullaniciId = @KullaniciId)
  AND (@IslemTipi IS NULL OR IslemTipi = @IslemTipi)
  AND (@BasariliMi IS NULL OR BasariliMi = @BasariliMi)
ORDER BY OlusturmaTarihi DESC, Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new
        {
            TenantId = tenantId,
            SubeId = request.SubeId,
            KullaniciId = request.KullaniciId,
            IslemTipi = NormalizeOptional(request.IslemTipi),
            BasariliMi = request.BasariliMi,
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize
        };

        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<IslemLogListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<IslemLogListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = request.Page,
            SayfaBoyutu = request.PageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<IslemLogDto> DetayGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var log = await _dbContext.IslemLoglari.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Islem log bulunamadi", "Islem log bulunamadi.", "audit_log_not_found");

        return MapDto(log);
    }

    private static IslemLogDto MapDto(IslemLog log)
    {
        return new IslemLogDto
        {
            Id = log.Id,
            TenantId = log.TenantId,
            SubeId = log.SubeId,
            CihazId = log.CihazId,
            KullaniciId = log.KullaniciId,
            ModulAdi = log.ModulAdi,
            EkranAdi = log.EkranAdi,
            ButonAdi = log.ButonAdi,
            IslemTipi = log.IslemTipi,
            HedefTablo = log.HedefTablo,
            HedefId = log.HedefId,
            Aciklama = log.Aciklama,
            BasariliMi = log.BasariliMi,
            HataKodu = log.HataKodu,
            HataMesaji = log.HataMesaji,
            SureMs = log.SureMs,
            CorrelationId = log.CorrelationId,
            OlusturmaTarihi = log.OlusturmaTarihi
        };
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
