using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Audit;
using PanoPos.Application.Auth;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Auth;

public sealed class AuthIslemLogServisi : IAuthIslemLogServisi
{
    private readonly IIslemLogServisi _islemLogServisi;
    private readonly PanoPosDbContext _dbContext;

    public AuthIslemLogServisi(IIslemLogServisi islemLogServisi, PanoPosDbContext dbContext)
    {
        _islemLogServisi = islemLogServisi;
        _dbContext = dbContext;
    }

    public async Task LoginBasariliAsync(long kullaniciId, long cihazId, long oturumId, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(kullaniciId, cihazId, cancellationToken);
        await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = context.TenantId,
            SubeId = context.SubeId,
            CihazId = cihazId,
            KullaniciId = kullaniciId,
            ModulAdi = "Auth",
            EkranAdi = "Login",
            ButonAdi = "Giris",
            IslemTipi = "Login",
            HedefTablo = nameof(KullaniciOturum),
            HedefId = oturumId,
            Aciklama = "Kullanici girisi basarili.",
            BasariliMi = true
        }, cancellationToken);
    }

    public async Task LoginBasarisizAsync(long? kullaniciId, long cihazId, string neden, CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(kullaniciId, cihazId, cancellationToken);
        await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = context.TenantId,
            SubeId = context.SubeId,
            CihazId = cihazId,
            KullaniciId = kullaniciId,
            ModulAdi = "Auth",
            EkranAdi = "Login",
            ButonAdi = "Giris",
            IslemTipi = "Login",
            HedefTablo = nameof(KullaniciOturum),
            Aciklama = neden,
            BasariliMi = false,
            HataKodu = "login_failed",
            HataMesaji = neden
        }, cancellationToken);
    }

    public async Task LogoutAsync(long kullaniciId, long kullaniciOturumId, CancellationToken cancellationToken = default)
    {
        var oturum = await _dbContext.KullaniciOturumlari.FindAsync([kullaniciOturumId], cancellationToken)
            ?? throw new InvalidOperationException("Kullanici oturumu bulunamadi.");

        await _islemLogServisi.LogEkleAsync(new IslemLogEkleRequestDto
        {
            TenantId = oturum.TenantId,
            SubeId = oturum.SubeId,
            CihazId = oturum.CihazId,
            KullaniciId = kullaniciId,
            ModulAdi = "Auth",
            EkranAdi = "Logout",
            ButonAdi = "Cikis",
            IslemTipi = "Logout",
            HedefTablo = nameof(KullaniciOturum),
            HedefId = kullaniciOturumId,
            Aciklama = "Kullanici cikisi basarili.",
            BasariliMi = true
        }, cancellationToken);
    }

    private async Task<(Guid TenantId, long SubeId)> GetContextAsync(long? kullaniciId, long cihazId, CancellationToken cancellationToken)
    {
        var cihaz = await _dbContext.Cihazlar.AsNoTracking().SingleOrDefaultAsync(x => x.Id == cihazId, cancellationToken)
            ?? throw new InvalidOperationException("Cihaz bulunamadi.");

        if (!kullaniciId.HasValue)
        {
            return (cihaz.TenantId, cihaz.SubeId);
        }

        var kullanici = await _dbContext.Kullanicilar.AsNoTracking().SingleOrDefaultAsync(x => x.Id == kullaniciId.Value, cancellationToken);
        return kullanici is null ? (cihaz.TenantId, cihaz.SubeId) : (kullanici.TenantId, cihaz.SubeId);
    }
}
