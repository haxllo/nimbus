using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class NavigationServiceTests
{
    [Fact]
    public void BackForward_Tracks_History()
    {
        var nav = new NavigationService();
        nav.NavigateTo("C:\\");
        nav.NavigateTo("C:\\Users");

        Assert.True(nav.CanGoBack);

        nav.GoBack();
        Assert.Equal("C:\\", nav.CurrentPath);
    }

    [Fact]
    public void GetBreadcrumbSegments_Returns_Path_Segments()
    {
        var nav = new NavigationService();
        nav.NavigateTo("C:\\Users\\dev");

        var segments = nav.GetBreadcrumbSegments();

        Assert.Equal(new[] { "C:", "Users", "dev" }, segments);
    }

    [Fact]
    public void GetBreadcrumbItems_Returns_Labels_And_Paths()
    {
        var nav = new NavigationService();
        nav.NavigateTo("C:\\Users\\dev");

        var breadcrumbs = nav.GetBreadcrumbItems();

        Assert.Equal(3, breadcrumbs.Count);
        Assert.Equal("C:", breadcrumbs[0].Label);
        Assert.Equal("C:\\", breadcrumbs[0].Path);
        Assert.Equal("Users", breadcrumbs[1].Label);
        Assert.Equal("C:\\Users", breadcrumbs[1].Path);
        Assert.Equal("dev", breadcrumbs[2].Label);
        Assert.Equal("C:\\Users\\dev", breadcrumbs[2].Path);
    }
}
