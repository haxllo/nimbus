using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class ItemLaunchServiceTests
{
    [Fact]
    public async Task LaunchAsync_Empty_Path_Returns_InvalidInput()
    {
        var service = new ItemLaunchService();

        var result = await service.LaunchAsync("  ");

        Assert.False(result.IsSuccess);
        Assert.Equal(FileOperationErrorCode.InvalidInput, result.ErrorCode);
    }

    [Fact]
    public async Task LaunchAsync_Missing_Path_Returns_NotFound()
    {
        var service = new ItemLaunchService();
        var missingPath = Path.Combine(Path.GetTempPath(), $"nimbus-missing-{Guid.NewGuid():N}.txt");

        var result = await service.LaunchAsync(missingPath);

        Assert.False(result.IsSuccess);
        Assert.Equal(FileOperationErrorCode.NotFound, result.ErrorCode);
    }
}
