using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nimbus.Core.Models;

namespace Nimbus.Core.ViewModels;

public sealed class TabsViewModel : INotifyPropertyChanged
{
    private ExplorerTabModel? _activeTab;

    public ObservableCollection<ExplorerTabModel> Tabs { get; } = new();

    public ExplorerTabModel? ActiveTab
    {
        get => _activeTab;
        private set
        {
            if (ReferenceEquals(_activeTab, value))
            {
                return;
            }

            _activeTab = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanCloseActiveTab));
        }
    }

    public bool CanCloseActiveTab => Tabs.Count > 1 && ActiveTab is not null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ExplorerTabModel EnsureInitialized(NavigationState initialState)
    {
        if (Tabs.Count > 0)
        {
            ActiveTab ??= Tabs[0];
            return ActiveTab!;
        }

        var firstTab = new ExplorerTabModel(Guid.NewGuid(), initialState);
        Tabs.Add(firstTab);
        ActiveTab = firstTab;
        return firstTab;
    }

    public ExplorerTabModel OpenTab(NavigationState initialState, bool activate = true)
    {
        var tab = new ExplorerTabModel(Guid.NewGuid(), initialState);
        var insertIndex = Tabs.Count;
        if (ActiveTab is not null)
        {
            insertIndex = Math.Min(Tabs.IndexOf(ActiveTab) + 1, Tabs.Count);
        }

        Tabs.Insert(insertIndex, tab);
        if (activate)
        {
            ActiveTab = tab;
        }

        OnPropertyChanged(nameof(CanCloseActiveTab));
        return tab;
    }

    public bool ActivateTab(Guid tabId)
    {
        var tab = Tabs.FirstOrDefault(item => item.Id == tabId);
        if (tab is null)
        {
            return false;
        }

        ActiveTab = tab;
        return true;
    }

    public bool ActivateNextTab()
    {
        if (Tabs.Count < 2 || ActiveTab is null)
        {
            return false;
        }

        var currentIndex = Tabs.IndexOf(ActiveTab);
        if (currentIndex < 0)
        {
            ActiveTab = Tabs[0];
            return true;
        }

        var nextIndex = (currentIndex + 1) % Tabs.Count;
        ActiveTab = Tabs[nextIndex];
        return true;
    }

    public bool ActivatePreviousTab()
    {
        if (Tabs.Count < 2 || ActiveTab is null)
        {
            return false;
        }

        var currentIndex = Tabs.IndexOf(ActiveTab);
        if (currentIndex < 0)
        {
            ActiveTab = Tabs[0];
            return true;
        }

        var previousIndex = (currentIndex - 1 + Tabs.Count) % Tabs.Count;
        ActiveTab = Tabs[previousIndex];
        return true;
    }

    public bool CloseTab(Guid tabId)
    {
        if (Tabs.Count <= 1)
        {
            return false;
        }

        var index = -1;
        for (var i = 0; i < Tabs.Count; i++)
        {
            if (Tabs[i].Id == tabId)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return false;
        }

        var closingTab = Tabs[index];
        var wasActive = ReferenceEquals(closingTab, ActiveTab);
        Tabs.RemoveAt(index);

        if (wasActive)
        {
            var activeIndex = Math.Min(index, Tabs.Count - 1);
            ActiveTab = Tabs[activeIndex];
        }

        if (ActiveTab is null && Tabs.Count > 0)
        {
            ActiveTab = Tabs[0];
        }

        OnPropertyChanged(nameof(CanCloseActiveTab));
        return true;
    }

    public bool CloseActiveTab()
    {
        var tab = ActiveTab;
        if (tab is null)
        {
            return false;
        }

        return CloseTab(tab.Id);
    }

    public void UpdateActiveState(NavigationState state)
    {
        if (ActiveTab is null)
        {
            EnsureInitialized(state);
            return;
        }

        ActiveTab.UpdateNavigationState(state);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
