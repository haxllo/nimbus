using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface IViewPreferenceService
{
    FileViewMode GetViewMode(string path);

    void SetViewMode(string path, FileViewMode viewMode);
}
