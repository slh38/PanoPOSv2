using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Product;

public sealed class UrunGrupServisi : IUrunGrupServisi
{
    private readonly PanoPosDbContext _dbContext;

    public UrunGrupServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UrunGrupDto> OlusturAsync(UrunGrupOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Grup adi zorunludur.", "urun_grup_required");
        }

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        var kod = NormalizeOptional(request.Kod);
        if (kod is not null && await _dbContext.UrunGruplari.AnyAsync(x => x.TenantId == sube.TenantId && x.Kod == kod, cancellationToken))
        {
            throw new UygulamaHatasi(409, "Grup hatasi", "Ayni tenant icinde grup kodu tekrar edemez.", "urun_grup_duplicate");
        }

        var grup = new UrunGrup
        {
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            Ad = request.Ad.Trim(),
            Kod = kod,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.UrunGruplari.Add(grup);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UrunGrupDto
        {
            Id = grup.Id,
            Ad = grup.Ad,
            Kod = grup.Kod
        };
    }

    public async Task<List<UrunGrupDto>> ListeleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrunGruplari
            .OrderBy(x => x.Ad)
            .Select(x => new UrunGrupDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Kod = x.Kod
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
