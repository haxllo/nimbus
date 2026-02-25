using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nimbus.Core.Models;

public sealed class ExplorerTabModel : INotifyPropertyChanged
{
    private NavigationState _navigationState;
    private string _title;

    public ExplorerTabModel(Guid id, NavigationState navigationState)
    {
        Id = id;
        _navigationState = navigationState;
        _title = BuildTitle(navigationState.CurrentPath);
    }

    public Guid Id { get; }

    public string Title
    {
        get => _title;
        private set
        {
            if (string.Equals(_title, value, StringComparison.Ordinal))
            {
                return;
            }

            _title = value;
            OnPropertyChanged();
        }
    }

    public string? CurrentPath => _navigationState.CurrentPath;

    public NavigationState NavigationState => _navigationState;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateNavigationState(NavigationState navigationState)
    {
        _navigationState = navigationState;
        Title = BuildTitle(navigationState.CurrentPath);
        OnPropertyChanged(nameof(CurrentPath));
        OnPropertyChanged(nameof(NavigationState));
    }

    public override string ToString() => Title;

    private static string BuildTitle(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "New Tab";
        }

        var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var leafName = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(leafName) ? path : leafName;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
