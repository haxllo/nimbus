using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class ViewPreferenceService : IViewPreferenceService
{
    private readonly Dictionary<string, FileViewMode> _modeByPath = new(StringComparer.OrdinalIgnoreCase);

    public FileViewMode GetViewMode(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return FileViewMode.List;
        }

        var normalizedPath = NormalizePath(path);
        return _modeByPath.TryGetValue(normalizedPath, out var mode)
            ? mode
            : FileViewMode.List;
    }

    public void SetViewMode(string path, FileViewMode viewMode)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var normalizedPath = NormalizePath(path);
        _modeByPath[normalizedPath] = viewMode;
    }

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (
            ex is ArgumentException or
            NotSupportedException or
            PathTooLongException)
        {
            return trimmed;
        }
    }
}
