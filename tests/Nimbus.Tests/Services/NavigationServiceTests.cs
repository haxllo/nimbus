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
}
