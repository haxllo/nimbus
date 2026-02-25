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

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _viewModel.InitializeAsync();
        UpdatePathBox();
        UpdateNavButtons();
    }

    private async void OnSidebarLocationSelected(object? sender, SidebarLocation e)
    {
        await _viewModel.NavigateToAsync(e.Path);
        UpdatePathBox();
        UpdateNavButtons();
    }

    private async void OnFileItemInvoked(object? sender, ShellItemModel e)
    {
        if (e.IsFolder)
        {
            await _viewModel.NavigateToAsync(e.Path);
            UpdatePathBox();
            UpdateNavButtons();
        }
    }

    private async void OnBackClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.GoBackAsync();
        UpdatePathBox();
        UpdateNavButtons();
    }

    private async void OnForwardClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.GoForwardAsync();
        UpdatePathBox();
        UpdateNavButtons();
    }

    private async void OnPathBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(PathBox.Text))
        {
            await _viewModel.NavigateToAsync(PathBox.Text);
            UpdatePathBox();
            UpdateNavButtons();
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
            return;
        }

        var results = await _searchService.SearchAsync(root, SearchBox.Text);
        _viewModel.FileList.Items.Clear();
        foreach (var result in results)
        {
            _viewModel.FileList.Items.Add(new ShellItemModel(result)
            {
                DisplayName = Path.GetFileName(result),
                IsFolder = false
            });
        }
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
}
