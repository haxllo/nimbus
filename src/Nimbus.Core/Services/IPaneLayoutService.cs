using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface IPaneLayoutService
{
    PaneLayoutModel GetLayout();

    void SaveLayout(double sidebarWidth, double previewWidth);
}
