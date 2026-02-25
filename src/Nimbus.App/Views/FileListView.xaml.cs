using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nimbus.Core.Models;
using Nimbus.Core.ViewModels;

namespace Nimbus.App.Views;

public sealed partial class FileListView : UserControl
{
    public event EventHandler<ShellItemModel>? ItemInvoked;
    public event EventHandler? ItemSelectionChanged;
    private FileListViewModel? _fileListViewModel;

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
        }

        _fileListViewModel = nextViewModel;

        if (_fileListViewModel is not null)
        {
            _fileListViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileListViewModel.CurrentViewMode))
        {
            ApplyCurrentMode();
        }
    }

    private void ApplyCurrentMode()
    {
        var mode = _fileListViewModel?.CurrentViewMode ?? FileViewMode.List;
        ApplyMode(mode);
    }

    private void ApplyMode(FileViewMode mode)
    {
        var templateKey = mode switch
        {
            FileViewMode.Icon => "IconModeTemplate",
            FileViewMode.Column => "ColumnModeTemplate",
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

        ListHeaderGrid.Visibility = mode is FileViewMode.List or FileViewMode.Column
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
