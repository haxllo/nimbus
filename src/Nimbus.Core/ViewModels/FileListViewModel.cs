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
    private FileSortField _currentSortField = FileSortField.Name;
    private bool _isSortDescending;
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

    public FileSortField CurrentSortField
    {
        get => _currentSortField;
        private set
        {
            if (_currentSortField == value)
            {
                return;
            }

            _currentSortField = value;
            OnPropertyChanged();
        }
    }

    public bool IsSortDescending
    {
        get => _isSortDescending;
        private set
        {
            if (_isSortDescending == value)
            {
                return;
            }

            _isSortDescending = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        _currentPath = path;
        OnPropertyChanged(nameof(CurrentPath));
        CurrentViewMode = _viewPreferenceService.GetViewMode(path);
        SelectedItem = null;
        CurrentPreview = null;

        Items.Clear();

        var items = await _shellItemService.EnumerateAsync(path, cancellationToken);
        foreach (var item in ApplySort(items))
        {
            Items.Add(item);
        }
    }

    public string? CurrentPath => _currentPath;

    public async Task<IReadOnlyList<ShellItemModel>> LoadColumnItemsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Array.Empty<ShellItemModel>();
        }

        var items = await _shellItemService.EnumerateAsync(path, cancellationToken);
        return ApplySort(items).ToArray();
    }

    public void SetSort(FileSortField sortField, bool descending)
    {
        CurrentSortField = sortField;
        IsSortDescending = descending;

        var sorted = ApplySort(Items).ToArray();
        Items.Clear();
        foreach (var item in sorted)
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

    private IEnumerable<ShellItemModel> ApplySort(IEnumerable<ShellItemModel> source)
    {
        var ordered = source.OrderByDescending(item => item.IsFolder);

        ordered = CurrentSortField switch
        {
            FileSortField.DateModified when IsSortDescending
                => ordered.ThenByDescending(item => item.DateModified ?? DateTimeOffset.MinValue),
            FileSortField.DateModified
                => ordered.ThenBy(item => item.DateModified ?? DateTimeOffset.MinValue),
            FileSortField.Size when IsSortDescending
                => ordered.ThenByDescending(item => item.SizeBytes ?? -1),
            FileSortField.Size
                => ordered.ThenBy(item => item.SizeBytes ?? -1),
            _ when IsSortDescending
                => ordered.ThenByDescending(item => item.DisplayName, StringComparer.OrdinalIgnoreCase),
            _
                => ordered.ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
        };

        return ordered.ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
