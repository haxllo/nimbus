using System.Text.Json;
using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class PaneLayoutService : IPaneLayoutService
{
    private const double DefaultSidebarWidth = 260;
    private const double DefaultPreviewWidth = 300;
    private const double MinSidebarWidth = 180;
    private const double MaxSidebarWidth = 520;
    private const double MinPreviewWidth = 220;
    private const double MaxPreviewWidth = 620;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _storagePath;
    private readonly object _gate = new();

    public PaneLayoutService()
        : this(storagePath: null)
    {
    }

    public PaneLayoutService(string? storagePath)
    {
        _storagePath = string.IsNullOrWhiteSpace(storagePath)
            ? BuildDefaultStoragePath()
            : storagePath;
    }

    public PaneLayoutModel GetLayout()
    {
        lock (_gate)
        {
            if (!File.Exists(_storagePath))
            {
                return new PaneLayoutModel();
            }

            try
            {
                var json = File.ReadAllText(_storagePath);
                var parsed = JsonSerializer.Deserialize<PaneLayoutModel>(json, JsonOptions);
                if (parsed is null)
                {
                    return new PaneLayoutModel();
                }

                return new PaneLayoutModel
                {
                    SidebarWidth = ClampSidebar(parsed.SidebarWidth),
                    PreviewWidth = ClampPreview(parsed.PreviewWidth)
                };
            }
            catch (IOException)
            {
                return new PaneLayoutModel();
            }
            catch (UnauthorizedAccessException)
            {
                return new PaneLayoutModel();
            }
            catch (JsonException)
            {
                return new PaneLayoutModel();
            }
        }
    }

    public void SaveLayout(double sidebarWidth, double previewWidth)
    {
        var payload = new PaneLayoutModel
        {
            SidebarWidth = ClampSidebar(sidebarWidth),
            PreviewWidth = ClampPreview(previewWidth)
        };

        lock (_gate)
        {
            var directory = Path.GetDirectoryName(_storagePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            File.WriteAllText(_storagePath, json);
        }
    }

    private static double ClampSidebar(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            return DefaultSidebarWidth;
        }

        return Math.Clamp(value, MinSidebarWidth, MaxSidebarWidth);
    }

    private static double ClampPreview(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            return DefaultPreviewWidth;
        }

        return Math.Clamp(value, MinPreviewWidth, MaxPreviewWidth);
    }

    private static string BuildDefaultStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(appData))
        {
            appData = Path.GetTempPath();
        }

        return Path.Combine(appData, "Nimbus", "pane-layout.json");
    }
}
