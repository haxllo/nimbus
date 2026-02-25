namespace Nimbus.Core.Services;

public sealed class FileOperationsService : IFileOperationsService
{
    public Task<FileOperationResult> CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var validation = ValidateSourceAndDestination(sourcePath, destinationPath);
            if (validation is not null)
            {
                return Task.FromResult(validation);
            }

            var normalizedSource = sourcePath.Trim();
            var normalizedDestination = destinationPath.Trim();

            if (PathExists(normalizedDestination))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.Conflict,
                    $"Cannot copy because destination already exists: {normalizedDestination}"));
            }

            if (Directory.Exists(normalizedSource))
            {
                CopyDirectory(normalizedSource, normalizedDestination);
            }
            else
            {
                File.Copy(normalizedSource, normalizedDestination, overwrite: false);
            }

            return Task.FromResult(FileOperationResult.Success($"Copied to {normalizedDestination}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MapException("copy", ex));
        }
    }

    public Task<FileOperationResult> MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var validation = ValidateSourceAndDestination(sourcePath, destinationPath);
            if (validation is not null)
            {
                return Task.FromResult(validation);
            }

            var normalizedSource = sourcePath.Trim();
            var normalizedDestination = destinationPath.Trim();

            if (PathExists(normalizedDestination))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.Conflict,
                    $"Cannot move because destination already exists: {normalizedDestination}"));
            }

            if (Directory.Exists(normalizedSource))
            {
                Directory.Move(normalizedSource, normalizedDestination);
            }
            else
            {
                File.Move(normalizedSource, normalizedDestination, overwrite: false);
            }

            return Task.FromResult(FileOperationResult.Success($"Moved to {normalizedDestination}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MapException("move", ex));
        }
    }

    public Task<FileOperationResult> RenameAsync(string sourcePath, string newName, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(newName))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.InvalidInput,
                    "Source path and new name are required."));
            }

            if (newName.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                newName.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.InvalidInput,
                    "New name must not contain directory separators."));
            }

            var normalizedSource = sourcePath.Trim();
            if (!PathExists(normalizedSource))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.NotFound,
                    $"Source item was not found: {normalizedSource}"));
            }

            var parent = Path.GetDirectoryName(normalizedSource) ?? string.Empty;
            var destinationPath = Path.Combine(parent, newName.Trim());

            if (PathExists(destinationPath))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.Conflict,
                    $"Cannot rename because destination already exists: {destinationPath}"));
            }

            if (Directory.Exists(normalizedSource))
            {
                Directory.Move(normalizedSource, destinationPath);
            }
            else
            {
                File.Move(normalizedSource, destinationPath, overwrite: false);
            }

            return Task.FromResult(FileOperationResult.Success($"Renamed to {newName.Trim()}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MapException("rename", ex));
        }
    }

    public Task<FileOperationResult> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.InvalidInput,
                    "Path is required."));
            }

            var normalizedPath = path.Trim();
            if (!PathExists(normalizedPath))
            {
                return Task.FromResult(FileOperationResult.Failure(
                    FileOperationErrorCode.NotFound,
                    $"Item was not found: {normalizedPath}"));
            }

            if (Directory.Exists(normalizedPath))
            {
                Directory.Delete(normalizedPath, recursive: true);
            }
            else
            {
                File.Delete(normalizedPath);
            }

            return Task.FromResult(FileOperationResult.Success($"Deleted {normalizedPath}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MapException("delete", ex));
        }
    }

    internal static FileOperationResult MapException(string operation, Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => FileOperationResult.Failure(
                FileOperationErrorCode.Cancelled,
                $"The {operation} operation was cancelled."),

            UnauthorizedAccessException => FileOperationResult.Failure(
                FileOperationErrorCode.AccessDenied,
                $"Access denied during {operation} operation."),

            FileNotFoundException or DirectoryNotFoundException => FileOperationResult.Failure(
                FileOperationErrorCode.NotFound,
                $"Required item was not found during {operation} operation."),

            PathTooLongException or ArgumentException => FileOperationResult.Failure(
                FileOperationErrorCode.InvalidInput,
                $"Invalid path or name for {operation} operation."),

            IOException ioException => FileOperationResult.Failure(
                FileOperationErrorCode.IoError,
                $"I/O error during {operation} operation: {ioException.Message}"),

            _ => FileOperationResult.Failure(
                FileOperationErrorCode.Unknown,
                $"Unexpected error during {operation} operation: {exception.Message}")
        };
    }

    private static FileOperationResult? ValidateSourceAndDestination(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
        {
            return FileOperationResult.Failure(
                FileOperationErrorCode.InvalidInput,
                "Source and destination paths are required.");
        }

        var normalizedSource = sourcePath.Trim();
        if (!PathExists(normalizedSource))
        {
            return FileOperationResult.Failure(
                FileOperationErrorCode.NotFound,
                $"Source item was not found: {normalizedSource}");
        }

        return null;
    }

    private static bool PathExists(string path) =>
        Directory.Exists(path) || File.Exists(path);

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.EnumerateFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var dest = Path.Combine(destinationDir, fileName);
            File.Copy(file, dest, overwrite: true);
        }

        foreach (var dir in Directory.EnumerateDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            var dest = Path.Combine(destinationDir, dirName);
            CopyDirectory(dir, dest);
        }
    }
}
