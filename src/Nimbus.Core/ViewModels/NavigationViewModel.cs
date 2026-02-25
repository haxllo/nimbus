using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class NavigationViewModel
{
    private readonly INavigationService _navigationService;

    public NavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public string? CurrentPath => _navigationService.CurrentPath;

    public bool CanGoBack => _navigationService.CanGoBack;

    public bool CanGoForward => _navigationService.CanGoForward;

    public IReadOnlyList<string> BreadcrumbSegments => _navigationService.GetBreadcrumbSegments();

    public void NavigateTo(string path) => _navigationService.NavigateTo(path);

    public void GoBack() => _navigationService.GoBack();

    public void GoForward() => _navigationService.GoForward();
}
