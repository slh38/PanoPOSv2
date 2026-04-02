using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class RenkServisi : IRenkServisi
{
    private readonly PanoPosDbContext _dbContext;

    public RenkServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RenkDto> OlusturAsync(RenkOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad) || string.IsNullOrWhiteSpace(request.Kod))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Renk adi ve kodu zorunludur.", "renk_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = request.Kod.Trim();
        if (await _dbContext.Renkler.AnyAsync(x => x.TenantId == sube.TenantId && x.Kod == kod, cancellationToken))
        {
            throw new UygulamaHatasi(409, "Renk hatasi", "Ayni tenant icinde ayni renk tekrar edemez.", "renk_duplicate");
        }

        var renk = new Renk
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Renkler.Add(renk);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RenkDto { Id = renk.Id, Ad = renk.Ad, Kod = renk.Kod };
    }

    public async Task<List<RenkDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Renkler.OrderBy(x => x.Ad).Select(x => new RenkDto { Id = x.Id, Ad = x.Ad, Kod = x.Kod }).ToListAsync(cancellationToken);
    }
}
