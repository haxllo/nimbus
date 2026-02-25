using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nimbus.Core.Models;
using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class FileListViewModel : INotifyPropertyChanged
{
    private readonly IShellItemService _shellItemService;
    private readonly IViewPreferenceService _viewPreferenceService;
    private readonly IFilePreviewService _filePreviewService;
    private string? _currentPath;
    private FileViewMode _currentViewMode = FileViewMode.List;
    private ShellItemModel? _selectedItem;
    private FilePreviewModel? _currentPreview;

    public FileListViewModel(
        IShellItemService shellItemService,
        IViewPreferenceService viewPreferenceService,
        IFilePreviewService filePreviewService)
    {
        _shellItemService = shellItemService;
        _viewPreferenceService = viewPreferenceService;
        _filePreviewService = filePreviewService;
    }

    public ObservableCollection<ShellItemModel> Items { get; } = new();

    public ShellItemModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (ReferenceEquals(_selectedItem, value))
            {
                return;
            }

            _selectedItem = value;
            OnPropertyChanged();

            if (_selectedItem is null)
            {
                CurrentPreview = null;
            }
        }
    }

    public FilePreviewModel? CurrentPreview
    {
        get => _currentPreview;
        private set
        {
            if (ReferenceEquals(_currentPreview, value))
            {
                return;
            }

            _currentPreview = value;
            OnPropertyChanged();
        }
    }

    public FileViewMode CurrentViewMode
    {
        get => _currentViewMode;
        private set
        {
            if (_currentViewMode == value)
            {
                return;
            }

            _currentViewMode = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        _currentPath = path;
        CurrentViewMode = _viewPreferenceService.GetViewMode(path);
        SelectedItem = null;
        CurrentPreview = null;

        Items.Clear();

        var items = await _shellItemService.EnumerateAsync(path, cancellationToken);
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    public void SetViewModeForCurrentPath(FileViewMode viewMode)
    {
        CurrentViewMode = viewMode;

        if (string.IsNullOrWhiteSpace(_currentPath))
        {
            return;
        }

        _viewPreferenceService.SetViewMode(_currentPath, viewMode);
    }

    public async Task LoadPreviewForSelectionAsync(CancellationToken cancellationToken = default)
    {
        var selectedPath = SelectedItem?.Path;
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            CurrentPreview = null;
            return;
        }

        var preview = await _filePreviewService.GetPreviewAsync(selectedPath, cancellationToken);

        if (!string.Equals(SelectedItem?.Path, selectedPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentPreview = preview;
    }

    public void ClearPreview()
    {
        CurrentPreview = null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
