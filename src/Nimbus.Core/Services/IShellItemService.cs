using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface IShellItemService
{
    Task<IReadOnlyList<ShellItemModel>> EnumerateAsync(string path, CancellationToken cancellationToken = default);
}
