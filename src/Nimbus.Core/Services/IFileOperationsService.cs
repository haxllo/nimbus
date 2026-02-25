namespace Nimbus.Core.Services;

public interface IFileOperationsService
{
    Task<FileOperationResult> CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task<FileOperationResult> MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task<FileOperationResult> RenameAsync(string sourcePath, string newName, CancellationToken cancellationToken = default);
    Task<FileOperationResult> DeleteAsync(string path, CancellationToken cancellationToken = default);
}
