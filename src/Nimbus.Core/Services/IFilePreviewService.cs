using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface IFilePreviewService
{
    Task<FilePreviewModel> GetPreviewAsync(string path, CancellationToken cancellationToken = default);
}
