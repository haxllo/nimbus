namespace Nimbus.Core.Models;

public sealed class BreadcrumbItem
{
    public BreadcrumbItem(string label, string path)
    {
        Label = label;
        Path = path;
    }

    public string Label { get; }

    public string Path { get; }
}
