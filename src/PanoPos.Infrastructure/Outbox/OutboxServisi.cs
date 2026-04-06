using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Outbox;

public sealed class OutboxServisi : IOutboxServisi
{
    private readonly PanoPosDbContext _dbContext;

    public OutboxServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OutboxOlayDto> OlayEkleAsync(OutboxOlayEkleRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateEkleRequest(request);

        var cihazVar = await _dbContext.Cihazlar.AnyAsync(x => x.Id == request.CihazId && x.SubeId == request.SubeId, cancellationToken);
        if (!cihazVar)
        {
            throw new UygulamaHatasi(404, "Cihaz bulunamadi", "Cihaz bulunamadi.", "cihaz_not_found");
        }

        var olay = new OutboxOlay
        {
            TenantId = request.TenantId,
            SubeId = request.SubeId,
            CihazId = request.CihazId,
            OlayTipi = request.OlayTipi.Trim(),
            KaynakTablo = request.KaynakTablo.Trim(),
            KaynakId = request.KaynakId,
            PayloadJson = request.PayloadJson.Trim(),
            Durum = request.Durum,
            DenemeSayisi = request.DenemeSayisi,
            OlusturmaTarihi = request.OlusturmaTarihi ?? DateTime.UtcNow
        };

        _dbContext.OutboxOlaylari.Add(olay);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDto(olay);
    }

    public async Task<SayfaliSonucDto<OutboxListeItemDto>> BekleyenleriListeleAsync(long subeId, OutboxDurumu? durum, int page, int pageSize, CancellationToken cancellationToken = default)
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
FROM OutboxOlay
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@Durum IS NULL OR Durum = @Durum);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, OlayTipi, KaynakTablo, KaynakId, Durum, DenemeSayisi, OlusturmaTarihi, GonderimTarihi
FROM OutboxOlay
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY OlusturmaTarihi DESC, Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, OlayTipi, KaynakTablo, KaynakId, Durum, DenemeSayisi, OlusturmaTarihi, GonderimTarihi
FROM OutboxOlay
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY OlusturmaTarihi DESC, Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Durum = durum, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<OutboxListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<OutboxListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<OutboxOlayDto> GonderildiIsaretleAsync(long id, CancellationToken cancellationToken = default)
    {
        var olay = await _dbContext.OutboxOlaylari.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Outbox olay bulunamadi", "Outbox olay bulunamadi.", "outbox_not_found");

        olay.Durum = OutboxDurumu.Gonderildi;
        olay.GonderimTarihi = DateTime.UtcNow;
        olay.SonHataMesaji = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDto(olay);
    }

    public async Task<OutboxOlayDto> HataIsaretleAsync(long id, string hataMesaji, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hataMesaji))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Hata mesaji zorunludur.", "outbox_error_required");
        }

        var olay = await _dbContext.OutboxOlaylari.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Outbox olay bulunamadi", "Outbox olay bulunamadi.", "outbox_not_found");

        olay.Durum = OutboxDurumu.Hata;
        olay.DenemeSayisi += 1;
        olay.SonHataMesaji = hataMesaji.Trim();
        olay.GonderimTarihi = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDto(olay);
    }

    public async Task<OutboxOlayDto> GetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var olay = await _dbContext.OutboxOlaylari.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Outbox olay bulunamadi", "Outbox olay bulunamadi.", "outbox_not_found");

        return MapDto(olay);
    }

    private static void ValidateEkleRequest(OutboxOlayEkleRequestDto request)
    {
        if (request.TenantId == Guid.Empty || request.SubeId <= 0 || request.CihazId <= 0 || request.KaynakId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "TenantId, SubeId, CihazId ve KaynakId zorunludur.", "outbox_required_fields");
        }

        if (string.IsNullOrWhiteSpace(request.OlayTipi) || string.IsNullOrWhiteSpace(request.KaynakTablo) || string.IsNullOrWhiteSpace(request.PayloadJson))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "OlayTipi, KaynakTablo ve PayloadJson zorunludur.", "outbox_payload_required");
        }
    }

    private static OutboxOlayDto MapDto(OutboxOlay olay)
    {
        return new OutboxOlayDto
        {
            Id = olay.Id,
            TenantId = olay.TenantId,
            SubeId = olay.SubeId,
            CihazId = olay.CihazId,
            OlayTipi = olay.OlayTipi,
            KaynakTablo = olay.KaynakTablo,
            KaynakId = olay.KaynakId,
            PayloadJson = olay.PayloadJson,
            Durum = olay.Durum,
            DenemeSayisi = olay.DenemeSayisi,
            OlusturmaTarihi = olay.OlusturmaTarihi,
            GonderimTarihi = olay.GonderimTarihi,
            SonHataMesaji = olay.SonHataMesaji
        };
    }
}
