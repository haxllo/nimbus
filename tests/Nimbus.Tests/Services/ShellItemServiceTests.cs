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
}
