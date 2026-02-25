using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface ISavedSearchService
{
    IReadOnlyList<SavedSearchModel> GetAll();

    SavedSearchModel Create(string displayName, string rootPath, string query);

    SavedSearchModel? Resolve(string id);
}
