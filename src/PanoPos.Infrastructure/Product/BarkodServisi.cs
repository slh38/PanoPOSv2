using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class BarkodServisi : IBarkodServisi
{
    private readonly PanoPosDbContext _dbContext;

    public BarkodServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BarkodDto> BarkodOlusturAsync(BarkodOlusturRequestDto request, CancellationToken cancellationToken = default) => KaydetAsync(null, request, cancellationToken);

    public async Task<BarkodDto?> BarkodIleBulAsync(string barkodNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barkodNo))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "BarkodNo bos olamaz.", "barcode_required");
        }

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var sql = @"SELECT b.Id, b.BarkodNo, b.BarkodTipi, b.UrunId, b.UrunVaryantId, u.Ad AS UrunAd, uv.VaryantKodu
                    FROM Barkod b
                    LEFT JOIN Urun u ON u.Id = b.UrunId AND u.SilindiMi = 0
                    LEFT JOIN UrunVaryant uv ON uv.Id = b.UrunVaryantId AND uv.SilindiMi = 0
                    WHERE b.SilindiMi = 0 AND b.BarkodNo = @BarkodNo;";

        return await connection.QuerySingleOrDefaultAsync<BarkodDto>(new CommandDefinition(sql, new { BarkodNo = barkodNo.Trim() }, cancellationToken: cancellationToken));
    }

    public Task<BarkodDto> BarkodGuncelleAsync(long id, BarkodOlusturRequestDto request, CancellationToken cancellationToken = default) => KaydetAsync(id, request, cancellationToken);

    private async Task<BarkodDto> KaydetAsync(long? id, BarkodOlusturRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BarkodNo))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "BarkodNo bos olamaz.", "barcode_required");
        }

        if ((request.UrunId.HasValue && request.UrunVaryantId.HasValue) || (!request.UrunId.HasValue && !request.UrunVaryantId.HasValue))
        {
            throw new UygulamaHatasi(400, "Gecersiz barkod", "Barkod mutlaka urun veya varyanta bagli olsun.", "barcode_target_invalid");
        }

        Guid tenantId;
        long subeId;
        if (request.UrunId.HasValue)
        {
            var urun = await _dbContext.Urunler.SingleOrDefaultAsync(x => x.Id == request.UrunId.Value, cancellationToken)
                ?? throw new UygulamaHatasi(404, "Urun bulunamadi", "Urun bulunamadi.", "urun_not_found");
            tenantId = urun.TenantId;
            subeId = urun.SubeId;
        }
        else
        {
            var varyant = await _dbContext.UrunVaryantlari.SingleOrDefaultAsync(x => x.Id == request.UrunVaryantId!.Value, cancellationToken)
                ?? throw new UygulamaHatasi(404, "Varyant bulunamadi", "Varyant bulunamadi.", "variant_not_found");
            tenantId = varyant.TenantId;
            subeId = varyant.SubeId;
        }

        var barkodNo = request.BarkodNo.Trim();
        var duplicate = await _dbContext.Barkodlar.AnyAsync(x => x.TenantId == tenantId && x.BarkodNo == barkodNo && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicate)
        {
            throw new UygulamaHatasi(409, "Barkod hatasi", "Ayni barkod iki kez eklenemez.", "barcode_duplicate");
        }

        Barkod barkod;
        if (id.HasValue)
        {
            barkod = await _dbContext.Barkodlar.SingleOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? throw new UygulamaHatasi(404, "Barkod bulunamadi", "Barkod bulunamadi.", "barcode_not_found");
        }
        else
        {
            barkod = new Barkod();
            _dbContext.Barkodlar.Add(barkod);
        }

        barkod.TenantId = tenantId;
        barkod.SubeId = subeId;
        barkod.BarkodNo = barkodNo;
        barkod.BarkodTipi = request.BarkodTipi;
        barkod.UrunId = request.UrunId;
        barkod.UrunVaryantId = request.UrunVaryantId;
        barkod.AktifMi = true;
        barkod.SilindiMi = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await BarkodIleBulAsync(barkodNo, cancellationToken))!;
    }
}
