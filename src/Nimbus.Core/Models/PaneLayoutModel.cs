namespace Nimbus.Core.Models;

public sealed record class PaneLayoutModel
{
    public double SidebarWidth { get; init; } = 260;

    public double PreviewWidth { get; init; } = 300;
}
