namespace Nimbus.Core.Models;

public sealed record class NavigationState(
    string? CurrentPath,
    IReadOnlyList<string> BackHistory,
    IReadOnlyList<string> ForwardHistory)
{
    public static NavigationState Empty { get; } =
        new(null, Array.Empty<string>(), Array.Empty<string>());

    public static NavigationState FromCurrentPath(string? currentPath) =>
        new(currentPath, Array.Empty<string>(), Array.Empty<string>());
}
