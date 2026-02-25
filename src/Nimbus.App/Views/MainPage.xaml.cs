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
    private readonly IFileOperationsService _fileOperationsService;

    public MainPage()
    {
        InitializeComponent();

        _viewModel = App.Services.GetRequiredService<MainPageViewModel>();
        _searchService = App.Services.GetRequiredService<ISearchService>();
        _fileOperationsService = App.Services.GetRequiredService<IFileOperationsService>();
        DataContext = _viewModel;

        Sidebar.LocationSelected += OnSidebarLocationSelected;
        FileList.ItemInvoked += OnFileItemInvoked;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _viewModel.InitializeAsync();
        UpdatePathBox();
        UpdateNavButtons();
        SetStatus("Ready.");
    }

    private async void OnSidebarLocationSelected(object? sender, SidebarLocation e)
    {
        var success = await _viewModel.NavigateToAsync(e.Path);
        UpdatePathBox();
        UpdateNavButtons();

        if (!success)
        {
            SetStatus($"Unable to open location: {e.Path}", isError: true);
            return;
        }

        SetStatus($"Opened {e.DisplayName}.");
    }

    private async void OnFileItemInvoked(object? sender, ShellItemModel e)
    {
        if (e.IsFolder)
        {
            var success = await _viewModel.NavigateToAsync(e.Path);
            UpdatePathBox();
            UpdateNavButtons();

            if (!success)
            {
                SetStatus($"Unable to open folder: {e.Path}", isError: true);
                return;
            }

            SetStatus($"Opened {e.DisplayName}.");
        }
    }

    private async void OnBackClicked(object sender, RoutedEventArgs e)
    {
        var success = await _viewModel.GoBackAsync();
        UpdatePathBox();
        UpdateNavButtons();
        SetStatus(success ? "Navigated back." : "No previous location.");
    }

    private async void OnForwardClicked(object sender, RoutedEventArgs e)
    {
        var success = await _viewModel.GoForwardAsync();
        UpdatePathBox();
        UpdateNavButtons();
        SetStatus(success ? "Navigated forward." : "No forward location.");
    }

    private async void OnPathBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(PathBox.Text))
        {
            var success = await _viewModel.NavigateToAsync(PathBox.Text);
            UpdatePathBox();
            UpdateNavButtons();

            if (!success)
            {
                SetStatus($"Path was not found: {PathBox.Text}", isError: true);
                return;
            }

            SetStatus($"Opened {PathBox.Text.Trim()}.");
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
            SetStatus("Enter a search term after opening a folder.");
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

            SetStatus(results.Count == 0
                ? $"No results for \"{query}\"."
                : $"Found {results.Count} result(s) for \"{query}\".");
        }
        catch (Exception ex)
        {
            SetStatus($"Search failed: {ex.Message}", isError: true);
        }
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = _viewModel.FileList.SelectedItem;
        if (selectedItem is null)
        {
            SetStatus("Select an item to delete.");
            return;
        }

        var result = await _fileOperationsService.DeleteAsync(selectedItem.Path);
        if (!result.IsSuccess)
        {
            SetStatus(result.Message, isError: true);
            return;
        }

        if (_viewModel.Navigation.CurrentPath is { } currentPath)
        {
            await _viewModel.FileList.LoadAsync(currentPath);
        }

        SetStatus(result.Message);
    }

    private void UpdatePathBox()
    {
        PathBox.Text = _viewModel.Navigation.CurrentPath ?? string.Empty;
    }

    private void UpdateNavButtons()
    {
        BackButton.IsEnabled = _viewModel.Navigation.CanGoBack;
        ForwardButton.IsEnabled = _viewModel.Navigation.CanGoForward;
    }

    private void SetStatus(string message, bool isError = false)
    {
        StatusTextBlock.Text = isError ? $"Error: {message}" : message;
    }
}
