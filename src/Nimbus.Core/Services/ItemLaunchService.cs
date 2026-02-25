using System.ComponentModel;
using System.Diagnostics;

namespace Nimbus.Core.Services;

public sealed class ItemLaunchService : IItemLaunchService
{
    public Task<FileOperationResult> LaunchAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.InvalidInput,
                    "Select an item to open."));
            }

            var normalizedPath = path.Trim();
            if (!Directory.Exists(normalizedPath) && !File.Exists(normalizedPath))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.NotFound,
                    $"Item was not found: {normalizedPath}"));
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = normalizedPath,
                UseShellExecute = true,
                Verb = "open"
            };

            var process = Process.Start(startInfo);
            if (process is null)
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.Unknown,
                    $"Unable to open item: {normalizedPath}"));
            }

            var name = Path.GetFileName(normalizedPath);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = normalizedPath;
            }

            return Task.FromResult(FileOperationResult.Success($"Opened {name}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MapException(ex));
        }
    }

    private static FileOperationResult MapException(Exception exception)
    {
        return exception switch
        {
            System.OperationCanceledException => FileOperationResult.Failure(
                FileOperationErrorCode.Cancelled,
                "Open operation was cancelled."),

            UnauthorizedAccessException => FileOperationResult.Failure(
                FileOperationErrorCode.AccessDenied,
                "Access denied while opening item."),

            FileNotFoundException or DirectoryNotFoundException => FileOperationResult.Failure(
                FileOperationErrorCode.NotFound,
                "Item was not found while opening item."),

            PathTooLongException or ArgumentException or NotSupportedException => FileOperationResult.Failure(
                FileOperationErrorCode.InvalidInput,
                "Invalid path while opening item."),

            Win32Exception win32Exception => FileOperationResult.Failure(
                FileOperationErrorCode.Unknown,
                $"Unable to open item: {win32Exception.Message}"),

            InvalidOperationException invalidOperationException => FileOperationResult.Failure(
                FileOperationErrorCode.Unknown,
                $"Unable to open item: {invalidOperationException.Message}"),

            _ => FileOperationResult.Failure(
                FileOperationErrorCode.Unknown,
                $"Unexpected error while opening item: {exception.Message}")
        };
    }
}
