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

    private static FileListViewModel CreateViewModel(ViewPreferenceService? preferenceService = null)
    {
        var shellItemService = new ShellItemService();
        var resolvedPreferenceService = preferenceService ?? new ViewPreferenceService();
        return new FileListViewModel(shellItemService, resolvedPreferenceService);
    }
}
