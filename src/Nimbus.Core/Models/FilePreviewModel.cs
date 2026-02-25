namespace Nimbus.Core.Models;

public sealed record class FilePreviewModel
{
    public string Name { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public bool IsFolder { get; init; }

    public string ItemType { get; init; } = string.Empty;

    public DateTimeOffset? DateModified { get; init; }

    public long? SizeBytes { get; init; }

    public string? TextPreview { get; init; }

    public bool IsImage { get; init; }

    public string? ImagePath { get; init; }

    public string? ErrorMessage { get; init; }
}
