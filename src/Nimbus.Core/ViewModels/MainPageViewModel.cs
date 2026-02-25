using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class MainPageViewModel
{
    public MainPageViewModel(
        SidebarViewModel sidebarViewModel,
        FileListViewModel fileListViewModel,
        NavigationViewModel navigationViewModel)
    {
        Sidebar = sidebarViewModel;
        FileList = fileListViewModel;
        Navigation = navigationViewModel;
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
}
