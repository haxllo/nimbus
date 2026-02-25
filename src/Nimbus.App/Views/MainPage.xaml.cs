using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Nimbus.Core.Models;
using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.App.Views;

/// <summary>
/// A simple page that can be used on its own or navigated to within a Frame.
/// </summary>
public partial class MainPage : Page
{
    private readonly MainPageViewModel _viewModel;
    private readonly ISearchService _searchService;

    public MainPage()
    {
        InitializeComponent();

        _viewModel = App.Services.GetRequiredService<MainPageViewModel>();
        _searchService = App.Services.GetRequiredService<ISearchService>();
        DataContext = _viewModel;

        Sidebar.LocationSelected += OnSidebarLocationSelected;
        FileList.ItemInvoked += OnFileItemInvoked;
        FileList.ItemSelectionChanged += OnFileItemSelectionChanged;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _viewModel.InitializeAsync();
        UpdateNavigationUi();
        SetStatus("Ready.");
    }

    private async void OnSidebarLocationSelected(object? sender, SidebarLocation e)
    {
        var success = await _viewModel.NavigateToAsync(e.Path);
        UpdateNavigationUi();

        if (!success)
        {
            SetStatus($"Unable to open location: {e.Path}", InfoBarSeverity.Error);
            return;
        }

        SetStatus($"Opened {e.DisplayName}.", InfoBarSeverity.Success);
    }

    private async void OnFileItemInvoked(object? sender, ShellItemModel e)
    {
        if (e.IsFolder)
        {
            var success = await _viewModel.NavigateToAsync(e.Path);
            UpdateNavigationUi();

            if (!success)
            {
                SetStatus($"Unable to open folder: {e.Path}", InfoBarSeverity.Error);
                return;
            }

            SetStatus($"Opened {e.DisplayName}.", InfoBarSeverity.Success);
        }
    }

    private void OnFileItemSelectionChanged(object? sender, EventArgs e)
    {
        UpdateNavButtons();
    }

    private async void OnBackClicked(object sender, RoutedEventArgs e)
    {
        await NavigateBackAsync();
    }

    private async void OnForwardClicked(object sender, RoutedEventArgs e)
    {
        await NavigateForwardAsync();
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        await RefreshCurrentFolderAsync();
    }

    private async void OnNewFolderClicked(object sender, RoutedEventArgs e)
    {
        await CreateNewFolderAsync();
    }

    private async void OnRenameClicked(object sender, RoutedEventArgs e)
    {
        await RenameSelectedItemAsync();
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedItemAsync();
    }

    private async void OnBackAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await NavigateBackAsync();
        args.Handled = true;
    }

    private async void OnForwardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await NavigateForwardAsync();
        args.Handled = true;
    }

    private async void OnRefreshAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await RefreshCurrentFolderAsync();
        args.Handled = true;
    }

    private async void OnNewFolderAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await CreateNewFolderAsync();
        args.Handled = true;
    }

    private async void OnDeleteAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await DeleteSelectedItemAsync();
        args.Handled = true;
    }

    private async void OnRenameAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await RenameSelectedItemAsync();
        args.Handled = true;
    }

    private void OnSearchFocusAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        _ = SearchBox.Focus(FocusState.Programmatic);
        SearchBox.SelectAll();
        args.Handled = true;
    }

    private async Task NavigateBackAsync()
    {
        var success = await _viewModel.GoBackAsync();
        UpdateNavigationUi();
        SetStatus(
            success ? "Navigated back." : "No previous location.",
            success ? InfoBarSeverity.Informational : InfoBarSeverity.Warning);
    }

    private async Task NavigateForwardAsync()
    {
        var success = await _viewModel.GoForwardAsync();
        UpdateNavigationUi();
        SetStatus(
            success ? "Navigated forward." : "No forward location.",
            success ? InfoBarSeverity.Informational : InfoBarSeverity.Warning);
    }

    private async Task RefreshCurrentFolderAsync(bool showStatus = true)
    {
        var currentPath = _viewModel.Navigation.CurrentPath;
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            if (showStatus)
            {
                SetStatus("Open a folder before refreshing.", InfoBarSeverity.Warning);
            }

            return;
        }

        var success = await _viewModel.NavigateToAsync(currentPath);
        UpdateNavigationUi();

        if (!showStatus)
        {
            return;
        }

        SetStatus(
            success ? $"Refreshed {currentPath}." : $"Unable to refresh folder: {currentPath}",
            success ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private async void OnPathBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(PathBox.Text))
        {
            var success = await _viewModel.NavigateToAsync(PathBox.Text);
            UpdateNavigationUi();

            if (!success)
            {
                SetStatus($"Path was not found: {PathBox.Text}", InfoBarSeverity.Error);
                return;
            }

            SetStatus($"Opened {PathBox.Text.Trim()}.", InfoBarSeverity.Success);
        }
    }

    private async void OnSearchBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        var root = _viewModel.Navigation.CurrentPath;
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            SetStatus("Enter a search term after opening a folder.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            var query = SearchBox.Text.Trim();
            var results = await _searchService.SearchAsync(root, query);

            _viewModel.FileList.Items.Clear();
            foreach (var result in results)
            {
                _viewModel.FileList.Items.Add(new ShellItemModel(result)
                {
                    DisplayName = Path.GetFileName(result),
                    IsFolder = false
                });
            }

            SetStatus(
                results.Count == 0
                    ? $"No results for \"{query}\"."
                    : $"Found {results.Count} result(s) for \"{query}\".",
                results.Count == 0 ? InfoBarSeverity.Warning : InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            SetStatus($"Search failed: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private async Task DeleteSelectedItemAsync()
    {
        var selectedItem = _viewModel.FileList.SelectedItem;
        if (selectedItem is null)
        {
            SetStatus("Select an item to delete.", InfoBarSeverity.Warning);
            return;
        }

        var confirmed = await ConfirmDeleteAsync(selectedItem);
        if (!confirmed)
        {
            SetStatus("Delete cancelled.", InfoBarSeverity.Warning);
            return;
        }

        var result = await _viewModel.DeleteItemAsync(selectedItem.Path);
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, InfoBarSeverity.Error);
            return;
        }

        SetStatus(result.Message, InfoBarSeverity.Success);
    }

    private async Task CreateNewFolderAsync()
    {
        var currentPath = _viewModel.Navigation.CurrentPath;
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            SetStatus("Open a folder before creating a new folder.", InfoBarSeverity.Warning);
            return;
        }

        if (!Directory.Exists(currentPath))
        {
            SetStatus($"Current path is unavailable: {currentPath}", InfoBarSeverity.Error);
            return;
        }

        var folderName = FindAvailableFolderName(currentPath);
        while (true)
        {
            var result = await _viewModel.CreateFolderAsync(currentPath, folderName);
            if (result.IsSuccess)
            {
                SelectItemByPath(Path.Combine(currentPath, folderName));

                var renamed = await RenameSelectedItemAsync(showMissingSelectionStatus: false, showCancelledStatus: false);
                if (!renamed)
                {
                    SetStatus(result.Message, InfoBarSeverity.Success);
                }

                return;
            }

            if (result.ErrorCode != FileOperationErrorCode.Conflict)
            {
                SetStatus(result.Message, InfoBarSeverity.Error);
                return;
            }

            var retryName = await PromptForNameAsync(
                title: "Folder Name Already Exists",
                initialName: GetNextAvailableName(currentPath, folderName),
                primaryButtonText: "Create",
                allowEmptyMessage: "Folder name cannot be empty.");

            if (retryName is null)
            {
                SetStatus("Create folder cancelled.", InfoBarSeverity.Warning);
                return;
            }

            folderName = retryName;
        }
    }

    private async Task<bool> RenameSelectedItemAsync(
        bool showMissingSelectionStatus = true,
        bool showCancelledStatus = true)
    {
        var selectedItem = _viewModel.FileList.SelectedItem;
        if (selectedItem is null)
        {
            if (showMissingSelectionStatus)
            {
                SetStatus("Select an item to rename.", InfoBarSeverity.Warning);
            }

            return false;
        }

        var currentName = string.IsNullOrWhiteSpace(selectedItem.DisplayName)
            ? Path.GetFileName(selectedItem.Path)
            : selectedItem.DisplayName;
        var parentPath = Path.GetDirectoryName(selectedItem.Path) ?? string.Empty;
        var proposedName = currentName;

        while (true)
        {
            var newName = await PromptForNameAsync(
                title: "Rename Item",
                initialName: proposedName,
                primaryButtonText: "Rename",
                allowEmptyMessage: "Name cannot be empty.");

            if (newName is null)
            {
                if (showCancelledStatus)
                {
                    SetStatus("Rename cancelled.", InfoBarSeverity.Warning);
                }

                return false;
            }

            if (string.Equals(newName, currentName, StringComparison.OrdinalIgnoreCase))
            {
                if (showCancelledStatus)
                {
                    SetStatus("Rename cancelled.", InfoBarSeverity.Warning);
                }

                return false;
            }

            var result = await _viewModel.RenameItemAsync(selectedItem.Path, newName);
            if (result.IsSuccess)
            {
                SelectItemByPath(Path.Combine(parentPath, newName));
                SetStatus(result.Message, InfoBarSeverity.Success);
                return true;
            }

            if (result.ErrorCode != FileOperationErrorCode.Conflict)
            {
                SetStatus(result.Message, InfoBarSeverity.Error);
                return false;
            }

            proposedName = GetNextAvailableName(parentPath, newName);
            SetStatus(result.Message, InfoBarSeverity.Warning);
        }
    }

    private async void OnBreadcrumbClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string targetPath })
        {
            return;
        }

        var success = await _viewModel.NavigateToAsync(targetPath);
        UpdateNavigationUi();

        if (!success)
        {
            SetStatus($"Unable to open location: {targetPath}", InfoBarSeverity.Error);
            return;
        }

        SetStatus($"Opened {targetPath}.", InfoBarSeverity.Success);
    }

    private void UpdatePathBox()
    {
        PathBox.Text = _viewModel.Navigation.CurrentPath ?? string.Empty;
    }

    private void UpdateNavButtons()
    {
        BackButton.IsEnabled = _viewModel.Navigation.CanGoBack;
        ForwardButton.IsEnabled = _viewModel.Navigation.CanGoForward;
        RefreshButton.IsEnabled = !string.IsNullOrWhiteSpace(_viewModel.Navigation.CurrentPath);
        NewFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(_viewModel.Navigation.CurrentPath);
        RenameButton.IsEnabled = _viewModel.FileList.SelectedItem is not null;
        DeleteButton.IsEnabled = _viewModel.FileList.SelectedItem is not null;
    }

    private void UpdateBreadcrumbs()
    {
        BreadcrumbPanel.Children.Clear();

        var breadcrumbs = _viewModel.Navigation.BreadcrumbItems;
        if (breadcrumbs.Count == 0)
        {
            BreadcrumbPanel.Children.Add(new TextBlock { Text = "(no location)" });
            return;
        }

        for (var i = 0; i < breadcrumbs.Count; i++)
        {
            var breadcrumb = breadcrumbs[i];
            var button = new Button
            {
                Content = breadcrumb.Label,
                Tag = breadcrumb.Path
            };
            button.Click += OnBreadcrumbClicked;
            BreadcrumbPanel.Children.Add(button);

            if (i < breadcrumbs.Count - 1)
            {
                BreadcrumbPanel.Children.Add(new TextBlock
                {
                    Text = ">",
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }
    }

    private void UpdateNavigationUi()
    {
        UpdatePathBox();
        UpdateNavButtons();
        UpdateBreadcrumbs();
    }

    private void SetStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;
        StatusInfoBar.IsOpen = true;
    }

    private bool IsTextInputFocused() =>
        FocusManager.GetFocusedElement(XamlRoot) is TextBox;

    private static string FindAvailableFolderName(string parentPath)
    {
        const string baseName = "New Folder";
        var candidate = baseName;
        var suffix = 2;

        while (Directory.Exists(Path.Combine(parentPath, candidate)) ||
               File.Exists(Path.Combine(parentPath, candidate)))
        {
            candidate = $"{baseName} ({suffix})";
            suffix++;
        }

        return candidate;
    }

    private async Task<bool> ConfirmDeleteAsync(ShellItemModel selectedItem)
    {
        var name = string.IsNullOrWhiteSpace(selectedItem.DisplayName)
            ? selectedItem.Path
            : selectedItem.DisplayName;

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Delete Item",
            Content = $"Delete \"{name}\"? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private async Task<string?> PromptForNameAsync(
        string title,
        string initialName,
        string primaryButtonText,
        string allowEmptyMessage)
    {
        var textBox = new TextBox
        {
            Text = initialName
        };
        textBox.SelectAll();

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = textBox,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        var newName = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            SetStatus(allowEmptyMessage, InfoBarSeverity.Warning);
            return null;
        }

        return newName;
    }

    private static string GetNextAvailableName(string parentPath, string preferredName)
    {
        var baseName = Path.GetFileNameWithoutExtension(preferredName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = preferredName;
        }

        var extension = Path.GetExtension(preferredName);
        var candidate = preferredName;
        var suffix = 2;

        while (Directory.Exists(Path.Combine(parentPath, candidate)) ||
               File.Exists(Path.Combine(parentPath, candidate)))
        {
            candidate = string.IsNullOrWhiteSpace(extension)
                ? $"{baseName} ({suffix})"
                : $"{baseName} ({suffix}){extension}";
            suffix++;
        }

        return candidate;
    }

    private void SelectItemByPath(string path)
    {
        var selected = _viewModel.FileList.Items.FirstOrDefault(item =>
            string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase));
        _viewModel.FileList.SelectedItem = selected;
    }
}
