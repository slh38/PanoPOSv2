using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class BedenServisi : IBedenServisi
{
    private readonly PanoPosDbContext _dbContext;

    public BedenServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BedenDto> OlusturAsync(BedenOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad) || string.IsNullOrWhiteSpace(request.Kod))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Beden adi ve kodu zorunludur.", "beden_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = request.Kod.Trim();
        if (await _dbContext.Bedenler.AnyAsync(x => x.TenantId == sube.TenantId && x.Kod == kod, cancellationToken))
        {
            throw new UygulamaHatasi(409, "Beden hatasi", "Ayni tenant icinde ayni beden tekrar edemez.", "beden_duplicate");
        }

        var beden = new Beden
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Bedenler.Add(beden);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BedenDto { Id = beden.Id, Ad = beden.Ad, Kod = beden.Kod };
    }

    public async Task<List<BedenDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bedenler.OrderBy(x => x.Ad).Select(x => new BedenDto { Id = x.Id, Ad = x.Ad, Kod = x.Kod }).ToListAsync(cancellationToken);
    }
}
