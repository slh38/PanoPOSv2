using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class StokGrupServisi : IStokGrupServisi
{
    private readonly PanoPosDbContext _dbContext;

    public StokGrupServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StokGrupDto> OlusturAsync(StokGrupOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Grup adi zorunludur.", "stok_grup_required");
        }

        var sube = await _dbContext.Subeler.YetkiliSube(_dbContext).SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = NormalizeOptional(request.Kod);
        if (kod is not null && await _dbContext.StokGruplari.TenantKapsami(_dbContext).AnyAsync(x => x.TenantId == sube.TenantId && x.Kod == kod, cancellationToken))
        {
            throw new UygulamaHatasi(409, "Grup hatasi", "Ayni tenant icinde grup kodu tekrar edemez.", "stok_grup_duplicate");
        }

        var grup = new StokGrup
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.StokGruplari.Add(grup);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StokGrupDto
        {
            Id = grup.Id,
            Ad = grup.Ad,
            Kod = grup.Kod
        };
    }

    public async Task<List<StokGrupDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.StokGruplari.TenantKapsami(_dbContext)
            .OrderBy(x => x.Ad)
            .Select(x => new StokGrupDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Kod = x.Kod
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
