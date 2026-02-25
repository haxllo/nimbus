using Nimbus.Core.Models;
using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class SidebarViewModel
{
    private readonly ISavedSearchService _savedSearchService;
    private readonly ITagService _tagService;

    public SidebarViewModel(ISavedSearchService savedSearchService, ITagService tagService)
    {
        _savedSearchService = savedSearchService;
        _tagService = tagService;
        Locations = BuildDefaultLocations();
        SavedSearches = _savedSearchService.GetAll();
        Tags = _tagService.GetAll();
    }

    public IReadOnlyList<SidebarLocation> Locations { get; }

    public IReadOnlyList<SavedSearchModel> SavedSearches { get; private set; }

    public IReadOnlyList<FileTagModel> Tags { get; private set; }

    public void RefreshSavedSearches()
    {
        SavedSearches = _savedSearchService.GetAll();
    }

    public void RefreshTags()
    {
        Tags = _tagService.GetAll();
    }

    private static IReadOnlyList<SidebarLocation> BuildDefaultLocations()
    {
        var list = new List<SidebarLocation>
        {
            CreateKnownFolder("Home", Environment.SpecialFolder.UserProfile, "\uE80F"),
            CreateKnownFolder("Documents", Environment.SpecialFolder.MyDocuments, "\uE8A5"),
            CreateKnownFolder("Downloads", Environment.SpecialFolder.UserProfile, "Downloads", "\uE896"),
            CreateKnownFolder("Pictures", Environment.SpecialFolder.MyPictures, "\uEB9F"),
            CreateKnownFolder("Music", Environment.SpecialFolder.MyMusic, "\uE189"),
            CreateKnownFolder("Videos", Environment.SpecialFolder.MyVideos, "\uE714"),
        };

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            list.Add(new SidebarLocation(
                $"Drive-{drive.Name}",
                drive.Name.TrimEnd(Path.DirectorySeparatorChar),
                drive.RootDirectory.FullName,
                "\uEDA2"));
        }

        return list;
    }

    private static SidebarLocation CreateKnownFolder(string id, Environment.SpecialFolder folder, string iconGlyph)
    {
        var path = Environment.GetFolderPath(folder);
        return new SidebarLocation(id, id, path, iconGlyph);
    }

    private static SidebarLocation CreateKnownFolder(string id, Environment.SpecialFolder root, string child, string iconGlyph)
    {
        var rootPath = Environment.GetFolderPath(root);
        var path = Path.Combine(rootPath, child);
        return new SidebarLocation(id, id, path, iconGlyph);
    }
}
