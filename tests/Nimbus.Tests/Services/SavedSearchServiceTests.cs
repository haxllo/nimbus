using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class SavedSearchServiceTests
{
    [Fact]
    public void Create_Persists_Saved_Search_And_GetAll_Returns_It()
    {
        using var fixture = new SavedSearchFixture();
        var service = fixture.CreateService(seedDefaults: false);

        var created = service.Create("Reports", fixture.RootPath, "*.csv");
        var all = service.GetAll();

        var persisted = Assert.Single(all);
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal("Reports", persisted.DisplayName);
        Assert.Equal(fixture.RootPath, persisted.RootPath);
        Assert.Equal("*.csv", persisted.Query);
    }

    [Fact]
    public void Resolve_Returns_Search_By_Id()
    {
        using var fixture = new SavedSearchFixture();
        var service = fixture.CreateService(seedDefaults: false);

        var created = service.Create("Images", fixture.RootPath, "*.png");
        var resolved = service.Resolve(created.Id);

        Assert.NotNull(resolved);
        Assert.Equal(created.Id, resolved.Id);
        Assert.Equal("Images", resolved.DisplayName);
    }

    [Fact]
    public void GetAll_When_File_Missing_And_Seed_Disabled_Returns_Empty()
    {
        using var fixture = new SavedSearchFixture();
        var service = fixture.CreateService(seedDefaults: false);

        var all = service.GetAll();

        Assert.Empty(all);
    }

    private sealed class SavedSearchFixture : IDisposable
    {
        private readonly string _tempRoot;

        public SavedSearchFixture()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-saved-search-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempRoot);
            RootPath = Path.Combine(_tempRoot, "root");
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public SavedSearchService CreateService(bool seedDefaults)
        {
            var storagePath = Path.Combine(_tempRoot, "saved-searches.json");
            return new SavedSearchService(storagePath, seedDefaults);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }
    }
}
