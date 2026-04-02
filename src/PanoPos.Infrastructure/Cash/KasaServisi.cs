using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Cash;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Cash;

public sealed class KasaServisi : IKasaServisi
{
    private readonly PanoPosDbContext _dbContext;

    public KasaServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<KasaDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Kasalar
            .OrderBy(x => x.Ad)
            .Select(x => new KasaDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Aciklama = x.Aciklama,
                AktifMi = x.AktifMi
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<KasaDto> OlusturAsync(KasaOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Kasa adi zorunludur.", "kasa_ad_required");
        }

        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kasa = new Kasa
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(request.Aciklama) ? null : request.Aciklama.Trim(),
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Kasalar.Add(kasa);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new KasaDto
        {
            Id = kasa.Id,
            Ad = kasa.Ad,
            Aciklama = kasa.Aciklama,
            AktifMi = kasa.AktifMi
        };
    }
}
