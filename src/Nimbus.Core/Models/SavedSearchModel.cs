namespace Nimbus.Core.Models;

public sealed record class SavedSearchModel
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string RootPath { get; init; } = string.Empty;

    public string Query { get; init; } = string.Empty;
}
