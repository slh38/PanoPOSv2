using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Session;

public sealed class AppSession
{
    public static AppSession Current { get; } = new();

    private AppSession()
    {
    }

    public long KullaniciId { get; private set; }
    public string AdSoyad { get; private set; } = string.Empty;
    public long CihazId { get; private set; }
    public long OturumId { get; private set; }
    public long VarsayilanSubeId { get; private set; }
    public List<string> Roller { get; private set; } = [];
    public List<SubeModel> Subeler { get; private set; } = [];
    public bool IsAuthenticated => KullaniciId > 0 && OturumId > 0;

    public void Fill(LoginResponseModel response)
    {
        KullaniciId = response.KullaniciId;
        AdSoyad = response.AdSoyad;
        CihazId = response.CihazId;
        OturumId = response.OturumId;
        VarsayilanSubeId = response.VarsayilanSubeId;
        Roller = response.Roller.ToList();
        Subeler = response.Subeler.ToList();
    }

    public void Clear()
    {
        KullaniciId = 0;
        AdSoyad = string.Empty;
        CihazId = 0;
        OturumId = 0;
        VarsayilanSubeId = 0;
        Roller = [];
        Subeler = [];
    }
}
