using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class FilePreviewServiceTests
{
    [Fact]
    public async Task GetPreviewAsync_TextFile_Returns_Metadata_And_TextPreview()
    {
        var service = new FilePreviewService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-preview-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var filePath = Path.Combine(tempRoot, "notes.txt");
        await File.WriteAllTextAsync(filePath, "hello from nimbus preview");

        try
        {
            var preview = await service.GetPreviewAsync(filePath);

            Assert.Equal("notes.txt", preview.Name);
            Assert.False(preview.IsFolder);
            Assert.Equal(".TXT", preview.ItemType);
            Assert.NotNull(preview.DateModified);
            Assert.NotNull(preview.SizeBytes);
            Assert.Contains("nimbus preview", preview.TextPreview);
            Assert.Null(preview.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GetPreviewAsync_ImageFile_Marks_Image_Preview()
    {
        var service = new FilePreviewService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-preview-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var filePath = Path.Combine(tempRoot, "cover.png");
        await File.WriteAllTextAsync(filePath, "not-real-image-bytes");

        try
        {
            var preview = await service.GetPreviewAsync(filePath);

            Assert.True(preview.IsImage);
            Assert.Equal(filePath, preview.ImagePath);
            Assert.Null(preview.TextPreview);
            Assert.Null(preview.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GetPreviewAsync_Directory_Returns_Folder_Preview()
    {
        var service = new FilePreviewService();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"nimbus-preview-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var preview = await service.GetPreviewAsync(tempRoot);

            Assert.True(preview.IsFolder);
            Assert.Equal("Folder", preview.ItemType);
            Assert.NotNull(preview.DateModified);
            Assert.Equal(tempRoot, preview.Path);
            Assert.Null(preview.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GetPreviewAsync_Missing_Path_Returns_Error_Model()
    {
        var service = new FilePreviewService();
        var missingPath = Path.Combine(Path.GetTempPath(), $"nimbus-missing-{Guid.NewGuid():N}");

        var preview = await service.GetPreviewAsync(missingPath);

        Assert.False(preview.IsFolder);
        Assert.Equal("Item not found.", preview.ErrorMessage);
    }
}
