using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class FileOperationsServiceTests
{
    [Fact]
    public async Task CreateDirectoryAsync_Creates_Folder()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var result = await svc.CreateDirectoryAsync(temp, "New Folder");

            Assert.True(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.None, result.ErrorCode);
            Assert.True(Directory.Exists(Path.Combine(temp, "New Folder")));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CopyFile_Creates_Destination()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var src = Path.Combine(temp, "nimbus-src.txt");
            var dst = Path.Combine(temp, "nimbus-dst.txt");

            await File.WriteAllTextAsync(src, "x");
            var result = await svc.CopyAsync(src, dst);

            Assert.True(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.None, result.ErrorCode);
            Assert.True(File.Exists(dst));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CopyAsync_Missing_Source_Returns_NotFound()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var missingSource = Path.Combine(temp, "missing-source.txt");
            var destination = Path.Combine(temp, "destination.txt");

            var result = await svc.CopyAsync(missingSource, destination);

            Assert.False(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.NotFound, result.ErrorCode);
            Assert.False(File.Exists(destination));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CopyAsync_When_Destination_Already_Exists_Returns_Conflict()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);

        try
        {
            var source = Path.Combine(temp, "source.txt");
            var destination = Path.Combine(temp, "destination.txt");

            await File.WriteAllTextAsync(source, "source-content");
            await File.WriteAllTextAsync(destination, "existing-content");

            var result = await svc.CopyAsync(source, destination);

            Assert.False(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.Conflict, result.ErrorCode);
            Assert.Equal("existing-content", await File.ReadAllTextAsync(destination));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public void MapException_UnauthorizedAccess_Returns_AccessDenied()
    {
        var result = FileOperationsService.MapException("copy", new UnauthorizedAccessException("denied"));

        Assert.False(result.IsSuccess);
        Assert.Equal(FileOperationErrorCode.AccessDenied, result.ErrorCode);
    }

    [Fact]
    public async Task RenameAsync_Invalid_Name_Returns_InvalidInput()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        var source = Path.Combine(temp, "source.txt");
        await File.WriteAllTextAsync(source, "x");

        try
        {
            var invalidName = $"invalid{Path.DirectorySeparatorChar}name.txt";
            var result = await svc.RenameAsync(source, invalidName);

            Assert.False(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.InvalidInput, result.ErrorCode);
            Assert.True(File.Exists(source));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RenameAsync_When_Target_Exists_Returns_Conflict()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        var source = Path.Combine(temp, "source.txt");
        var existingTarget = Path.Combine(temp, "target.txt");
        await File.WriteAllTextAsync(source, "x");
        await File.WriteAllTextAsync(existingTarget, "x");

        try
        {
            var result = await svc.RenameAsync(source, "target.txt");

            Assert.False(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.Conflict, result.ErrorCode);
            Assert.True(File.Exists(source));
            Assert.True(File.Exists(existingTarget));
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_Missing_Path_Returns_NotFound()
    {
        var svc = new FileOperationsService();
        var temp = Path.Combine(Path.GetTempPath(), $"nimbus-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        var missingPath = Path.Combine(temp, "missing.txt");

        try
        {
            var result = await svc.DeleteAsync(missingPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(FileOperationErrorCode.NotFound, result.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CopyAsync_Cancelled_Token_Returns_Cancelled()
    {
        var svc = new FileOperationsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await svc.CopyAsync("a", "b", cts.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(FileOperationErrorCode.Cancelled, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_Cancelled_Token_Returns_Cancelled()
    {
        var svc = new FileOperationsService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await svc.DeleteAsync("a", cts.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(FileOperationErrorCode.Cancelled, result.ErrorCode);
    }
}
