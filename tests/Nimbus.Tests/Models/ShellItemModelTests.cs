using Nimbus.Core.Models;

namespace Nimbus.Tests.Models;

public class ShellItemModelTests
{
    [Fact]
    public void ShellItemModel_Has_Required_Properties()
    {
        var item = new ShellItemModel("C:\\")
        {
            DisplayName = "C:\\",
            IsFolder = true
        };

        Assert.Equal("C:\\", item.Path);
        Assert.NotNull(item.DisplayName);
        Assert.True(item.IsFolder);
    }
}
