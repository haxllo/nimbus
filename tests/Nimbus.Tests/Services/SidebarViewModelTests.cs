using Nimbus.Core.Models;
using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.Services;

public class SidebarViewModelTests
{
    [Fact]
    public void Sidebar_Includes_Known_Folders()
    {
        var vm = new SidebarViewModel(new InMemorySavedSearchService(), new InMemoryTagService());
        Assert.Contains(vm.Locations, l => l.Id == "Documents");
    }

    [Fact]
    public void Sidebar_Exposes_Saved_Searches_From_Service()
    {
        var vm = new SidebarViewModel(new InMemorySavedSearchService(
            new SavedSearchModel
            {
                Id = "saved-1",
                DisplayName = "Reports",
                RootPath = @"C:\Temp",
                Query = "*.csv"
            }),
            new InMemoryTagService());

        var saved = Assert.Single(vm.SavedSearches);
        Assert.Equal("Reports", saved.DisplayName);
    }

    [Fact]
    public void Sidebar_Exposes_Tags_From_Service()
    {
        var vm = new SidebarViewModel(
            new InMemorySavedSearchService(),
            new InMemoryTagService(
                new FileTagModel
                {
                    Id = "work",
                    DisplayName = "Work",
                    RootPath = @"C:\Temp",
                    Query = "*.docx"
                }));

        var tag = Assert.Single(vm.Tags);
        Assert.Equal("Work", tag.DisplayName);
        Assert.Equal("*.docx", tag.Query);
    }

    private sealed class InMemorySavedSearchService : ISavedSearchService
    {
        private readonly List<SavedSearchModel> _searches;

        public InMemorySavedSearchService(params SavedSearchModel[] initialSearches)
        {
            _searches = initialSearches.ToList();
        }

        public IReadOnlyList<SavedSearchModel> GetAll() => _searches.ToArray();

        public SavedSearchModel Create(string displayName, string rootPath, string query)
        {
            var created = new SavedSearchModel
            {
                Id = Guid.NewGuid().ToString("N"),
                DisplayName = displayName,
                RootPath = rootPath,
                Query = query
            };
            _searches.Add(created);
            return created;
        }

        public SavedSearchModel? Resolve(string id) =>
            _searches.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class InMemoryTagService : ITagService
    {
        private readonly List<FileTagModel> _tags;

        public InMemoryTagService(params FileTagModel[] initialTags)
        {
            _tags = initialTags.ToList();
        }

        public IReadOnlyList<FileTagModel> GetAll() => _tags.ToArray();

        public FileTagModel? Resolve(string id) =>
            _tags.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
