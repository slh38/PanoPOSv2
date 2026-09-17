using Microsoft.EntityFrameworkCore;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Auth;

public sealed class OturumDogrulamaServisi(PanoPosDbContext db, IslemBaglami baglam)
{
    public async Task DogrulaAsync(string token, CancellationToken ct = default)
    {
        if (token.Length != 64 || !token.All(Uri.IsHexDigit)) throw IslemBaglami.Yetkisiz();
        var hash = OturumToken.Hash(token);
        var session = await db.KullaniciOturumlari.AsNoTracking().SingleOrDefaultAsync(x =>
            x.OturumTokenHash == hash && x.AktifMi && x.CikisTarihi == null, ct);
        if (session == null) throw IslemBaglami.Yetkisiz();
        var tenant = session.TenantId;
        var valid = await db.Tenantler.AnyAsync(x => x.TenantId == tenant && x.AktifMi, ct)
            && await db.Kullanicilar.AnyAsync(x => x.Id == session.KullaniciId && x.TenantId == tenant && x.AktifMi && !x.KilitliMi, ct)
            && await db.Subeler.AnyAsync(x => x.Id == session.SubeId && x.TenantId == tenant && x.AktifMi, ct)
            && await db.Cihazlar.AnyAsync(x => x.Id == session.CihazId && x.TenantId == tenant && x.SubeId == session.SubeId && x.AktifMi, ct)
            && await db.KullaniciSubeleri.AnyAsync(x => x.KullaniciId == session.KullaniciId && x.BagliSubeId == session.SubeId && x.TenantId == tenant && x.AktifMi, ct);
        if (!valid) throw IslemBaglami.Yetkisiz();
        baglam.Ata(tenant, session.SubeId, session.KullaniciId, session.CihazId, session.Id);
    }
}
