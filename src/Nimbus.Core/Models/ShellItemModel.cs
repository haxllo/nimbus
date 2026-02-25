namespace Nimbus.Core.Models;

public sealed class ShellItemModel
{
    public ShellItemModel(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public string DisplayName { get; set; } = "";

    public bool IsFolder { get; set; }

    public long? SizeBytes { get; set; }

    public DateTimeOffset? DateModified { get; set; }

    public string? IconKey { get; set; }

    public string IconGlyph { get; set; } = "\uE8A5";

    public string? ThumbnailPath { get; set; }
}
