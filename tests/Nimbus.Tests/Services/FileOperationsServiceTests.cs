using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class FileOperationsServiceTests
{
    [Fact]
    public async Task CopyFile_Creates_Destination()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), "nimbus-tests");
        Directory.CreateDirectory(temp);

        var src = Path.Combine(temp, "nimbus-src.txt");
        var dst = Path.Combine(temp, "nimbus-dst.txt");

        await File.WriteAllTextAsync(src, "x");
        await svc.CopyAsync(src, dst);

        Assert.True(File.Exists(dst));

        File.Delete(src);
        File.Delete(dst);
        Directory.Delete(temp, recursive: true);
    }
}
