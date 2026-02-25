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
        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            return Array.Empty<string>();
        }

        var segments = new List<string>();
        var pathRoot = Path.GetPathRoot(CurrentPath);

        if (!string.IsNullOrWhiteSpace(pathRoot))
        {
            segments.Add(pathRoot.TrimEnd(Path.DirectorySeparatorChar));
        }

        var remainder = CurrentPath.Substring(pathRoot?.Length ?? 0);
        var parts = remainder.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        segments.AddRange(parts);
        return segments;
    }
}
