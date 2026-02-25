using Nimbus.Core.Models;
using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class SidebarViewModel
{
    private readonly ISavedSearchService _savedSearchService;

    public SidebarViewModel(ISavedSearchService savedSearchService)
    {
        _savedSearchService = savedSearchService;
        Locations = BuildDefaultLocations();
        SavedSearches = _savedSearchService.GetAll();
    }

    public IReadOnlyList<SidebarLocation> Locations { get; }

    public IReadOnlyList<SavedSearchModel> SavedSearches { get; private set; }

    public void RefreshSavedSearches()
    {
        SavedSearches = _savedSearchService.GetAll();
    }

    private static IReadOnlyList<SidebarLocation> BuildDefaultLocations()
    {
        var list = new List<SidebarLocation>
        {
            CreateKnownFolder("Home", Environment.SpecialFolder.UserProfile),
            CreateKnownFolder("Documents", Environment.SpecialFolder.MyDocuments),
            CreateKnownFolder("Downloads", Environment.SpecialFolder.UserProfile, "Downloads"),
            CreateKnownFolder("Pictures", Environment.SpecialFolder.MyPictures),
            CreateKnownFolder("Music", Environment.SpecialFolder.MyMusic),
            CreateKnownFolder("Videos", Environment.SpecialFolder.MyVideos),
        };

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            list.Add(new SidebarLocation($"Drive-{drive.Name}", drive.Name.TrimEnd(Path.DirectorySeparatorChar), drive.RootDirectory.FullName));
        }

        return list;
    }

    private static SidebarLocation CreateKnownFolder(string id, Environment.SpecialFolder folder)
    {
        var path = Environment.GetFolderPath(folder);
        return new SidebarLocation(id, id, path);
    }

    private static SidebarLocation CreateKnownFolder(string id, Environment.SpecialFolder root, string child)
    {
        var rootPath = Environment.GetFolderPath(root);
        var path = Path.Combine(rootPath, child);
        return new SidebarLocation(id, id, path);
    }
}
