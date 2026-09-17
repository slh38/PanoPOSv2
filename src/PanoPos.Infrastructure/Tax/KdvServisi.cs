using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Tax;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;
namespace PanoPos.Infrastructure.Tax;

public sealed class KdvServisi(PanoPosDbContext db) : IKdvServisi
{
    public async Task<KdvDto> CreateAsync(KdvKaydetRequest request, CancellationToken ct = default)
    {
        var tenant = await TenantAsync(request.SubeId, ct);
        await ValidateAsync(request, tenant, null, ct);
        var entity = new Kdv { TenantId = tenant, SubeId = request.SubeId,
            Kod = request.Kod.Trim().ToUpperInvariant(), Ad = request.Ad.Trim(), Oran = request.Oran, AktifMi = request.AktifMi };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Kdvler.Add(entity);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Map(entity);
    }
    public async Task<KdvDto> UpdateAsync(long id, KdvKaydetRequest request, CancellationToken ct = default)
    {
        var entity = await FindAsync(id, request.SubeId, ct);
        await ValidateAsync(request, entity.TenantId, id, ct);
        entity.Kod = request.Kod.Trim().ToUpperInvariant();
        entity.Ad = request.Ad.Trim();
        entity.Oran = request.Oran;
        entity.AktifMi = request.AktifMi;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }
    public async Task<KdvDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default) => Map(await FindAsync(id, subeId, ct));
    public async Task DeleteAsync(long id, long subeId, CancellationToken ct = default)
    {
        var entity = await FindAsync(id, subeId, ct);
        if (await db.StokKartler.TenantKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.KdvId == id, ct) ||
            await db.SiparisDetaylari.SubeKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.KdvId == id, ct) ||
            await db.FaturaDetaylari.SubeKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.KdvId == id, ct) ||
            await db.AlisFaturaDetaylari.SubeKapsami(db).IgnoreQueryFilters().AnyAsync(x => x.KdvId == id, ct))
            throw new UygulamaHatasi(409, "KDV kullaniliyor", "Kullanimdaki KDV silinemez.", "kdv_in_use");
        db.Kdvler.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
    public async Task<SayfaliSonucDto<KdvDto>> GetPagedAsync(long subeId, string? arama, bool? aktifMi, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 500) throw new UygulamaHatasi(400, "Sayfalama", "Gecersiz sayfa boyutu.", "pagination_invalid");
        var tenant = await TenantAsync(subeId, ct);
        var conn = db.Database.GetDbConnection();
        var args = new { TenantId = tenant, Search = string.IsNullOrWhiteSpace(arama) ? null : "%" + arama.Trim() + "%", AktifMi = aktifMi, Skip = (long)(page - 1) * pageSize, Take = pageSize };
        const string where = " FROM Kdv WHERE TenantId=@TenantId AND SilindiMi=0 AND (@AktifMi IS NULL OR AktifMi=@AktifMi) AND (@Search IS NULL OR Kod LIKE @Search OR Ad LIKE @Search)";
        var paging = (db.Database.ProviderName ?? "").Contains("Sqlite") ? " LIMIT @Take OFFSET @Skip" : " OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
        var count = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT COUNT(1)" + where, args, cancellationToken: ct));
        var rows = await conn.QueryAsync<KdvDto>(new CommandDefinition("SELECT Id, Kod, Ad, Oran, AktifMi" + where + " ORDER BY Id" + paging, args, cancellationToken: ct));
        return new() { Kayitlar = rows.ToList(), ToplamKayit = count, Sayfa = page, SayfaBoyutu = pageSize };
    }
    private async Task<Guid> TenantAsync(long subeId, CancellationToken ct) =>
        (await db.Subeler.YetkiliSube(db).SingleOrDefaultAsync(x => x.Id == subeId && x.AktifMi, ct)
        ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found")).TenantId;
    private async Task<Kdv> FindAsync(long id, long subeId, CancellationToken ct)
    {
        var tenant = await TenantAsync(subeId, ct);
        return await db.Kdvler.TenantKapsami(db).SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenant, ct)
            ?? throw new UygulamaHatasi(404, "KDV bulunamadi", "KDV bulunamadi.", "kdv_not_found");
    }
    private async Task ValidateAsync(KdvKaydetRequest r, Guid tenant, long? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Kod) || r.Kod.Trim().Length > 20 ||
            string.IsNullOrWhiteSpace(r.Ad) || r.Ad.Trim().Length > 50 ||
            r.Oran is < 0 or > 100 || decimal.Round(r.Oran, 2) != r.Oran)
            throw new UygulamaHatasi(400, "Gecersiz KDV", "Kod, ad veya oran gecersiz.", "kdv_invalid");
        var kod = r.Kod.Trim().ToUpperInvariant();
        if (r.AktifMi && await db.Kdvler.TenantKapsami(db).AnyAsync(x => x.TenantId == tenant && x.Id != id && x.AktifMi && (x.Kod == kod || x.Oran == r.Oran), ct))
            throw new UygulamaHatasi(409, "KDV tekrari", "Aktif KDV kodu veya orani tekrar edemez.", "kdv_duplicate");
    }
    private static KdvDto Map(Kdv k) => new() { Id = k.Id, Kod = k.Kod, Ad = k.Ad, Oran = k.Oran, AktifMi = k.AktifMi };
}
