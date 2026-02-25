using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public sealed class TagService : ITagService
{
    private readonly IReadOnlyList<FileTagModel> _tags;

    public TagService()
    {
        _tags = BuildDefaultTags()
            .OrderBy(tag => tag.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<FileTagModel> GetAll() => _tags;

    public FileTagModel? Resolve(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return _tags.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private static List<FileTagModel> BuildDefaultTags()
    {
        var tags = new List<FileTagModel>();

        AddTag(tags, "work", "Work", GetFolderPath(Environment.SpecialFolder.MyDocuments), "*.docx", "\uE8A5");
        AddTag(tags, "media", "Media", GetFolderPath(Environment.SpecialFolder.MyPictures), "*.png", "\uEB9F");
        AddTag(tags, "archive", "Archive", GetDownloadsPath(), "*.zip", "\uE7B8");
        AddTag(tags, "source", "Source", GetFolderPath(Environment.SpecialFolder.UserProfile), "*.cs", "\uE943");

        return tags;
    }

    private static void AddTag(
        List<FileTagModel> list,
        string id,
        string displayName,
        string? rootPath,
        string query,
        string iconGlyph)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            return;
        }

        list.Add(new FileTagModel
        {
            Id = id,
            DisplayName = displayName,
            RootPath = rootPath,
            Query = query,
            IconGlyph = iconGlyph
        });
    }

    private static string? GetFolderPath(Environment.SpecialFolder folder)
    {
        var path = Environment.GetFolderPath(folder);
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path;
    }

    private static string? GetDownloadsPath()
    {
        var home = GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home))
        {
            return null;
        }

        return Path.Combine(home, "Downloads");
    }
}
