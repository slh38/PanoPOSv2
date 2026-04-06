using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Payment;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Payment;

public sealed class BankaServisi : IBankaServisi
{
    private readonly PanoPosDbContext _dbContext;

    public BankaServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BankaDto> BankaOlusturAsync(BankaOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Banka adi bos olamaz.", "banka_ad_required");
        }

        if (string.IsNullOrWhiteSpace(request.Kod))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Banka kodu bos olamaz.", "banka_kod_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = request.Kod.Trim();
        var ayniKodVar = await _dbContext.Bankalar.AnyAsync(x => x.TenantId == sube.TenantId && x.SubeId == request.SubeId && x.Kod == kod, cancellationToken);
        if (ayniKodVar)
        {
            throw new UygulamaHatasi(409, "Banka hatasi", "Ayni subede banka kodu tekrar edemez.", "banka_kod_duplicate");
        }

        var banka = new Banka
        {
            TenantId = sube.TenantId,
            SubeId = request.SubeId,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = request.AktifMi,
            SilindiMi = false
        };

        _dbContext.Bankalar.Add(banka);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BankaDto
        {
            Id = banka.Id,
            SubeId = banka.SubeId,
            Ad = banka.Ad,
            Kod = banka.Kod,
            AktifMi = banka.AktifMi
        };
    }

    public async Task<List<BankaDto>> BankaListeleAsync(long subeId, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        var subeVar = await _dbContext.Subeler.AnyAsync(x => x.Id == subeId, cancellationToken);
        if (!subeVar)
        {
            throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
        }

        return await _dbContext.Bankalar
            .AsNoTracking()
            .Where(x => x.SubeId == subeId)
            .OrderBy(x => x.Ad)
            .Select(x => new BankaDto
            {
                Id = x.Id,
                SubeId = x.SubeId,
                Ad = x.Ad,
                Kod = x.Kod,
                AktifMi = x.AktifMi
            })
            .ToListAsync(cancellationToken);
    }
}
