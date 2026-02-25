using System.Text.Json;
using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class SavedSearchService : ISavedSearchService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _storagePath;
    private readonly bool _seedDefaults;
    private readonly object _gate = new();

    public SavedSearchService()
        : this(storagePath: null, seedDefaults: true)
    {
    }

    public SavedSearchService(string? storagePath, bool seedDefaults = true)
    {
        _storagePath = string.IsNullOrWhiteSpace(storagePath)
            ? BuildDefaultStoragePath()
            : storagePath;
        _seedDefaults = seedDefaults;
    }

    public IReadOnlyList<SavedSearchModel> GetAll()
    {
        lock (_gate)
        {
            return LoadAll()
                .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public SavedSearchModel Create(string displayName, string rootPath, string query)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path is required.", nameof(rootPath));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        var newSearch = new SavedSearchModel
        {
            Id = Guid.NewGuid().ToString("N"),
            DisplayName = displayName.Trim(),
            RootPath = rootPath.Trim(),
            Query = query.Trim()
        };

        lock (_gate)
        {
            var allSearches = LoadAll();
            allSearches.Add(newSearch);
            SaveAll(allSearches);
        }

        return newSearch;
    }

    public SavedSearchModel? Resolve(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        lock (_gate)
        {
            return LoadAll().FirstOrDefault(item =>
                string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }

    private List<SavedSearchModel> LoadAll()
    {
        if (!File.Exists(_storagePath))
        {
            var defaults = _seedDefaults ? BuildDefaultSearches() : new List<SavedSearchModel>();
            if (defaults.Count > 0)
            {
                SaveAll(defaults);
            }

            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_storagePath);
            var parsed = JsonSerializer.Deserialize<List<SavedSearchModel>>(json, JsonOptions);
            if (parsed is null)
            {
                return new List<SavedSearchModel>();
            }

            return parsed
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.Id) &&
                    !string.IsNullOrWhiteSpace(item.DisplayName) &&
                    !string.IsNullOrWhiteSpace(item.RootPath) &&
                    !string.IsNullOrWhiteSpace(item.Query))
                .ToList();
        }
        catch (IOException)
        {
            return new List<SavedSearchModel>();
        }
        catch (UnauthorizedAccessException)
        {
            return new List<SavedSearchModel>();
        }
        catch (JsonException)
        {
            return new List<SavedSearchModel>();
        }
    }

    private void SaveAll(IReadOnlyCollection<SavedSearchModel> searches)
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = JsonSerializer.Serialize(searches, JsonOptions);
        File.WriteAllText(_storagePath, payload);
    }

    private static List<SavedSearchModel> BuildDefaultSearches()
    {
        var defaults = new List<SavedSearchModel>();

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrWhiteSpace(documentsPath))
        {
            defaults.Add(new SavedSearchModel
            {
                Id = "default-documents-text",
                DisplayName = "Documents: Text Files",
                RootPath = documentsPath,
                Query = "*.txt"
            });
        }

        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        if (!string.IsNullOrWhiteSpace(picturesPath))
        {
            defaults.Add(new SavedSearchModel
            {
                Id = "default-pictures-images",
                DisplayName = "Pictures: Images",
                RootPath = picturesPath,
                Query = "*.png"
            });
        }

        return defaults;
    }

    private static string BuildDefaultStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(appData))
        {
            appData = Path.GetTempPath();
        }

        return Path.Combine(appData, "Nimbus", "saved-searches.json");
    }
}
