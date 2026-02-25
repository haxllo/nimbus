using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.ViewModels;

public class MainPageViewModelOperationsTests
{
    [Fact]
    public async Task CreateFolderAsync_When_CurrentPath_Matches_Reloads_FileList()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-ops-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.NavigateToAsync(tempRoot);

            var result = await viewModel.CreateFolderAsync(tempRoot, "New Folder");

            Assert.True(result.IsSuccess);
            Assert.Contains(viewModel.FileList.Items, i => i.DisplayName == "New Folder" && i.IsFolder);
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
    public async Task RenameItemAsync_When_CurrentPath_Matches_Reloads_FileList()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-ops-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var sourcePath = Path.Combine(tempRoot, "old-name.txt");
        await File.WriteAllTextAsync(sourcePath, "x");

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.NavigateToAsync(tempRoot);

            var result = await viewModel.RenameItemAsync(sourcePath, "new-name.txt");

            Assert.True(result.IsSuccess);
            Assert.Contains(viewModel.FileList.Items, i => i.DisplayName == "new-name.txt");
            Assert.DoesNotContain(viewModel.FileList.Items, i => i.DisplayName == "old-name.txt");
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
    public async Task DeleteItemAsync_When_CurrentPath_Matches_Reloads_FileList()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-ops-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var filePath = Path.Combine(tempRoot, "delete-me.txt");
        await File.WriteAllTextAsync(filePath, "x");

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.NavigateToAsync(tempRoot);

            var result = await viewModel.DeleteItemAsync(filePath);

            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(viewModel.FileList.Items, i => i.DisplayName == "delete-me.txt");
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static MainPageViewModel CreateViewModel()
    {
        var shellItemService = new ShellItemService();
        var viewPreferenceService = new ViewPreferenceService();
        var fileListViewModel = new FileListViewModel(shellItemService, viewPreferenceService);
        var navigationViewModel = new NavigationViewModel(new NavigationService());
        var sidebarViewModel = new SidebarViewModel();
        var fileOperationsService = new FileOperationsService();
        return new MainPageViewModel(sidebarViewModel, fileListViewModel, navigationViewModel, fileOperationsService);
    }
}
