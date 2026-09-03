using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class StokKategoriServisi : IStokKategoriServisi
{
    private readonly PanoPosDbContext _dbContext;

    public StokKategoriServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StokKategoriDto> OlusturAsync(StokKategoriOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Kategori adi zorunludur.", "urun_kategori_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = NormalizeOptional(request.Kod);
        if (kod is not null && await _dbContext.StokKategorileri.AnyAsync(x => x.TenantId == sube.TenantId && x.Kod == kod, cancellationToken))
        {
            throw new UygulamaHatasi(409, "Kategori hatasi", "Ayni tenant icinde kategori kodu tekrar edemez.", "urun_kategori_duplicate");
        }

        var kategori = new StokKategori
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokKategorileri.Add(kategori);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StokKategoriDto
        {
            Id = kategori.Id,
            Ad = kategori.Ad,
            Kod = kategori.Kod
        };
    }

    public async Task<List<StokKategoriDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.StokKategorileri
            .OrderBy(x => x.Ad)
            .Select(x => new StokKategoriDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Kod = x.Kod
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
