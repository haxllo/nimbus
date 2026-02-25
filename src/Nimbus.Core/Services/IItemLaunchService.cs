namespace Nimbus.Core.Services;

public interface IItemLaunchService
{
    Task<FileOperationResult> LaunchAsync(string path, CancellationToken cancellationToken = default);
}
