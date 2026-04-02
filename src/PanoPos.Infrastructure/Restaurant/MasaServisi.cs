using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Restaurant;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Infrastructure.Restaurant;

public sealed class MasaServisi : IMasaServisi
{
    private readonly PanoPosDbContext _dbContext;

    public MasaServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MasaDto> MasaOlusturAsync(MasaOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(request.Kod) || string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Masa kodu ve adi bos olamaz.", "masa_required_fields");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var masa = new Masa
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Kod = request.Kod.Trim(),
            Ad = request.Ad.Trim(),
            MasaDurumId = SystemSeedData.MasaDurumBosId,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Masalar.Add(masa);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Masalar
            .AsNoTracking()
            .Where(x => x.Id == masa.Id)
            .Select(x => new MasaDto
            {
                Id = x.Id,
                SubeId = x.SubeId,
                Kod = x.Kod,
                Ad = x.Ad,
                MasaDurumId = x.MasaDurumId,
                MasaDurumAd = x.MasaDurum!.Ad,
                AktifMi = x.AktifMi
            })
            .SingleAsync(cancellationToken);
    }

    public async Task<List<MasaDto>> MasaListeleAsync(long subeId, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        return await _dbContext.Masalar
            .AsNoTracking()
            .Where(x => x.SubeId == subeId)
            .OrderBy(x => x.Ad)
            .Select(x => new MasaDto
            {
                Id = x.Id,
                SubeId = x.SubeId,
                Kod = x.Kod,
                Ad = x.Ad,
                MasaDurumId = x.MasaDurumId,
                MasaDurumAd = x.MasaDurum!.Ad,
                AktifMi = x.AktifMi
            })
            .ToListAsync(cancellationToken);
    }
}
