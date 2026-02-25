using Nimbus.Core.Services;

namespace Nimbus.Tests.Services;

public class TagServiceTests
{
    [Fact]
    public void GetAll_Returns_Default_Tags_With_Required_Fields()
    {
        var service = new TagService();

        var tags = service.GetAll();
        Assert.NotEmpty(tags);
        Assert.All(tags, tag =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tag.Id));
            Assert.False(string.IsNullOrWhiteSpace(tag.DisplayName));
            Assert.False(string.IsNullOrWhiteSpace(tag.RootPath));
            Assert.False(string.IsNullOrWhiteSpace(tag.Query));
        });
    }

    [Fact]
    public void Resolve_Matches_Identifier_Case_Insensitively()
    {
        var service = new TagService();
        var existing = service.GetAll();
        Assert.NotEmpty(existing);

        var resolved = service.Resolve(existing[0].Id.ToUpperInvariant());

        Assert.NotNull(resolved);
        Assert.Equal(existing[0].Id, resolved!.Id, StringComparer.OrdinalIgnoreCase);
    }
}
