using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class ShellItemService : IShellItemService
{
    public Task<IReadOnlyList<ShellItemModel>> EnumerateAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }

        var trimmedPath = path.Trim();
        if (!Directory.Exists(trimmedPath))
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }

        var items = new List<ShellItemModel>();
        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(trimmedPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var model = TryBuildModel(entry);
                if (model is not null)
                {
                    items.Add(model);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }
        catch (DirectoryNotFoundException)
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }
        catch (IOException)
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }
        catch (PathTooLongException)
        {
            return Task.FromResult<IReadOnlyList<ShellItemModel>>(Array.Empty<ShellItemModel>());
        }

        var orderedItems = items
            .OrderByDescending(i => i.IsFolder)
            .ThenBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<ShellItemModel>>(orderedItems);
    }

    private static string GetDisplayName(string path)
    {
        var name = Path.GetFileName(path);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return path;
    }

    private static ShellItemModel? TryBuildModel(string entry)
    {
        try
        {
            var isFolder = Directory.Exists(entry);
            var model = new ShellItemModel(entry)
            {
                DisplayName = GetDisplayName(entry),
                IsFolder = isFolder
            };

            if (isFolder)
            {
                var info = new DirectoryInfo(entry);
                model.DateModified = info.Exists ? info.LastWriteTimeUtc : null;
                model.SizeBytes = null;
                return model;
            }

            var fileInfo = new FileInfo(entry);
            if (!fileInfo.Exists)
            {
                return null;
            }

            model.DateModified = fileInfo.LastWriteTimeUtc;
            model.SizeBytes = fileInfo.Length;
            return model;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (PathTooLongException)
        {
            return null;
        }
    }
}
