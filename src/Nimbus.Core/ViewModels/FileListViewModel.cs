using System.Collections.ObjectModel;
using Nimbus.Core.Models;
using Nimbus.Core.Services;

namespace Nimbus.Core.ViewModels;

public sealed class FileListViewModel
{
    private readonly IShellItemService _shellItemService;

    public FileListViewModel(IShellItemService shellItemService)
    {
        _shellItemService = shellItemService;
    }

    public ObservableCollection<ShellItemModel> Items { get; } = new();

    public ShellItemModel? SelectedItem { get; set; }

    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        Items.Clear();

        var items = await _shellItemService.EnumerateAsync(path, cancellationToken);
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }
}
