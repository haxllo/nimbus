using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class MainPageViewModel
{
    private readonly IFileOperationsService _fileOperationsService;

    public MainPageViewModel(
        SidebarViewModel sidebarViewModel,
        FileListViewModel fileListViewModel,
        NavigationViewModel navigationViewModel,
        IFileOperationsService fileOperationsService)
    {
        Sidebar = sidebarViewModel;
        FileList = fileListViewModel;
        Navigation = navigationViewModel;
        _fileOperationsService = fileOperationsService;
    }

    public SidebarViewModel Sidebar { get; }

    public FileListViewModel FileList { get; }

    public NavigationViewModel Navigation { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var start = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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
        return true;
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
        }

        return result;
    }
}
