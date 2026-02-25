using Microsoft.UI.Xaml.Controls;
using Nimbus.Core.Models;

namespace Nimbus.App.Views;

public sealed partial class SidebarView : UserControl
{
    public event EventHandler<SidebarLocation>? LocationSelected;

    public SidebarView()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SidebarList.SelectedItem is SidebarLocation location)
        {
            LocationSelected?.Invoke(this, location);
        }
    }
}
