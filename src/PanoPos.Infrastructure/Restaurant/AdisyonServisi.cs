using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Restaurant;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Infrastructure.Restaurant;

public sealed class AdisyonServisi : IAdisyonServisi
{
    private readonly PanoPosDbContext _dbContext;

    public AdisyonServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdisyonDto> AdisyonAcAsync(AdisyonAcRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.MasaId <= 0 || request.AcanKullaniciId <= 0 || request.AcanCihazId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "MasaId, AcanKullaniciId ve AcanCihazId zorunludur.", "adisyon_open_invalid_request");
        }

        if (request.KisiSayisi.HasValue && request.KisiSayisi.Value <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "KisiSayisi 0'dan buyuk olmalidir.", "adisyon_guest_count_invalid");
        }

        var masa = await _dbContext.Masalar.SingleOrDefaultAsync(x => x.Id == request.MasaId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Masa bulunamadi", "Masa bulunamadi.", "masa_not_found");

        if (!masa.AktifMi)
        {
            throw new UygulamaHatasi(409, "Adisyon acilamadi", "Pasif masa icin adisyon acilamaz.", "masa_pasif");
        }

        var kullaniciVar = await _dbContext.Kullanicilar.AnyAsync(x => x.Id == request.AcanKullaniciId && x.AktifMi, cancellationToken);
        if (!kullaniciVar)
        {
            throw new UygulamaHatasi(404, "Kullanici bulunamadi", "Kullanici bulunamadi.", "kullanici_not_found");
        }

        var cihazVar = await _dbContext.Cihazlar.AnyAsync(x => x.Id == request.AcanCihazId && x.AktifMi, cancellationToken);
        if (!cihazVar)
        {
            throw new UygulamaHatasi(404, "Cihaz bulunamadi", "Cihaz bulunamadi.", "cihaz_not_found");
        }

        var acikAdisyonVar = await _dbContext.Adisyonlar.AnyAsync(x => x.MasaId == request.MasaId && x.Durum == AdisyonDurumu.Acik, cancellationToken);
        if (acikAdisyonVar)
        {
            throw new UygulamaHatasi(409, "Adisyon acilamadi", "Ayni masada ayni anda sadece 1 acik adisyon olabilir.", "masa_open_check_exists");
        }

        var adisyon = new Domain.Entities.Adisyon
        {
            TenantId = masa.TenantId,
            SubeId = masa.SubeId,
            MasaId = masa.Id,
            AcanKullaniciId = request.AcanKullaniciId,
            AcanCihazId = request.AcanCihazId,
            KisiSayisi = request.KisiSayisi,
            AcilisTarihi = DateTime.UtcNow,
            Durum = AdisyonDurumu.Acik,
            Aciklama = string.IsNullOrWhiteSpace(request.Aciklama) ? null : request.Aciklama.Trim(),
            AktifMi = true,
            SilindiMi = false
        };

        masa.MasaDurumId = SystemSeedData.MasaDurumDoluId;

        _dbContext.Adisyonlar.Add(adisyon);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetDtoAsync(adisyon.Id, cancellationToken);
    }

    public async Task<AdisyonDto> AdisyonKapatAsync(AdisyonKapatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.AdisyonId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "AdisyonId zorunludur.", "adisyon_id_required");
        }

        var adisyon = await _dbContext.Adisyonlar
            .Include(x => x.Masa)
            .SingleOrDefaultAsync(x => x.Id == request.AdisyonId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Adisyon bulunamadi", "Adisyon bulunamadi.", "adisyon_not_found");

        if (adisyon.Durum != AdisyonDurumu.Acik || !adisyon.AktifMi)
        {
            throw new UygulamaHatasi(409, "Adisyon kapatilamadi", "Acik adisyon bulunamadi.", "open_adisyon_not_found");
        }

        adisyon.Durum = AdisyonDurumu.Kapali;
        adisyon.KapanisTarihi = DateTime.UtcNow;
        adisyon.AktifMi = false;

        if (adisyon.Masa is not null)
        {
            adisyon.Masa.MasaDurumId = SystemSeedData.MasaDurumBosId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetDtoAsync(adisyon.Id, cancellationToken);
    }

    public async Task<AdisyonDto?> AcikAdisyonGetirAsync(long masaId, CancellationToken cancellationToken = default)
    {
        if (masaId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "MasaId zorunludur.", "masa_required");
        }

        var adisyonId = await _dbContext.Adisyonlar
            .AsNoTracking()
            .Where(x => x.MasaId == masaId && x.Durum == AdisyonDurumu.Acik)
            .Select(x => (long?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        return adisyonId.HasValue ? await GetDtoAsync(adisyonId.Value, cancellationToken) : null;
    }

    private Task<AdisyonDto> GetDtoAsync(long adisyonId, CancellationToken cancellationToken)
    {
        return _dbContext.Adisyonlar
            .AsNoTracking()
            .Where(x => x.Id == adisyonId)
            .Select(x => new AdisyonDto
            {
                Id = x.Id,
                MasaId = x.MasaId,
                MasaAd = x.Masa!.Ad,
                AcanKullaniciId = x.AcanKullaniciId,
                AcanCihazId = x.AcanCihazId,
                KisiSayisi = x.KisiSayisi,
                AcilisTarihi = x.AcilisTarihi,
                KapanisTarihi = x.KapanisTarihi,
                Durum = x.Durum,
                Aciklama = x.Aciklama,
                AktifMi = x.AktifMi
            })
            .SingleAsync(cancellationToken);
    }
}
