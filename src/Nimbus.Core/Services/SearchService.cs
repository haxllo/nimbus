namespace Nimbus.Core.Services;

public sealed class SearchService : ISearchService
{
    private static readonly EnumerationOptions DirectoryEnumerationOptions = new()
    {
        RecurseSubdirectories = false,
        IgnoreInaccessible = true,
        ReturnSpecialDirectories = false
    };

    public Task<IReadOnlyList<string>> SearchAsync(string rootPath, string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(pattern))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var trimmedRootPath = rootPath.Trim();
        if (!Directory.Exists(trimmedRootPath))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var trimmedQuery = pattern.Trim();
        var hasWildcards = HasWildcards(trimmedQuery);
        var filePattern = hasWildcards ? trimmedQuery : "*";

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();
        stack.Push(trimmedRootPath);

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = stack.Pop();

            foreach (var file in EnumerateFilesSafely(current, filePattern))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (hasWildcards || IsPlainTextMatch(file, trimmedQuery))
                {
                    results.Add(file);
                }
            }

            foreach (var dir in EnumerateDirectoriesSafely(current))
            {
                cancellationToken.ThrowIfCancellationRequested();
                stack.Push(dir);
            }
        }

        var orderedResults = results
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(orderedResults);
    }

    private static bool HasWildcards(string query) =>
        query.IndexOf('*') >= 0 || query.IndexOf('?') >= 0;

    private static bool IsPlainTextMatch(string path, string query)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateFilesSafely(string directory, string pattern)
    {
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(directory, pattern, DirectoryEnumerationOptions);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (PathTooLongException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (ArgumentException)
        {
            yield break;
        }

        try
        {
            foreach (var file in files)
            {
                yield return file;
            }
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (PathTooLongException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (ArgumentException)
        {
            yield break;
        }
    }

    private static IEnumerable<string> EnumerateDirectoriesSafely(string directory)
    {
        IEnumerable<string> directories;
        try
        {
            directories = Directory.EnumerateDirectories(directory, "*", DirectoryEnumerationOptions);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (PathTooLongException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (ArgumentException)
        {
            yield break;
        }

        try
        {
            foreach (var childDirectory in directories)
            {
                yield return childDirectory;
            }
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (PathTooLongException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (ArgumentException)
        {
            yield break;
        }
    }
}
