using Microsoft.UI.Xaml.Controls;
using Nimbus.Core.Models;

namespace Nimbus.App.Views;

public sealed partial class SidebarView : UserControl
{
    public event EventHandler<SidebarLocation>? LocationSelected;
    public event EventHandler<SavedSearchModel>? SavedSearchSelected;

    public SidebarView()
    {
        InitializeComponent();
    }

    private void OnLocationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SidebarList.SelectedItem is SidebarLocation location)
        {
            LocationSelected?.Invoke(this, location);
        }
    }

    private void OnSavedSearchSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SavedSearchList.SelectedItem is SavedSearchModel savedSearch)
        {
            SavedSearchSelected?.Invoke(this, savedSearch);
        }
    }
}
