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
}
