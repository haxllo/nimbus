using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class RegressionServiceTests
{
    [Fact]
    public async Task ShellItemService_EnumerateAsync_LargeFolder_Remains_Deterministic()
    {
        const int folderCount = 120;
        const int fileCount = 600;

        var svc = new ShellItemService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-regression-shell-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            for (var i = 0; i < folderCount; i++)
            {
                Directory.CreateDirectory(Path.Combine(tempRoot, $"folder-{i:D3}"));
            }

            for (var i = 0; i < fileCount; i++)
            {
                var filePath = Path.Combine(tempRoot, $"file-{i:D4}.txt");
                await File.WriteAllTextAsync(filePath, "x");
            }

            var items = await svc.EnumerateAsync(tempRoot);

            Assert.Equal(folderCount + fileCount, items.Count);
            Assert.All(items.Take(folderCount), i => Assert.True(i.IsFolder));
            Assert.All(items.Skip(folderCount), i => Assert.False(i.IsFolder));
            Assert.Equal(
                Enumerable.Range(0, folderCount).Select(i => $"folder-{i:D3}"),
                items.Take(folderCount).Select(i => i.DisplayName));
            Assert.Equal(
                Enumerable.Range(0, fileCount).Select(i => $"file-{i:D4}.txt"),
                items.Skip(folderCount).Select(i => i.DisplayName));
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
    public async Task SearchService_SearchAsync_LargeTree_Returns_Sorted_Matches_For_Wildcard_And_PlainText()
    {
        const int directoryCount = 24;
        const int noiseFileCountPerDirectory = 24;

        var svc = new SearchService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-regression-search-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var expectedMatches = new List<string>(directoryCount);

        try
        {
            for (var i = 0; i < directoryCount; i++)
            {
                var folder = Path.Combine(tempRoot, $"section-{i:D2}");
                Directory.CreateDirectory(folder);

                for (var j = 0; j < noiseFileCountPerDirectory; j++)
                {
                    var noiseFile = Path.Combine(folder, $"noise-{i:D2}-{j:D2}.log");
                    await File.WriteAllTextAsync(noiseFile, "x");
                }

                var target = Path.Combine(folder, $"target-{i:D2}-report.txt");
                await File.WriteAllTextAsync(target, "x");
                expectedMatches.Add(target);
            }

            var expectedSorted = expectedMatches
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var wildcardResults = await svc.SearchAsync(tempRoot, "*target-*-report.txt");
            var plainTextResults = await svc.SearchAsync(tempRoot, "target-");

            Assert.Equal(expectedSorted, wildcardResults);
            Assert.Equal(expectedSorted, plainTextResults);
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
    public async Task SearchService_SearchAsync_CancelledToken_Throws_OperationCanceledException()
    {
        var svc = new SearchService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-regression-cancel-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        await File.WriteAllTextAsync(Path.Combine(tempRoot, "a.txt"), "x");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => svc.SearchAsync(tempRoot, "*.txt", cts.Token));
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
