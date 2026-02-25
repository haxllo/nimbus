namespace Nimbus.Core.Models;

public sealed class SidebarLocation
{
    public SidebarLocation(string id, string displayName, string path, string? iconKey = null)
    {
        Id = id;
        DisplayName = displayName;
        Path = path;
        IconKey = iconKey;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Path { get; }

    public string? IconKey { get; }
}
