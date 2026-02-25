using System.IO.Enumeration;

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
        var trimmedQuery = pattern.Trim();
        return Task.Run(() => SearchCore(trimmedRootPath, trimmedQuery, cancellationToken));
    }

    private static IReadOnlyList<string> SearchCore(
        string rootPath,
        string query,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(rootPath))
        {
            return Array.Empty<string>();
        }

        var hasWildcards = HasWildcards(query);
        var filePattern = hasWildcards ? query : "*";

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();
        stack.Push(rootPath);

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = stack.Pop();

            EnumerateFilesSafely(current, filePattern, cancellationToken, file =>
            {
                if (IsQueryMatch(file, query, hasWildcards))
                {
                    results.Add(file);
                }
            });

            EnumerateDirectoriesSafely(current, cancellationToken, directory =>
            {
                if (IsQueryMatch(directory, query, hasWildcards))
                {
                    results.Add(directory);
                }

                stack.Push(directory);
            });
        }

        return results
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool HasWildcards(string query) =>
        query.IndexOf('*') >= 0 || query.IndexOf('?') >= 0;

    private static bool IsQueryMatch(string path, string query, bool hasWildcards)
    {
        var name = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = path;
        }

        if (!hasWildcards)
        {
            return name.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        return FileSystemName.MatchesSimpleExpression(query, name, ignoreCase: true);
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
