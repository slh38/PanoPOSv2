using PanoPos.Desktop.Config;
using PanoPos.Desktop.Services;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settings = AppSettings.Load();
        var apiClient = new ApiClient(settings);
        var authService = new AuthService(apiClient, settings, AppSession.Current);
        var hizliSatisService = new HizliSatisService(apiClient, AppSession.Current);
        var tahsilatService = new TahsilatService(apiClient);

        Application.Run(new DesktopApplicationContext(authService, hizliSatisService, tahsilatService));
    }
}
