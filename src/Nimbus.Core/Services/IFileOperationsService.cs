namespace Nimbus.Core.Services;

public interface IFileOperationsService
{
    Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task RenameAsync(string sourcePath, string newName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
