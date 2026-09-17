using PanoPos.Application.Auth;
using PanoPos.Application.Common;

namespace PanoPos.Infrastructure.Auth;

public sealed class IslemBaglami : IIslemBaglami
{
    public bool Dogrulandi { get; private set; }
    public Guid TenantId { get; private set; }
    public long SubeId { get; private set; }
    public long KullaniciId { get; private set; }
    public long CihazId { get; private set; }
    public long KullaniciOturumId { get; private set; }

    internal void Ata(Guid tenantId, long subeId, long kullaniciId, long cihazId, long oturumId)
    {
        if (Dogrulandi) throw new InvalidOperationException("Islem baglami tekrar atanamaz.");
        TenantId = tenantId; SubeId = subeId; KullaniciId = kullaniciId;
        CihazId = cihazId; KullaniciOturumId = oturumId; Dogrulandi = true;
    }

    public static UygulamaHatasi Yetkisiz() => new(401, "Oturum gerekli", "Gecerli bir oturumla giris yapin.", "session_invalid");
}
