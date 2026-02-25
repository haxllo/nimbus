using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class ShellItemServiceTests
{
    [Fact]
    public async Task EnumerateFolder_Returns_Items_With_Folders_First_Then_Alphabetical()
    {
        var svc = new ShellItemService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-shell-items-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            Directory.CreateDirectory(Path.Combine(tempRoot, "z-folder"));
            Directory.CreateDirectory(Path.Combine(tempRoot, "a-folder"));
            await File.WriteAllTextAsync(Path.Combine(tempRoot, "b-file.txt"), "b");
            await File.WriteAllTextAsync(Path.Combine(tempRoot, "a-file.txt"), "a");

            var items = await svc.EnumerateAsync(tempRoot);

            Assert.Equal(
                new[] { "a-folder", "z-folder", "a-file.txt", "b-file.txt" },
                items.Select(i => i.DisplayName));
            Assert.All(items.Take(2), i => Assert.True(i.IsFolder));
            Assert.All(items.Skip(2), i => Assert.False(i.IsFolder));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task EnumerateFolder_Missing_Path_Returns_Empty()
    {
        var svc = new ShellItemService();
        var missingPath = Path.Combine(Path.GetTempPath(), $"nimbus-missing-{Guid.NewGuid():N}");

        var items = await svc.EnumerateAsync(missingPath);

        Assert.Empty(items);
    }

    [Fact]
    public async Task EnumerateFolder_Assigns_Icon_Metadata_And_Image_Thumbnail_Path()
    {
        var svc = new ShellItemService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-shell-icons-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            Directory.CreateDirectory(Path.Combine(tempRoot, "folder"));
            var imagePath = Path.Combine(tempRoot, "photo.png");
            await File.WriteAllTextAsync(imagePath, "x");

            var items = await svc.EnumerateAsync(tempRoot);

            var folder = Assert.Single(items.Where(item => item.IsFolder));
            Assert.Equal("folder", folder.IconKey);
            Assert.Equal("\uE8B7", folder.IconGlyph);
            Assert.Null(folder.ThumbnailPath);

            var image = Assert.Single(items.Where(item => string.Equals(item.DisplayName, "photo.png", StringComparison.OrdinalIgnoreCase)));
            Assert.Equal(".png", image.IconKey);
            Assert.Equal("\uEB9F", image.IconGlyph);
            Assert.Equal(imagePath, image.ThumbnailPath);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
