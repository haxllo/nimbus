using Nimbus.Core.Models;

namespace Nimbus.Core.Services;

public interface ITagService
{
    IReadOnlyList<FileTagModel> GetAll();

    FileTagModel? Resolve(string id);
}
