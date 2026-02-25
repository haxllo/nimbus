using Nimbus.Core.Models;
using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.ViewModels;

public class FileListViewModelTests
{
    [Fact]
    public void CurrentViewMode_Defaults_To_List()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(FileViewMode.List, viewModel.CurrentViewMode);
    }

    [Fact]
    public async Task SetViewModeForCurrentPath_Persists_Mode_For_Same_Path()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-filelist-vm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var preferenceService = new ViewPreferenceService();
            var first = CreateViewModel(preferenceService);
            await first.LoadAsync(tempRoot);
            first.SetViewModeForCurrentPath(FileViewMode.Icon);

            var second = CreateViewModel(preferenceService);
            await second.LoadAsync(tempRoot);

            Assert.Equal(FileViewMode.Icon, second.CurrentViewMode);
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
    public async Task LoadAsync_Uses_Path_Specific_ViewMode()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-filelist-vm-{Guid.NewGuid():N}");
        var firstPath = Path.Combine(tempRoot, "first");
        var secondPath = Path.Combine(tempRoot, "second");
        Directory.CreateDirectory(firstPath);
        Directory.CreateDirectory(secondPath);

        try
        {
            var preferenceService = new ViewPreferenceService();
            var viewModel = CreateViewModel(preferenceService);

            await viewModel.LoadAsync(firstPath);
            viewModel.SetViewModeForCurrentPath(FileViewMode.Column);

            await viewModel.LoadAsync(secondPath);
            Assert.Equal(FileViewMode.List, viewModel.CurrentViewMode);

            viewModel.SetViewModeForCurrentPath(FileViewMode.Gallery);
            await viewModel.LoadAsync(firstPath);

            Assert.Equal(FileViewMode.Column, viewModel.CurrentViewMode);
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
    public async Task LoadPreviewForSelectionAsync_Loads_Preview_For_Selected_Item()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-filelist-vm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var filePath = Path.Combine(tempRoot, "preview.txt");
        await File.WriteAllTextAsync(filePath, "preview-content");

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(tempRoot);
            viewModel.SelectedItem = viewModel.Items.First(i => string.Equals(i.Path, filePath, StringComparison.OrdinalIgnoreCase));

            await viewModel.LoadPreviewForSelectionAsync();

            Assert.NotNull(viewModel.CurrentPreview);
            Assert.Equal("preview.txt", viewModel.CurrentPreview.Name);
            Assert.Contains("preview-content", viewModel.CurrentPreview.TextPreview);
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
    public async Task SetSort_By_Size_Descending_Reorders_Files()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-filelist-vm-sort-size-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        await File.WriteAllTextAsync(Path.Combine(tempRoot, "small.txt"), "1");
        await File.WriteAllTextAsync(Path.Combine(tempRoot, "large.txt"), new string('x', 20));

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(tempRoot);

            viewModel.SetSort(FileSortField.Size, descending: true);
            var files = viewModel.Items.Where(item => !item.IsFolder).ToArray();

            Assert.Equal("large.txt", files[0].DisplayName);
            Assert.Equal("small.txt", files[1].DisplayName);
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
    public async Task SetSort_By_Date_Ascending_Reorders_Files()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-filelist-vm-sort-date-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var olderPath = Path.Combine(tempRoot, "older.txt");
        var newerPath = Path.Combine(tempRoot, "newer.txt");
        await File.WriteAllTextAsync(olderPath, "a");
        await File.WriteAllTextAsync(newerPath, "b");
        File.SetLastWriteTimeUtc(olderPath, DateTime.UtcNow.AddMinutes(-10));
        File.SetLastWriteTimeUtc(newerPath, DateTime.UtcNow.AddMinutes(-1));

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(tempRoot);

            viewModel.SetSort(FileSortField.DateModified, descending: false);
            var files = viewModel.Items.Where(item => !item.IsFolder).ToArray();

            Assert.Equal("older.txt", files[0].DisplayName);
            Assert.Equal("newer.txt", files[1].DisplayName);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static FileListViewModel CreateViewModel(ViewPreferenceService? preferenceService = null)
    {
        var shellItemService = new ShellItemService();
        var resolvedPreferenceService = preferenceService ?? new ViewPreferenceService();
        var filePreviewService = new FilePreviewService();
        return new FileListViewModel(shellItemService, resolvedPreferenceService, filePreviewService);
    }
}
