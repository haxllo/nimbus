using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface INavigationService
{
    string? CurrentPath { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }

    void NavigateTo(string path);
    void GoBack();
    void GoForward();
    NavigationState CaptureState();
    void RestoreState(NavigationState state);
    IReadOnlyList<string> GetBreadcrumbSegments();
    IReadOnlyList<BreadcrumbItem> GetBreadcrumbItems();
}
