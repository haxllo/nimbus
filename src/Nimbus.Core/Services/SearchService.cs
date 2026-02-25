namespace Nimbus.Core.Services;

public sealed class SearchService : ISearchService
{
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

        var results = new List<string>();
        var stack = new Stack<string>();
        stack.Push(trimmedRootPath);

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = stack.Pop();

            foreach (var file in SafeEnumerateFiles(current, filePattern))
            {
                if (hasWildcards || IsPlainTextMatch(file, trimmedQuery))
                {
                    results.Add(file);
                }
            }

            foreach (var dir in SafeEnumerateDirectories(current))
            {
                stack.Push(dir);
            }
        }

        var orderedResults = results
            .Distinct(StringComparer.OrdinalIgnoreCase)
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

    private static IEnumerable<string> SafeEnumerateFiles(string directory, string pattern)
    {
        try
        {
            return Directory.EnumerateFiles(directory, pattern).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch (PathTooLongException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
        catch (ArgumentException)
        {
            return Array.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string directory)
    {
        try
        {
            return Directory.EnumerateDirectories(directory).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch (PathTooLongException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
        catch (ArgumentException)
        {
            return Array.Empty<string>();
        }
    }
}
