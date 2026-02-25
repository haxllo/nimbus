using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class SearchServiceTests
{
    [Fact]
    public async Task Search_Returns_Matches()
    {
        var svc = new SearchService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-search-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var file = Path.Combine(temp, "nimbus-search.txt");
            await File.WriteAllTextAsync(file, "x");

            var results = await svc.SearchAsync(temp, "*.txt");

            Assert.Contains(file, results);
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Search_Plain_Text_Query_Matches_File_Name()
    {
        var svc = new SearchService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-search-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var file = Path.Combine(temp, "project-notes.md");
            await File.WriteAllTextAsync(file, "x");

            var results = await svc.SearchAsync(temp, "notes");

            Assert.Contains(file, results);
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Search_Missing_Root_Returns_Empty()
    {
        var svc = new SearchService();
        var missing = Path.Combine(Path.GetTempPath(), $"nimbus-missing-{Guid.NewGuid():N}");

        var results = await svc.SearchAsync(missing, "*.txt");

        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_Plain_Text_Query_Includes_Matching_Directories()
    {
        var svc = new SearchService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-search-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var matchingDirectory = Path.Combine(temp, "notes-folder");
            Directory.CreateDirectory(matchingDirectory);
            await File.WriteAllTextAsync(Path.Combine(matchingDirectory, "inside.txt"), "x");

            var results = await svc.SearchAsync(temp, "notes");

            Assert.Contains(matchingDirectory, results);
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }
}
