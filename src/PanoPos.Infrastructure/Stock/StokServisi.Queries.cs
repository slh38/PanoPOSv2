using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PanoPos.Application.Common;
using PanoPos.Application.Stock;

namespace PanoPos.Infrastructure.Stock;

public sealed partial class StokServisi
{
    public async Task<StokFisDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default)
    {
        var tenant = await TenantAsync(subeId, ct);
        var fis = await db.StokFisleri.AsNoTracking().Include(x => x.Detaylar)
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenant && x.SubeId == subeId, ct)
            ?? throw new UygulamaHatasi(404, "Stok fisi bulunamadi", "Bu subede stok fisi bulunamadi.", "stock_not_found");
        return Map(fis);
    }

    public async Task<StokMiktarDto> GetMiktarAsync(long subeId, long depoId, long stokKartId, long? varyantId, CancellationToken ct = default)
    {
        var tenant = await TenantAsync(subeId, ct);
        await ValidateDepoAsync(tenant, subeId, depoId, ct);
        await ValidateStockAsync(tenant, stokKartId, varyantId, ct);
        return new(depoId, stokKartId, varyantId, await SumAsync(tenant, subeId, depoId, stokKartId, varyantId, ct));
    }

    private async Task<decimal> SumAsync(Guid tenant, long subeId, long depoId, long stockId, long? variantId, CancellationToken ct)
    {
        if ((db.Database.ProviderName ?? "").Contains("Sqlite"))
        {
            // SQLite SUM uses binary floats. Sum only the scoped quantities as decimal for exact test semantics.
            var quantities = await db.StokHareketleri.AsNoTracking().Where(x =>
                x.TenantId == tenant && x.SubeId == subeId && x.DepoId == depoId &&
                x.StokKartId == stockId && x.StokKartVaryantId == variantId).Select(x => x.Miktar).ToListAsync(ct);
            return quantities.Sum();
        }
        var transaction = db.Database.CurrentTransaction?.GetDbTransaction();
        var locking = transaction is null ? "" : " WITH (UPDLOCK, HOLDLOCK)";
        var sql = "SELECT COALESCE(SUM(Miktar),0) FROM StokHareket" + locking +
            " WHERE TenantId=@TenantId AND SubeId=@SubeId AND DepoId=@DepoId AND StokKartId=@StokKartId" +
            " AND ((@VaryantId IS NULL AND StokKartVaryantId IS NULL) OR StokKartVaryantId=@VaryantId)";
        return await db.Database.GetDbConnection().ExecuteScalarAsync<decimal>(new CommandDefinition(sql,
            new { TenantId = tenant, SubeId = subeId, DepoId = depoId, StokKartId = stockId, VaryantId = variantId },
            transaction, cancellationToken: ct));
    }

    public async Task<SayfaliSonucDto<StokFisListeDto>> GetPagedAsync(StokFisFiltre r, CancellationToken ct = default)
    {
        if (r.Page < 1 || r.PageSize is < 1 or > 500 || (r.StokFisTipi.HasValue && !Enum.IsDefined(r.StokFisTipi.Value)))
            throw Error("Sayfalama veya fis tipi gecersiz.");
        var tenant = await TenantAsync(r.SubeId, ct);
        if (r.DepoId.HasValue) await ValidateDepoAsync(tenant, r.SubeId, r.DepoId.Value, ct);
        const string where = @" FROM StokFis
WHERE TenantId=@TenantId AND SubeId=@SubeId AND SilindiMi=0
AND (@Tip IS NULL OR StokFisTipi=@Tip)
AND (@DepoId IS NULL OR DepoId=@DepoId OR KaynakDepoId=@DepoId OR HedefDepoId=@DepoId)
AND (@Search IS NULL OR FisNo LIKE @Search OR Aciklama LIKE @Search)";
        var args = new { TenantId = tenant, r.SubeId, Tip = (int?)r.StokFisTipi, r.DepoId,
            Search = string.IsNullOrWhiteSpace(r.Arama) ? null : "%" + r.Arama.Trim() + "%",
            Skip = (long)(r.Page - 1) * r.PageSize, Take = r.PageSize };
        var conn = db.Database.GetDbConnection();
        var count = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT COUNT(1)" + where, args, cancellationToken: ct));
        var paging = (db.Database.ProviderName ?? "").Contains("Sqlite") ? " LIMIT @Take OFFSET @Skip" : " OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
        var rows = await conn.QueryAsync<StokFisListeDto>(new CommandDefinition(
            "SELECT Id, FaturaId, AlisFaturaId, StokFisTipi, FisNo, FisTarihi, DepoId, KaynakDepoId, HedefDepoId, Aciklama" +
            where + " ORDER BY FisTarihi DESC, Id DESC" + paging, args, cancellationToken: ct));
        return new() { Kayitlar = rows.ToList(), ToplamKayit = count, Sayfa = r.Page, SayfaBoyutu = r.PageSize };
    }
}
