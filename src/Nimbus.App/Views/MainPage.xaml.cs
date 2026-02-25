using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
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
    private CancellationTokenSource? _activeSearchCancellation;
    private CancellationTokenSource? _activePreviewCancellation;
    private bool _isSyncingTabSelection;

    public MainPage()
    {
        InitializeComponent();

        _viewModel = App.Services.GetRequiredService<MainPageViewModel>();
        _searchService = App.Services.GetRequiredService<ISearchService>();
        DataContext = _viewModel;

        Sidebar.LocationSelected += OnSidebarLocationSelected;
        FileList.ItemInvoked += OnFileItemInvoked;
        FileList.ItemSelectionChanged += OnFileItemSelectionChanged;
        Unloaded += OnPageUnloaded;

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
        CancelActiveSearch();
        CancelActivePreviewLoad();
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
            CancelActiveSearch();
            CancelActivePreviewLoad();
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

    private async void OnFileItemSelectionChanged(object? sender, EventArgs e)
    {
        UpdateNavButtons();
        await RefreshPreviewAsync();
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

    private async void OnNewTabAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await OpenNewTabAsync();
        args.Handled = true;
    }

    private async void OnCloseTabAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await CloseCurrentTabAsync();
        args.Handled = true;
    }

    private async void OnNextTabAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await SwitchToNextTabAsync();
        args.Handled = true;
    }

    private async void OnPreviousTabAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await SwitchToPreviousTabAsync();
        args.Handled = true;
    }

    private async void OnQuickLookAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        await ShowQuickLookAsync();
        args.Handled = true;
    }

    private void OnViewModeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainPageViewModel viewModel)
        {
            return;
        }

        if (sender is not ComboBox comboBox)
        {
            return;
        }

        if (comboBox.SelectedIndex < 0)
        {
            return;
        }

        var selectedMode = comboBox.SelectedIndex switch
        {
            1 => FileViewMode.Icon,
            2 => FileViewMode.Column,
            3 => FileViewMode.Gallery,
            _ => FileViewMode.List
        };

        viewModel.FileList.SetViewModeForCurrentPath(selectedMode);
    }

    private async void OnAddTabButtonClick(TabView sender, object args)
    {
        await OpenNewTabAsync();
    }

    private async void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not ExplorerTabModel tab)
        {
            return;
        }

        var success = await _viewModel.CloseTabAsync(tab.Id);
        if (!success)
        {
            SetStatus("Cannot close the last tab.", InfoBarSeverity.Warning);
            return;
        }

        UpdateNavigationUi();
        SetStatus($"Closed tab: {tab.Title}.", InfoBarSeverity.Informational);
    }

    private async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncingTabSelection)
        {
            return;
        }

        if (TabsControl.SelectedItem is not ExplorerTabModel tab)
        {
            return;
        }

        if (_viewModel.Tabs.ActiveTab?.Id == tab.Id)
        {
            return;
        }

        var switched = await _viewModel.SwitchToTabAsync(tab.Id);
        if (!switched)
        {
            SetStatus($"Unable to switch to tab: {tab.Title}.", InfoBarSeverity.Warning);
            return;
        }

        UpdateNavigationUi();
    }

    private async Task NavigateBackAsync()
    {
        CancelActiveSearch();
        CancelActivePreviewLoad();
        var success = await _viewModel.GoBackAsync();
        UpdateNavigationUi();
        SetStatus(
            success ? "Navigated back." : "No previous location.",
            success ? InfoBarSeverity.Informational : InfoBarSeverity.Warning);
    }

    private async Task NavigateForwardAsync()
    {
        CancelActiveSearch();
        CancelActivePreviewLoad();
        var success = await _viewModel.GoForwardAsync();
        UpdateNavigationUi();
        SetStatus(
            success ? "Navigated forward." : "No forward location.",
            success ? InfoBarSeverity.Informational : InfoBarSeverity.Warning);
    }

    private async Task RefreshCurrentFolderAsync(bool showStatus = true)
    {
        CancelActiveSearch();
        CancelActivePreviewLoad();
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
            CancelActiveSearch();
            CancelActivePreviewLoad();
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

        CancelActivePreviewLoad();
        _viewModel.FileList.ClearPreview();

        var root = _viewModel.Navigation.CurrentPath;
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            SetStatus("Enter a search term after opening a folder.", InfoBarSeverity.Warning);
            return;
        }

        var query = SearchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            SetStatus("Enter a search term after opening a folder.", InfoBarSeverity.Warning);
            return;
        }

        var searchCancellation = StartSearch();
        SetStatus($"Searching \"{query}\"...", InfoBarSeverity.Informational);

        try
        {
            var results = await _searchService.SearchAsync(root, query, searchCancellation.Token);
            if (!IsActiveSearch(searchCancellation))
            {
                return;
            }

            _viewModel.FileList.Items.Clear();
            _viewModel.FileList.SelectedItem = null;
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
        catch (OperationCanceledException)
        {
            if (IsActiveSearch(searchCancellation))
            {
                SetStatus("Search cancelled.", InfoBarSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            if (IsActiveSearch(searchCancellation))
            {
                SetStatus($"Search failed: {ex.Message}", InfoBarSeverity.Error);
            }
        }
        finally
        {
            CompleteSearch(searchCancellation);
        }
    }

    private async Task RefreshPreviewAsync()
    {
        var previewCancellation = StartPreviewLoad();

        try
        {
            await _viewModel.FileList.LoadPreviewForSelectionAsync(previewCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Selection changed again; a newer preview request will update the pane.
        }
        finally
        {
            CompletePreviewLoad(previewCancellation);
        }
    }

    private async Task ShowQuickLookAsync()
    {
        var selectedItem = _viewModel.FileList.SelectedItem;
        if (selectedItem is null)
        {
            SetStatus("Select an item for Quick Look.", InfoBarSeverity.Warning);
            return;
        }

        var preview = _viewModel.FileList.CurrentPreview;
        if (preview is null ||
            !string.Equals(preview.Path, selectedItem.Path, StringComparison.OrdinalIgnoreCase))
        {
            await RefreshPreviewAsync();
            preview = _viewModel.FileList.CurrentPreview;
        }

        if (preview is null)
        {
            SetStatus("Unable to load preview.", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = $"Quick Look - {preview.Name}",
            Content = BuildQuickLookContent(preview),
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync();
    }

    private static UIElement BuildQuickLookContent(FilePreviewModel preview)
    {
        var stack = new StackPanel
        {
            Spacing = 8,
            Width = 640
        };

        stack.Children.Add(new TextBlock { Text = preview.Path, TextWrapping = TextWrapping.WrapWholeWords });
        stack.Children.Add(new TextBlock { Text = $"Type: {preview.ItemType}" });
        stack.Children.Add(new TextBlock
        {
            Text = preview.DateModified is null
                ? "Modified: (unknown)"
                : $"Modified: {preview.DateModified.Value.ToLocalTime():G}"
        });

        if (preview.SizeBytes is not null)
        {
            stack.Children.Add(new TextBlock { Text = $"Size: {preview.SizeBytes.Value} bytes" });
        }

        if (!string.IsNullOrWhiteSpace(preview.ErrorMessage))
        {
            stack.Children.Add(new TextBlock
            {
                Text = preview.ErrorMessage,
                TextWrapping = TextWrapping.WrapWholeWords
            });
            return new ScrollViewer { Content = stack };
        }

        if (preview.IsImage && !string.IsNullOrWhiteSpace(preview.ImagePath))
        {
            var image = new Image
            {
                MaxWidth = 620,
                MaxHeight = 480,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform
            };
            image.Source = new BitmapImage(ToFileUri(preview.ImagePath));
            stack.Children.Add(image);
            return new ScrollViewer { Content = stack };
        }

        if (!string.IsNullOrWhiteSpace(preview.TextPreview))
        {
            stack.Children.Add(new TextBox
            {
                Text = preview.TextPreview,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinHeight = 280
            });
        }

        return new ScrollViewer { Content = stack };
    }

    private static Uri ToFileUri(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var directUri) &&
            directUri.IsAbsoluteUri &&
            string.Equals(directUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
        {
            return directUri;
        }

        var normalized = path.Replace('\\', '/');
        return new Uri($"file:///{normalized}");
    }

    private async Task OpenNewTabAsync()
    {
        await _viewModel.OpenNewTabAsync();
        UpdateNavigationUi();
        SetStatus("Opened new tab.", InfoBarSeverity.Informational);
    }

    private async Task CloseCurrentTabAsync()
    {
        var closed = await _viewModel.CloseCurrentTabAsync();
        if (!closed)
        {
            SetStatus("Cannot close the last tab.", InfoBarSeverity.Warning);
            return;
        }

        UpdateNavigationUi();
        SetStatus("Closed current tab.", InfoBarSeverity.Informational);
    }

    private async Task SwitchToNextTabAsync()
    {
        var switched = await _viewModel.SwitchToNextTabAsync();
        if (!switched)
        {
            return;
        }

        UpdateNavigationUi();
    }

    private async Task SwitchToPreviousTabAsync()
    {
        var switched = await _viewModel.SwitchToPreviousTabAsync();
        if (!switched)
        {
            return;
        }

        UpdateNavigationUi();
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

        CancelActiveSearch();
        CancelActivePreviewLoad();
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
        UpdateTabSelection();
        UpdateViewModeSelector();
        UpdatePathBox();
        UpdateNavButtons();
        UpdateBreadcrumbs();
    }

    private void UpdateTabSelection()
    {
        var activeTab = _viewModel.Tabs.ActiveTab;
        if (activeTab is null || ReferenceEquals(TabsControl.SelectedItem, activeTab))
        {
            return;
        }

        _isSyncingTabSelection = true;
        try
        {
            TabsControl.SelectedItem = activeTab;
        }
        finally
        {
            _isSyncingTabSelection = false;
        }
    }

    private void UpdateViewModeSelector()
    {
        var selectedIndex = _viewModel.FileList.CurrentViewMode switch
        {
            FileViewMode.Icon => 1,
            FileViewMode.Column => 2,
            FileViewMode.Gallery => 3,
            _ => 0
        };

        if (ViewModeSelector.SelectedIndex != selectedIndex)
        {
            ViewModeSelector.SelectedIndex = selectedIndex;
        }
    }

    private void SetStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;
        StatusInfoBar.IsOpen = true;
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        CancelActiveSearch();
        CancelActivePreviewLoad();
    }

    private CancellationTokenSource StartSearch()
    {
        CancelActiveSearch();

        var cancellation = new CancellationTokenSource();
        _activeSearchCancellation = cancellation;
        return cancellation;
    }

    private void CompleteSearch(CancellationTokenSource cancellation)
    {
        if (!ReferenceEquals(_activeSearchCancellation, cancellation))
        {
            return;
        }

        _activeSearchCancellation = null;
        cancellation.Dispose();
    }

    private bool IsActiveSearch(CancellationTokenSource cancellation) =>
        ReferenceEquals(_activeSearchCancellation, cancellation);

    private void CancelActiveSearch()
    {
        var cancellation = _activeSearchCancellation;
        if (cancellation is null)
        {
            return;
        }

        _activeSearchCancellation = null;
        cancellation.Cancel();
        cancellation.Dispose();
    }

    private CancellationTokenSource StartPreviewLoad()
    {
        CancelActivePreviewLoad();

        var cancellation = new CancellationTokenSource();
        _activePreviewCancellation = cancellation;
        return cancellation;
    }

    private void CompletePreviewLoad(CancellationTokenSource cancellation)
    {
        if (!ReferenceEquals(_activePreviewCancellation, cancellation))
        {
            return;
        }

        _activePreviewCancellation = null;
        cancellation.Dispose();
    }

    private void CancelActivePreviewLoad()
    {
        var cancellation = _activePreviewCancellation;
        if (cancellation is null)
        {
            return;
        }

        _activePreviewCancellation = null;
        cancellation.Cancel();
        cancellation.Dispose();
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
