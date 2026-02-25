using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.ViewModels;

public class MainPageViewModelTests
{
    [Fact]
    public async Task NavigateToAsync_Existing_Path_Updates_CurrentPath_And_Loads_Items()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var fileName = "item.txt";
        await File.WriteAllTextAsync(Path.Combine(tempRoot, fileName), "x");

        try
        {
            var viewModel = CreateViewModel();

            var result = await viewModel.NavigateToAsync(tempRoot);

            Assert.True(result);
            Assert.Equal(tempRoot, viewModel.Navigation.CurrentPath);
            Assert.Contains(viewModel.FileList.Items, i => i.DisplayName == fileName);
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
    public async Task NavigateToAsync_Missing_Path_Returns_False_And_Leaves_State_Unchanged()
    {
        var validRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-valid-{Guid.NewGuid():N}");
        Directory.CreateDirectory(validRoot);
        var missingPath = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-missing-{Guid.NewGuid():N}");

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.NavigateToAsync(validRoot);

            var result = await viewModel.NavigateToAsync(missingPath);

            Assert.False(result);
            Assert.Equal(validRoot, viewModel.Navigation.CurrentPath);
        }
        finally
        {
            if (Directory.Exists(validRoot))
            {
                Directory.Delete(validRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GoBackAsync_And_GoForwardAsync_Load_Folder_Items_From_History()
    {
        var firstRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-first-{Guid.NewGuid():N}");
        var secondRoot = Path.Combine(Path.GetTempPath(), $"nimbus-main-vm-second-{Guid.NewGuid():N}");
        Directory.CreateDirectory(firstRoot);
        Directory.CreateDirectory(secondRoot);
        var firstFile = "first.txt";
        var secondFile = "second.txt";
        await File.WriteAllTextAsync(Path.Combine(firstRoot, firstFile), "x");
        await File.WriteAllTextAsync(Path.Combine(secondRoot, secondFile), "x");

        try
        {
            var viewModel = CreateViewModel();
            await viewModel.NavigateToAsync(firstRoot);
            await viewModel.NavigateToAsync(secondRoot);

            var backResult = await viewModel.GoBackAsync();

            Assert.True(backResult);
            Assert.Equal(firstRoot, viewModel.Navigation.CurrentPath);
            Assert.Contains(viewModel.FileList.Items, i => i.DisplayName == firstFile);

            var forwardResult = await viewModel.GoForwardAsync();

            Assert.True(forwardResult);
            Assert.Equal(secondRoot, viewModel.Navigation.CurrentPath);
            Assert.Contains(viewModel.FileList.Items, i => i.DisplayName == secondFile);
        }
        finally
        {
            if (Directory.Exists(firstRoot))
            {
                Directory.Delete(firstRoot, recursive: true);
            }

            if (Directory.Exists(secondRoot))
            {
                Directory.Delete(secondRoot, recursive: true);
            }
        }
    }

    private static MainPageViewModel CreateViewModel()
    {
        var shellItemService = new ShellItemService();
        var viewPreferenceService = new ViewPreferenceService();
        var filePreviewService = new FilePreviewService();
        var fileListViewModel = new FileListViewModel(shellItemService, viewPreferenceService, filePreviewService);
        var navigationViewModel = new NavigationViewModel(new NavigationService());
        var sidebarViewModel = new SidebarViewModel();
        var fileOperationsService = new FileOperationsService();
        return new MainPageViewModel(sidebarViewModel, fileListViewModel, navigationViewModel, fileOperationsService);
    }
}
