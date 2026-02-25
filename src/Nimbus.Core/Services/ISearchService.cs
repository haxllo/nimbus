namespace Nimbus.Core.Services;

public interface ISearchService
{
    Task<IReadOnlyList<string>> SearchAsync(string rootPath, string pattern, CancellationToken cancellationToken = default);
}
