using System.ComponentModel;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Nimbus.Core.Models;
using Nimbus.Core.ViewModels;

namespace Nimbus.App.Views;

public sealed partial class FileListView : UserControl
{
    public event EventHandler<ShellItemModel>? ItemInvoked;
    public event EventHandler? ItemSelectionChanged;
    private FileListViewModel? _fileListViewModel;
    private readonly List<string> _columnSelectedPaths = new();
    private bool _suppressColumnSelectionChanges;
    private int _columnRebuildToken;

    public FileListView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (FileList.SelectedItem is ShellItemModel item)
        {
            ItemInvoked?.Invoke(this, item);
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ItemSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TryAttachViewModel(DataContext as FileListViewModel);
        ApplyCurrentMode();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        TryAttachViewModel(args.NewValue as FileListViewModel);
        ApplyCurrentMode();
    }

    private void TryAttachViewModel(FileListViewModel? nextViewModel)
    {
        if (ReferenceEquals(_fileListViewModel, nextViewModel))
        {
            return;
        }

        if (_fileListViewModel is not null)
        {
            _fileListViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _fileListViewModel.Items.CollectionChanged -= OnItemsCollectionChanged;
        }

        _fileListViewModel = nextViewModel;

        if (_fileListViewModel is not null)
        {
            _fileListViewModel.PropertyChanged += OnViewModelPropertyChanged;
            _fileListViewModel.Items.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileListViewModel.CurrentViewMode))
        {
            ApplyCurrentMode();
            return;
        }

        if (_fileListViewModel?.CurrentViewMode != FileViewMode.Column)
        {
            return;
        }

        if (e.PropertyName == nameof(FileListViewModel.CurrentPath))
        {
            _columnSelectedPaths.Clear();
            _ = RebuildColumnBrowserAsync();
            return;
        }

        if (e.PropertyName is nameof(FileListViewModel.CurrentSortField) or nameof(FileListViewModel.IsSortDescending))
        {
            _ = RebuildColumnBrowserAsync();
        }
    }

    private void OnItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_fileListViewModel?.CurrentViewMode == FileViewMode.Column)
        {
            _ = RebuildColumnBrowserAsync();
        }
    }

    private void ApplyCurrentMode()
    {
        var mode = _fileListViewModel?.CurrentViewMode ?? FileViewMode.List;
        ApplyMode(mode);
    }

    private void ApplyMode(FileViewMode mode)
    {
        if (mode == FileViewMode.Column)
        {
            ListHeaderGrid.Visibility = Visibility.Collapsed;
            FileList.Visibility = Visibility.Collapsed;
            ColumnBrowserHost.Visibility = Visibility.Visible;
            _ = RebuildColumnBrowserAsync();
            return;
        }

        ColumnBrowserHost.Visibility = Visibility.Collapsed;
        FileList.Visibility = Visibility.Visible;

        var templateKey = mode switch
        {
            FileViewMode.Icon => "IconModeTemplate",
            FileViewMode.Gallery => "GalleryModeTemplate",
            _ => "ListModeTemplate"
        };

        if (Resources[templateKey] is DataTemplate template)
        {
            FileList.ItemTemplate = template;
        }

        var panelKey = mode is FileViewMode.Icon or FileViewMode.Gallery
            ? "GridItemsPanel"
            : "ListItemsPanel";
        if (Resources[panelKey] is ItemsPanelTemplate panel)
        {
            FileList.ItemsPanel = panel;
        }

        ListHeaderGrid.Visibility = mode is FileViewMode.List
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async Task RebuildColumnBrowserAsync()
    {
        if (_fileListViewModel is null || _fileListViewModel.CurrentViewMode != FileViewMode.Column)
        {
            return;
        }

        var currentPath = _fileListViewModel.CurrentPath;
        ColumnBrowserPanel.Children.Clear();

        if (string.IsNullOrWhiteSpace(currentPath))
        {
            return;
        }

        var token = ++_columnRebuildToken;
        _suppressColumnSelectionChanges = true;

        try
        {
            var rootItems = _fileListViewModel.Items.ToArray();
            var selectedRootPath = _columnSelectedPaths.Count > 0
                ? _columnSelectedPaths[0]
                : _fileListViewModel.SelectedItem?.Path;
            var selectedRootItem = rootItems.FirstOrDefault(item =>
                string.Equals(item.Path, selectedRootPath, StringComparison.OrdinalIgnoreCase));

            AddColumn(
                depth: 0,
                header: BuildColumnHeader(currentPath),
                items: rootItems,
                selectedItem: selectedRootItem);

            var nextFolderPath = selectedRootItem?.IsFolder == true ? selectedRootItem.Path : null;
            var depth = 1;
            while (!string.IsNullOrWhiteSpace(nextFolderPath))
            {
                var childItems = await _fileListViewModel.LoadColumnItemsAsync(nextFolderPath);
                if (token != _columnRebuildToken)
                {
                    return;
                }

                var selectedChildPath = _columnSelectedPaths.Count > depth
                    ? _columnSelectedPaths[depth]
                    : null;
                var selectedChildItem = childItems.FirstOrDefault(item =>
                    string.Equals(item.Path, selectedChildPath, StringComparison.OrdinalIgnoreCase));

                AddColumn(
                    depth: depth,
                    header: BuildColumnHeader(nextFolderPath),
                    items: childItems,
                    selectedItem: selectedChildItem);

                nextFolderPath = selectedChildItem?.IsFolder == true ? selectedChildItem.Path : null;
                depth++;
            }
        }
        finally
        {
            _suppressColumnSelectionChanges = false;
        }
    }

    private void AddColumn(
        int depth,
        string header,
        IReadOnlyList<ShellItemModel> items,
        ShellItemModel? selectedItem)
    {
        var listView = new ListView
        {
            Width = 250,
            MinHeight = 380,
            ItemsSource = items,
            ItemTemplate = Resources["ColumnBrowserItemTemplate"] as DataTemplate,
            Tag = new ColumnListContext(depth)
        };

        if (Resources["ColumnBrowserListStyle"] is Style listStyle)
        {
            listView.Style = listStyle;
        }

        listView.SelectionChanged += OnColumnSelectionChanged;
        listView.DoubleTapped += OnColumnDoubleTapped;

        if (selectedItem is not null)
        {
            listView.SelectedItem = selectedItem;
        }

        var column = new Border
        {
            Width = 266,
            Padding = new Thickness(6),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(0),
            BorderBrush = Resources["FileListDividerBrush"] as Microsoft.UI.Xaml.Media.Brush,
            Background = Resources["FileListHeaderBrush"] as Microsoft.UI.Xaml.Media.Brush,
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock
                    {
                        Text = header,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    },
                    listView
                }
            }
        };

        ColumnBrowserPanel.Children.Add(column);
    }

    private async void OnColumnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressColumnSelectionChanges || _fileListViewModel is null)
        {
            return;
        }

        if (sender is not ListView { Tag: ColumnListContext context } listView)
        {
            return;
        }

        var selectedItem = listView.SelectedItem as ShellItemModel;
        _fileListViewModel.SelectedItem = selectedItem;
        ItemSelectionChanged?.Invoke(this, EventArgs.Empty);

        UpdateColumnExpansion(context.Depth, selectedItem);
        await RebuildColumnBrowserAsync();
    }

    private void OnColumnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not ListView { SelectedItem: ShellItemModel item })
        {
            return;
        }

        ItemInvoked?.Invoke(this, item);
    }

    private void UpdateColumnExpansion(int depth, ShellItemModel? selectedItem)
    {
        if (_columnSelectedPaths.Count > depth)
        {
            _columnSelectedPaths.RemoveRange(depth, _columnSelectedPaths.Count - depth);
        }

        if (selectedItem is not null)
        {
            _columnSelectedPaths.Add(selectedItem.Path);
        }
    }

    private static string BuildColumnHeader(string path)
    {
        var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? path : name;
    }

    private sealed record ColumnListContext(int Depth);
}
