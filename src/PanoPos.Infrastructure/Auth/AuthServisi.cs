using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Auth;

public sealed class AuthServisi : IAuthServisi
{
    private readonly PanoPosDbContext _dbContext;
    private readonly IPinHashServisi _pinHashServisi;
    private readonly IAuthIslemLogServisi _authIslemLogServisi;

    public AuthServisi(
        PanoPosDbContext dbContext,
        IPinHashServisi pinHashServisi,
        IAuthIslemLogServisi authIslemLogServisi)
    {
        _dbContext = dbContext;
        _pinHashServisi = pinHashServisi;
        _authIslemLogServisi = authIslemLogServisi;
    }

    public async Task<LoginResponseDto> LoginAsync(string pin, long cihazId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "PIN bos olamaz.", "pin_required");
        }

        if (cihazId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "CihazId zorunludur.", "cihaz_required");
        }

        var cihaz = await _dbContext.Cihazlar
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Id == cihazId, cancellationToken);

        if (cihaz is null || !cihaz.AktifMi)
        {
            throw new UygulamaHatasi(400, "Giris basarisiz", "Cihaz aktif degil veya bulunamadi.", "cihaz_invalid");
        }

        var adayKullanicilar = await _dbContext.Kullanicilar
            .Where(x => x.TenantId == cihaz.TenantId)
            .ToListAsync(cancellationToken);

        var eslesenKullanicilar = adayKullanicilar
            .Where(x => _pinHashServisi.Dogrula(pin, x.PinHash))
            .ToList();

        if (eslesenKullanicilar.Count == 0)
        {
            await _authIslemLogServisi.LoginBasarisizAsync(null, cihazId, "Yanlis PIN", cancellationToken);
            throw new UygulamaHatasi(401, "Giris basarisiz", "PIN hatali.", "invalid_pin");
        }

        if (eslesenKullanicilar.Count > 1)
        {
            throw new UygulamaHatasi(409, "Veri hatasi", "Ayni tenant icinde ayni PIN birden fazla kullanicida bulunuyor.", "duplicate_pin");
        }

        var eslesenKullanici = eslesenKullanicilar[0];

        if (!eslesenKullanici.AktifMi)
        {
            await _authIslemLogServisi.LoginBasarisizAsync(eslesenKullanici.Id, cihazId, "Pasif kullanici", cancellationToken);
            throw new UygulamaHatasi(403, "Giris basarisiz", "Pasif kullanici giris yapamaz.", "user_passive");
        }

        if (eslesenKullanici.KilitliMi)
        {
            await _authIslemLogServisi.LoginBasarisizAsync(eslesenKullanici.Id, cihazId, "Kilitli kullanici", cancellationToken);
            throw new UygulamaHatasi(403, "Giris basarisiz", "Kilitli kullanici giris yapamaz.", "user_locked");
        }

        var kullanici = await _dbContext.Kullanicilar
            .Include(x => x.KullaniciRoller.Where(y => y.AktifMi))
                .ThenInclude(x => x.Rol)
            .Include(x => x.KullaniciSubeler.Where(y => y.AktifMi))
                .ThenInclude(x => x.Sube)
            .SingleAsync(x => x.Id == eslesenKullanici.Id, cancellationToken);

        var yetkiliSubeler = kullanici.KullaniciSubeler
            .Where(x => x.Sube.AktifMi)
            .Select(x => x.Sube)
            .DistinctBy(x => x.Id)
            .ToList();

        if (yetkiliSubeler.Count == 0)
        {
            await _authIslemLogServisi.LoginBasarisizAsync(kullanici.Id, cihazId, "Sube yetkisi yok", cancellationToken);
            throw new UygulamaHatasi(403, "Giris basarisiz", "Kullanici icin yetkili sube bulunamadi.", "sube_not_authorized");
        }

        if (yetkiliSubeler.All(x => x.Id != cihaz.SubeId))
        {
            await _authIslemLogServisi.LoginBasarisizAsync(kullanici.Id, cihazId, "Cihaz subesi yetkisiz", cancellationToken);
            throw new UygulamaHatasi(403, "Giris basarisiz", "Kullanici bu sube icin yetkili degil.", "sube_not_authorized");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var simdikiZaman = DateTime.UtcNow;
        var aktifOturumlar = await _dbContext.KullaniciOturumlari
            .Where(x => x.KullaniciId == kullanici.Id && x.AktifMi && x.CikisTarihi == null)
            .ToListAsync(cancellationToken);

        foreach (var aktifOturum in aktifOturumlar)
        {
            aktifOturum.CikisTarihi = simdikiZaman;
            aktifOturum.AktifMi = false;
        }

        var yeniOturum = new KullaniciOturum
        {
            TenantId = kullanici.TenantId,
            SubeId = cihaz.SubeId,
            KullaniciId = kullanici.Id,
            CihazId = cihaz.Id,
            GirisTarihi = simdikiZaman,
            AktifMi = true,
            SilindiMi = false
        };

        kullanici.SonGirisTarihi = simdikiZaman;
        kullanici.BasarisizGirisSayisi = 0;

        _dbContext.KullaniciOturumlari.Add(yeniOturum);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _authIslemLogServisi.LoginBasariliAsync(kullanici.Id, cihaz.Id, yeniOturum.Id, cancellationToken);

        return new LoginResponseDto
        {
            KullaniciId = kullanici.Id,
            AdSoyad = $"{kullanici.Ad} {kullanici.Soyad}".Trim(),
            VarsayilanSubeId = cihaz.SubeId,
            CihazId = cihaz.Id,
            OturumId = yeniOturum.Id,
            Roller = kullanici.KullaniciRoller
                .Where(x => x.Rol.AktifMi)
                .Select(x => x.Rol.Ad)
                .Distinct()
                .OrderBy(x => x)
                .ToList(),
            Subeler = yetkiliSubeler
                .OrderBy(x => x.Ad)
                .Select(x => new SubeBilgisiDto
                {
                    SubeId = x.Id,
                    Ad = x.Ad
                })
                .ToList()
        };
    }

    public async Task LogoutAsync(long kullaniciOturumId, CancellationToken cancellationToken = default)
    {
        if (kullaniciOturumId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "KullaniciOturumId zorunludur.", "oturum_required");
        }

        var oturum = await _dbContext.KullaniciOturumlari
            .SingleOrDefaultAsync(x => x.Id == kullaniciOturumId, cancellationToken);

        if (oturum is null || !oturum.AktifMi || oturum.CikisTarihi is not null)
        {
            throw new UygulamaHatasi(404, "Oturum bulunamadi", "Aktif oturum bulunamadi.", "session_not_found");
        }

        oturum.CikisTarihi = DateTime.UtcNow;
        oturum.AktifMi = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _authIslemLogServisi.LogoutAsync(oturum.KullaniciId, oturum.Id, cancellationToken);
    }
}
