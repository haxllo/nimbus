# Finder-Style Windows Explorer v1 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Windows-native file explorer inspired by macOS Finder behavior, with a distinct name and Windows shell integration.

**Architecture:** WinUI 3 desktop app using MVVM. Core services wrap Windows Shell APIs for enumeration, icons, operations, and search. UI is a shell-aware navigation frame with sidebar, breadcrumb, and a virtualized file list.

**Tech Stack:** WinUI 3 (.NET 8/9), Windows App SDK, C#, CommunityToolkit.Mvvm, CsWin32 or Vanara for Shell APIs, xUnit for unit tests.

---

## Summary
- Create a WinUI 3 app that provides a Finder-like experience on Windows with a distinct brand name.
- Implement shell-backed navigation, sidebar favorites, list/icon views, breadcrumb path bar, and core file operations.
- Add search in v1; preview pane, tabs, and column view are out of scope unless explicitly added later.

## Public Interfaces and Types
- `IShellItemService`: Enumerate folders, get icons, metadata, and resolve special locations.
- `INavigationService`: Navigate to path, back/forward history, and breadcrumbs.
- `IFileOperationsService`: Copy, move, rename, delete using Windows shell operations.
- `ISearchService`: Query Windows Search index or fallback to filesystem search.
- `ShellItemModel`: Represents file/folder metadata and UI state.
- `SidebarLocation`: Known folders, drives, and user-pinned items.

## Assumptions and Defaults
- App name is a placeholder codename `Nimbus` until you pick a final name.
- Target OS is Windows 10 + 11.
- v1 scope is `Essentials`: sidebar, list/icon view, breadcrumb path, basic file operations, and search.
- No Apple branding or exact visual assets; behavior and UX patterns are inspired but distinct.
- Plan is created in the current repo without a separate worktree.

### Task 1: Scaffold WinUI 3 Solution
**Files:**
- Create: `Nimbus.sln`
- Create: `src/Nimbus.App/`
- Create: `tests/Nimbus.Tests/`

**Step 1: Create WinUI 3 project**
Run: `dotnet new winui3 -n Nimbus.App -o src/Nimbus.App`
Expected: WinUI 3 desktop app project created.

**Step 2: Create solution and add projects**
Run:
- `dotnet new sln -n Nimbus`
- `dotnet sln Nimbus.sln add src/Nimbus.App/Nimbus.App.csproj`

**Step 3: Create test project**
Run:
- `dotnet new xunit -n Nimbus.Tests -o tests/Nimbus.Tests`
- `dotnet sln Nimbus.sln add tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: xUnit test project added.

**Step 4: Commit**
Run:
- `git add Nimbus.sln src/Nimbus.App tests/Nimbus.Tests`
- `git commit -m "chore: scaffold WinUI3 app and tests"`

### Task 2: Core Domain Models
**Files:**
- Create: `src/Nimbus.App/Models/ShellItemModel.cs`
- Create: `src/Nimbus.App/Models/SidebarLocation.cs`
- Test: `tests/Nimbus.Tests/Models/ShellItemModelTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public void ShellItemModel_Has_Required_Properties()
{
    var item = new ShellItemModel("C:\\");
    Assert.Equal("C:\\", item.Path);
    Assert.NotNull(item.DisplayName);
    Assert.True(item.IsFolder);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL with missing type.

**Step 3: Write minimal implementation**
```csharp
public sealed class ShellItemModel
{
    public ShellItemModel(string path) { Path = path; }
    public string Path { get; }
    public string DisplayName { get; set; } = "";
    public bool IsFolder { get; set; }
    public long? SizeBytes { get; set; }
    public DateTimeOffset? DateModified { get; set; }
    public string? IconKey { get; set; }
}
```

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**
Run:
- `git add src/Nimbus.App/Models tests/Nimbus.Tests/Models`
- `git commit -m "feat: add shell item models"`

### Task 3: Shell Item Service (Enumeration + Metadata)
**Files:**
- Create: `src/Nimbus.App/Services/IShellItemService.cs`
- Create: `src/Nimbus.App/Services/ShellItemService.cs`
- Test: `tests/Nimbus.Tests/Services/ShellItemServiceTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public async Task EnumerateFolder_Returns_Items()
{
    var svc = new ShellItemService();
    var items = await svc.EnumerateAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    Assert.NotEmpty(items);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL.

**Step 3: Write minimal implementation**
- Use `Directory.EnumerateFileSystemEntries` for MVP.
- Map to `ShellItemModel` with basic metadata.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

### Task 4: Navigation Service + Breadcrumbs
**Files:**
- Create: `src/Nimbus.App/Services/INavigationService.cs`
- Create: `src/Nimbus.App/Services/NavigationService.cs`
- Create: `src/Nimbus.App/ViewModels/NavigationViewModel.cs`
- Test: `tests/Nimbus.Tests/Services/NavigationServiceTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public void BackForward_Tracks_History()
{
    var nav = new NavigationService();
    nav.NavigateTo("C:\\");
    nav.NavigateTo("C:\\Users");
    Assert.True(nav.CanGoBack);
    nav.GoBack();
    Assert.Equal("C:\\", nav.CurrentPath);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL.

**Step 3: Write minimal implementation**
Implement stacks for back/forward and expose breadcrumb segments as a list.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

### Task 5: Sidebar Favorites
**Files:**
- Create: `src/Nimbus.App/ViewModels/SidebarViewModel.cs`
- Create: `src/Nimbus.App/Views/SidebarView.xaml`
- Modify: `src/Nimbus.App/MainWindow.xaml`

**Step 1: Write the failing test**
```csharp
[Fact]
public void Sidebar_Includes_Known_Folders()
{
    var vm = new SidebarViewModel();
    Assert.Contains(vm.Locations, l => l.Id == "Documents");
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL.

**Step 3: Write minimal implementation**
Populate known folders and drives using `Environment.SpecialFolder` + `DriveInfo`.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

### Task 6: File List View (List + Icons)
**Files:**
- Create: `src/Nimbus.App/ViewModels/FileListViewModel.cs`
- Create: `src/Nimbus.App/Views/FileListView.xaml`
- Modify: `src/Nimbus.App/MainWindow.xaml`

**Step 1: Implement virtualized list with columns**
**Step 2: Bind selection + double-click to navigation**
**Step 3: Manual test**
- Open folders with 1k+ files and verify scrolling stays smooth.

### Task 7: File Operations (Copy/Move/Rename/Delete)
**Files:**
- Create: `src/Nimbus.App/Services/IFileOperationsService.cs`
- Create: `src/Nimbus.App/Services/FileOperationsService.cs`
- Test: `tests/Nimbus.Tests/Services/FileOperationsServiceTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public async Task CopyFile_Creates_Destination()
{
    var svc = new FileOperationsService();
    var temp = Path.GetTempPath();
    var src = Path.Combine(temp, "nimbus-src.txt");
    var dst = Path.Combine(temp, "nimbus-dst.txt");
    File.WriteAllText(src, "x");
    await svc.CopyAsync(src, dst);
    Assert.True(File.Exists(dst));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL.

**Step 3: Write minimal implementation**
Implement copy/move/rename/delete with `File` and `Directory` operations; replace with shell operations later.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

### Task 8: Search
**Files:**
- Create: `src/Nimbus.App/Services/ISearchService.cs`
- Create: `src/Nimbus.App/Services/SearchService.cs`
- Modify: `src/Nimbus.App/ViewModels/NavigationViewModel.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public async Task Search_Returns_Matches()
{
    var svc = new SearchService();
    var results = await svc.SearchAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "*.txt");
    Assert.NotNull(results);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL.

**Step 3: Write minimal implementation**
Implement simple filesystem search with cancellation; add Windows Search index integration later.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

**Step 5: Commit**

### Task 9: Polish + Packaging
**Files:**
- Modify: `src/Nimbus.App/Assets/`
- Modify: `src/Nimbus.App/Package.appxmanifest`

**Step 1: Add app icon + name**
**Step 2: Set window title and branding**
**Step 3: Build Release**
Run: `dotnet build src/Nimbus.App/Nimbus.App.csproj -c Release`
Expected: Build succeeds.

## Test Cases and Scenarios
- Open app and navigate `C:\\` and `Users` without errors.
- Back/forward history works across 5 navigations.
- Sidebar click navigates to Documents, Downloads, Pictures.
- List view shows icons and metadata columns.
- Copy/move/rename/delete works in a temp folder.
- Search returns files from user profile.

