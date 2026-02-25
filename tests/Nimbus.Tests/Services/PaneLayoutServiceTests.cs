using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class PaneLayoutServiceTests
{
    [Fact]
    public void GetLayout_When_File_Missing_Returns_Defaults()
    {
        var storagePath = Path.Combine(Path.GetTempPath(), $"nimbus-pane-layout-{Guid.NewGuid():N}.json");
        var service = new PaneLayoutService(storagePath);

        var layout = service.GetLayout();

        Assert.Equal(260, layout.SidebarWidth);
        Assert.Equal(300, layout.PreviewWidth);
    }

    [Fact]
    public void SaveLayout_Persists_And_Clamps_Values()
    {
        var storagePath = Path.Combine(Path.GetTempPath(), $"nimbus-pane-layout-{Guid.NewGuid():N}.json");

        try
        {
            var service = new PaneLayoutService(storagePath);
            service.SaveLayout(50, 5000);

            var layout = service.GetLayout();

            Assert.Equal(180, layout.SidebarWidth);
            Assert.Equal(620, layout.PreviewWidth);
        }
        finally
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
    }
}
