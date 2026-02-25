using Microsoft.UI.Xaml.Controls;
using Nimbus.Core.Models;

namespace Nimbus.App.Views;

public sealed partial class FileListView : UserControl
{
    public event EventHandler<ShellItemModel>? ItemInvoked;

    public FileListView()
    {
        InitializeComponent();
    }

    private void OnDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (FileList.SelectedItem is ShellItemModel item)
        {
            ItemInvoked?.Invoke(this, item);
        }
    }
}
