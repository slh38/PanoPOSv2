using System.Xml.Linq;

namespace PanoPos.Desktop.Config;

public sealed class AppSettings
{
    public string BaseApiUrl { get; init; } = string.Empty;
    public long CihazId { get; init; }

    public static AppSettings Load()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "App.config");
        if (!File.Exists(configPath))
        {
            throw new InvalidOperationException("App.config bulunamadi.");
        }

        var document = XDocument.Load(configPath);
        var settings = document.Root?
            .Element("appSettings")?
            .Elements("add")
            .ToDictionary(
                x => x.Attribute("key")?.Value ?? string.Empty,
                x => x.Attribute("value")?.Value ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        if (settings is null)
        {
            throw new InvalidOperationException("App.config appSettings alani okunamadi.");
        }

        if (!settings.TryGetValue("BaseApiUrl", out var baseApiUrl) || string.IsNullOrWhiteSpace(baseApiUrl))
        {
            throw new InvalidOperationException("BaseApiUrl ayari zorunludur.");
        }

        if (!settings.TryGetValue("CihazId", out var cihazIdText) || !long.TryParse(cihazIdText, out var cihazId) || cihazId <= 0)
        {
            throw new InvalidOperationException("CihazId ayari pozitif sayi olmalidir.");
        }

        return new AppSettings
        {
            BaseApiUrl = baseApiUrl.TrimEnd('/'),
            CihazId = cihazId
        };
    }
}
