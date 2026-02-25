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

            EnumerateFilesSafely(current, filePattern, cancellationToken, file =>
            {
                if (hasWildcards || IsPlainTextMatch(file, trimmedQuery))
                {
                    results.Add(file);
                }
            });

            EnumerateDirectoriesSafely(current, cancellationToken, dir =>
            {
                stack.Push(dir);
            });
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

    private static void EnumerateFilesSafely(
        string directory,
        string pattern,
        CancellationToken cancellationToken,
        Action<string> onFile)
    {
        IEnumerator<string>? enumerator;
        try
        {
            enumerator = Directory
                .EnumerateFiles(directory, pattern, DirectoryEnumerationOptions)
                .GetEnumerator();
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }
        catch (PathTooLongException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }
        catch (ArgumentException)
        {
            return;
        }

        using (enumerator)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    onFile(enumerator.Current);
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    break;
                }
                catch (PathTooLongException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
                catch (ArgumentException)
                {
                    break;
                }
            }
        }
    }

    private static void EnumerateDirectoriesSafely(
        string directory,
        CancellationToken cancellationToken,
        Action<string> onDirectory)
    {
        IEnumerator<string>? enumerator;
        try
        {
            enumerator = Directory
                .EnumerateDirectories(directory, "*", DirectoryEnumerationOptions)
                .GetEnumerator();
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }
        catch (PathTooLongException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }
        catch (ArgumentException)
        {
            return;
        }

        using (enumerator)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    onDirectory(enumerator.Current);
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    break;
                }
                catch (PathTooLongException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
                catch (ArgumentException)
                {
                    break;
                }
            }
        }
    }
}
