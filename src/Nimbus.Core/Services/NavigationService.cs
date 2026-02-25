using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class NavigationService : INavigationService
{
    private readonly Stack<string> _back = new();
    private readonly Stack<string> _forward = new();

    public string? CurrentPath { get; private set; }

    public bool CanGoBack => _back.Count > 0;

    public bool CanGoForward => _forward.Count > 0;

    public void NavigateTo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (CurrentPath is not null && !string.Equals(CurrentPath, path, StringComparison.OrdinalIgnoreCase))
        {
            _back.Push(CurrentPath);
            _forward.Clear();
        }

        CurrentPath = path;
    }

    public void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }

        if (CurrentPath is not null)
        {
            _forward.Push(CurrentPath);
        }

        CurrentPath = _back.Pop();
    }

    public void GoForward()
    {
        if (!CanGoForward)
        {
            return;
        }

        if (CurrentPath is not null)
        {
            _back.Push(CurrentPath);
        }

        CurrentPath = _forward.Pop();
    }

    public IReadOnlyList<string> GetBreadcrumbSegments()
    {
        return GetBreadcrumbItems().Select(i => i.Label).ToArray();
    }

    public IReadOnlyList<BreadcrumbItem> GetBreadcrumbItems()
    {
        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            return Array.Empty<BreadcrumbItem>();
        }

        var segments = new List<BreadcrumbItem>();
        var pathRoot = Path.GetPathRoot(CurrentPath);

        if (!string.IsNullOrWhiteSpace(pathRoot))
        {
            var displayRoot = pathRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(displayRoot))
            {
                displayRoot = pathRoot;
            }

            segments.Add(new BreadcrumbItem(displayRoot, pathRoot));
        }

        var remainder = CurrentPath.Substring(pathRoot?.Length ?? 0);
        var parts = remainder.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);

        var current = pathRoot ?? string.Empty;
        foreach (var part in parts)
        {
            current = string.IsNullOrEmpty(current) ? part : Path.Combine(current, part);
            segments.Add(new BreadcrumbItem(part, current));
        }

        if (segments.Count == 0)
        {
            segments.Add(new BreadcrumbItem(CurrentPath, CurrentPath));
        }

        return segments;
    }
}
