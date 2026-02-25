using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class FilePreviewService : IFilePreviewService
{
    private const int MaxPreviewChars = 4096;
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".json", ".xml", ".yaml", ".yml", ".ini", ".log", ".csv", ".cs", ".ts", ".js", ".html", ".css"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tif", ".tiff", ".ico"
    };

    public async Task<FilePreviewModel> GetPreviewAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new FilePreviewModel
            {
                ErrorMessage = "No item selected."
            };
        }

        cancellationToken.ThrowIfCancellationRequested();
        var trimmedPath = path.Trim();

        if (Directory.Exists(trimmedPath))
        {
            return BuildDirectoryPreview(trimmedPath);
        }

        if (!File.Exists(trimmedPath))
        {
            return new FilePreviewModel
            {
                Name = Path.GetFileName(trimmedPath),
                Path = trimmedPath,
                ErrorMessage = "Item not found."
            };
        }

        return await BuildFilePreviewAsync(trimmedPath, cancellationToken);
    }

    private static FilePreviewModel BuildDirectoryPreview(string directoryPath)
    {
        try
        {
            var info = new DirectoryInfo(directoryPath);
            return new FilePreviewModel
            {
                Name = info.Name,
                Path = info.FullName,
                IsFolder = true,
                ItemType = "Folder",
                DateModified = info.Exists ? info.LastWriteTimeUtc : null
            };
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException or
            IOException or
            PathTooLongException)
        {
            return new FilePreviewModel
            {
                Name = Path.GetFileName(directoryPath),
                Path = directoryPath,
                IsFolder = true,
                ItemType = "Folder",
                ErrorMessage = ex.Message
            };
        }
    }

    private static async Task<FilePreviewModel> BuildFilePreviewAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var info = new FileInfo(filePath);
            var extension = info.Extension;
            var preview = new FilePreviewModel
            {
                Name = info.Name,
                Path = info.FullName,
                IsFolder = false,
                ItemType = string.IsNullOrWhiteSpace(extension) ? "File" : extension.ToUpperInvariant(),
                DateModified = info.Exists ? info.LastWriteTimeUtc : null,
                SizeBytes = info.Exists ? info.Length : null
            };

            if (ImageExtensions.Contains(extension))
            {
                return preview with
                {
                    IsImage = true,
                    ImagePath = info.FullName
                };
            }

            if (TextExtensions.Contains(extension))
            {
                var textPreview = await ReadTextPreviewAsync(info.FullName, cancellationToken);
                return preview with
                {
                    TextPreview = textPreview
                };
            }

            return preview;
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException or
            IOException or
            PathTooLongException)
        {
            return new FilePreviewModel
            {
                Name = Path.GetFileName(filePath),
                Path = filePath,
                IsFolder = false,
                ItemType = "File",
                ErrorMessage = ex.Message
            };
        }
    }

    private static async Task<string> ReadTextPreviewAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 4096,
            options: FileOptions.SequentialScan);

        using var reader = new StreamReader(stream);
        var buffer = new char[MaxPreviewChars];
        var readCount = await reader.ReadBlockAsync(buffer.AsMemory(0, MaxPreviewChars), cancellationToken);
        var content = new string(buffer, 0, readCount);

        if (reader.EndOfStream)
        {
            return content;
        }

        return $"{content}\n…";
    }
}
