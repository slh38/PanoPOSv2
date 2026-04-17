using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Restaurant;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Restaurant;

public sealed class MasaGrupServisi : IMasaGrupServisi
{
    private readonly PanoPosDbContext _dbContext;

    public MasaGrupServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MasaGrupDto> OlusturAsync(MasaGrupOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Masa grup adi bos olamaz.", "masa_grup_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = NormalizeOptional(request.Kod);
        var masaGrup = new MasaGrup
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.MasaGruplari.Add(masaGrup);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MasaGrupDto
        {
            Id = masaGrup.Id,
            Ad = masaGrup.Ad,
            Kod = masaGrup.Kod
        };
    }

    public async Task<List<MasaGrupDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MasaGruplari
            .AsNoTracking()
            .OrderBy(x => x.Ad)
            .Select(x => new MasaGrupDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Kod = x.Kod
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
