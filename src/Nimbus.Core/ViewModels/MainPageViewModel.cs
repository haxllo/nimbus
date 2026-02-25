using Nimbus.Core.Models;
using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class MainPageViewModel
{
    private readonly IFileOperationsService _fileOperationsService;

    public MainPageViewModel(
        SidebarViewModel sidebarViewModel,
        FileListViewModel fileListViewModel,
        NavigationViewModel navigationViewModel,
        TabsViewModel tabsViewModel,
        IFileOperationsService fileOperationsService)
    {
        Sidebar = sidebarViewModel;
        FileList = fileListViewModel;
        Navigation = navigationViewModel;
        Tabs = tabsViewModel;
        _fileOperationsService = fileOperationsService;
    }

    public SidebarViewModel Sidebar { get; }

    public FileListViewModel FileList { get; }

    public NavigationViewModel Navigation { get; }

    public TabsViewModel Tabs { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var start = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Tabs.EnsureInitialized(NavigationState.FromCurrentPath(start));
        await NavigateToAsync(start, cancellationToken);
    }

    public async Task<bool> NavigateToAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmedPath = path.Trim();
        if (!Directory.Exists(trimmedPath))
        {
            return false;
        }

        Navigation.NavigateTo(trimmedPath);
        await FileList.LoadAsync(trimmedPath, cancellationToken);
        Tabs.UpdateActiveState(Navigation.CaptureState());
        return true;
    }

    public async Task<bool> GoBackAsync(CancellationToken cancellationToken = default)
    {
        if (!Navigation.CanGoBack)
        {
            return false;
        }

        Navigation.GoBack();
        if (Navigation.CurrentPath is null)
        {
            return false;
        }

        await FileList.LoadAsync(Navigation.CurrentPath, cancellationToken);
        Tabs.UpdateActiveState(Navigation.CaptureState());
        return true;
    }

    public async Task<bool> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        if (!Navigation.CanGoForward)
        {
            return false;
        }

        Navigation.GoForward();
        if (Navigation.CurrentPath is null)
        {
            return false;
        }

        await FileList.LoadAsync(Navigation.CurrentPath, cancellationToken);
        Tabs.UpdateActiveState(Navigation.CaptureState());
        return true;
    }

    public async Task<bool> OpenNewTabAsync(CancellationToken cancellationToken = default)
    {
        var initialPath = Navigation.CurrentPath;
        var initialState = NavigationState.FromCurrentPath(initialPath);
        Tabs.OpenTab(initialState, activate: true);
        return await RestoreActiveTabAsync(cancellationToken);
    }

    public async Task<bool> CloseCurrentTabAsync(CancellationToken cancellationToken = default)
    {
        var activeTab = Tabs.ActiveTab;
        if (activeTab is null)
        {
            return false;
        }

        return await CloseTabAsync(activeTab.Id, cancellationToken);
    }

    public async Task<bool> CloseTabAsync(Guid tabId, CancellationToken cancellationToken = default)
    {
        var previousActiveTabId = Tabs.ActiveTab?.Id;
        if (!Tabs.CloseTab(tabId))
        {
            return false;
        }

        if (Tabs.ActiveTab is null)
        {
            return false;
        }

        var activeTabChanged = previousActiveTabId != Tabs.ActiveTab.Id;
        if (!activeTabChanged)
        {
            return true;
        }

        return await RestoreActiveTabAsync(cancellationToken);
    }

    public async Task<bool> SwitchToTabAsync(Guid tabId, CancellationToken cancellationToken = default)
    {
        if (!Tabs.ActivateTab(tabId))
        {
            return false;
        }

        return await RestoreActiveTabAsync(cancellationToken);
    }

    public async Task<bool> SwitchToNextTabAsync(CancellationToken cancellationToken = default)
    {
        if (!Tabs.ActivateNextTab())
        {
            return false;
        }

        return await RestoreActiveTabAsync(cancellationToken);
    }

    public async Task<bool> SwitchToPreviousTabAsync(CancellationToken cancellationToken = default)
    {
        if (!Tabs.ActivatePreviousTab())
        {
            return false;
        }

        return await RestoreActiveTabAsync(cancellationToken);
    }

    public async Task<FileOperationResult> CreateFolderAsync(
        string parentPath,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileOperationsService.CreateDirectoryAsync(parentPath, folderName, cancellationToken);
        if (result.IsSuccess &&
            Navigation.CurrentPath is { } currentPath &&
            string.Equals(currentPath, parentPath, StringComparison.OrdinalIgnoreCase))
        {
            await FileList.LoadAsync(currentPath, cancellationToken);
            Tabs.UpdateActiveState(Navigation.CaptureState());
        }

        return result;
    }

    public async Task<FileOperationResult> RenameItemAsync(
        string sourcePath,
        string newName,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileOperationsService.RenameAsync(sourcePath, newName, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        var parentPath = Path.GetDirectoryName(sourcePath);
        if (Navigation.CurrentPath is { } currentPath &&
            !string.IsNullOrWhiteSpace(parentPath) &&
            string.Equals(currentPath, parentPath, StringComparison.OrdinalIgnoreCase))
        {
            await FileList.LoadAsync(currentPath, cancellationToken);
            Tabs.UpdateActiveState(Navigation.CaptureState());
        }

        return result;
    }

    public async Task<FileOperationResult> DeleteItemAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileOperationsService.DeleteAsync(path, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        if (Navigation.CurrentPath is { } currentPath)
        {
            await FileList.LoadAsync(currentPath, cancellationToken);
            Tabs.UpdateActiveState(Navigation.CaptureState());
        }

        return result;
    }

    private async Task<bool> RestoreActiveTabAsync(CancellationToken cancellationToken)
    {
        var activeTab = Tabs.ActiveTab;
        if (activeTab is null)
        {
            return false;
        }

        Navigation.RestoreState(activeTab.NavigationState);
        var currentPath = Navigation.CurrentPath;
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            FileList.Items.Clear();
            FileList.SelectedItem = null;
            FileList.ClearPreview();
            Tabs.UpdateActiveState(Navigation.CaptureState());
            return true;
        }

        if (!Directory.Exists(currentPath))
        {
            FileList.Items.Clear();
            FileList.SelectedItem = null;
            FileList.ClearPreview();
            Tabs.UpdateActiveState(Navigation.CaptureState());
            return true;
        }

        await FileList.LoadAsync(currentPath, cancellationToken);
        Tabs.UpdateActiveState(Navigation.CaptureState());
        return true;
    }
}
