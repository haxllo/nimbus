using Nimbus.Core.Models;
using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.ViewModels;

public class TabsViewModelTests
{
    [Fact]
    public void EnsureInitialized_Creates_First_Tab()
    {
        var viewModel = new TabsViewModel();

        var firstTab = viewModel.EnsureInitialized(NavigationState.FromCurrentPath("C:\\Users"));

        Assert.Single(viewModel.Tabs);
        Assert.Equal(firstTab.Id, viewModel.ActiveTab?.Id);
        Assert.Equal("Users", firstTab.Title);
    }

    [Fact]
    public void CloseTab_When_Active_Tab_Is_Closed_Selects_Right_Neighbor()
    {
        var viewModel = new TabsViewModel();
        var first = viewModel.EnsureInitialized(NavigationState.FromCurrentPath("C:\\A"));
        var second = viewModel.OpenTab(NavigationState.FromCurrentPath("C:\\B"));
        var third = viewModel.OpenTab(NavigationState.FromCurrentPath("C:\\C"));
        _ = viewModel.ActivateTab(second.Id);

        var closed = viewModel.CloseTab(second.Id);

        Assert.True(closed);
        Assert.Equal(2, viewModel.Tabs.Count);
        Assert.Equal(third.Id, viewModel.ActiveTab?.Id);
        Assert.DoesNotContain(viewModel.Tabs, tab => tab.Id == second.Id);
        Assert.Contains(viewModel.Tabs, tab => tab.Id == first.Id);
    }

    [Fact]
    public void CloseTab_When_Only_One_Tab_Exists_Returns_False()
    {
        var viewModel = new TabsViewModel();
        var firstTab = viewModel.EnsureInitialized(NavigationState.FromCurrentPath("C:\\Only"));

        var closed = viewModel.CloseTab(firstTab.Id);

        Assert.False(closed);
        Assert.Single(viewModel.Tabs);
    }

    [Fact]
    public void ActivateNextTab_Wraps_Around()
    {
        var viewModel = new TabsViewModel();
        var first = viewModel.EnsureInitialized(NavigationState.FromCurrentPath("C:\\One"));
        var second = viewModel.OpenTab(NavigationState.FromCurrentPath("C:\\Two"));

        _ = viewModel.ActivateTab(second.Id);
        var switched = viewModel.ActivateNextTab();

        Assert.True(switched);
        Assert.Equal(first.Id, viewModel.ActiveTab?.Id);
    }
}
