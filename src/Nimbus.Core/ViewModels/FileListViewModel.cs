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
    private string? _currentPath;
    private FileViewMode _currentViewMode = FileViewMode.List;

    public FileListViewModel(
        IShellItemService shellItemService,
        IViewPreferenceService viewPreferenceService)
    {
        _shellItemService = shellItemService;
        _viewPreferenceService = viewPreferenceService;
    }

    public ObservableCollection<ShellItemModel> Items { get; } = new();

    public ShellItemModel? SelectedItem { get; set; }

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
