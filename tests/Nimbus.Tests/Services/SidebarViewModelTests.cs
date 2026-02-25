using Nimbus.Core.ViewModels;

namespace Nimbus.Tests.Services;

public class SidebarViewModelTests
{
    [Fact]
    public void Sidebar_Includes_Known_Folders()
    {
        var vm = new SidebarViewModel();
        Assert.Contains(vm.Locations, l => l.Id == "Documents");
    }
}
