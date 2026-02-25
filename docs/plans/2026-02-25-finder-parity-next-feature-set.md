# Finder Parity V2 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make Nimbus visually and behaviorally closer to Finder by shipping multi-view browsing, preview/quick look, tabs, tags, and saved search collections.

**Architecture:** Keep filesystem logic in `Nimbus.Core` and keep WinUI orchestration in `Nimbus.App`. Add focused services for view-state preferences, preview metadata, tab state, and saved search collections so the UI remains thin and testable.

**Tech Stack:** C# (.NET 8), WinUI 3, Windows App SDK, xUnit.

---

## Research Inputs (Finder)
- Finder basics and layout: https://support.apple.com/en-us/ht201732
- Finder view options and behavior: https://support.apple.com/en-ae/guide/mac-help/mchldaafb302/mac
- Finder preview pane: https://support.apple.com/en-ng/guide/mac-help/mchlp1000/mac
- Quick Look behavior: https://support.apple.com/en-om/guide/mac-help/mh14119/mac
- Finder quick actions: https://support.apple.com/en-ae/guide/mac-help/mchl97ff9142/mac
- Finder tags: https://support.apple.com/en-al/guide/mac-help/mchlp15236/mac
- Smart folders (saved searches): https://support.apple.com/en-al/guide/mac-help/mchlp2804/mac
- Finder keyboard shortcuts reference baseline: https://support.apple.com/en-ie/102650

## Finder Parity Gaps (Current Nimbus)
- No Finder-like view switcher (`Icon`, `List`, `Column`, `Gallery`).
- No right-side preview pane or spacebar quick look flow.
- No tabbed navigation model.
- Sidebar is flat and missing Finder-style grouped sections (`Favorites`, `Locations`, `Tags`).
- No saved searches/smart collections.
- Layout still reads like a basic utility list rather than Finder-style explorer chrome.

## Feature Set Priority
- P0: Layout shell + view modes + preview pane.
- P1: Quick Look + tabs.
- P2: Tags + smart collections + sort/group and persisted view prefs.

### Task 1: Finder-Like Application Shell

**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml`
- Modify: `src/Nimbus.App/Views/FileListView.xaml`

**Step 1: Build grouped sidebar layout**
- Add section headers (`Favorites`, `Locations`, `Tags`) and spacing/typography for hierarchy.

**Step 2: Build Finder-like top chrome**
- Add view-mode segmented control area and sort/group controls near path/search.

**Step 3: Add right pane host**
- Reserve a right column for preview pane and metadata summary.

**Step 4: Manual verify**
Run: `dotnet run --project src/Nimbus.App/Nimbus.App.csproj`
Expected: app shell resembles Finder information hierarchy (sidebar groups, central browser area, right preview host).

### Task 2: View Modes (Icon/List/Column/Gallery)

**Files:**
- Create: `src/Nimbus.Core/Models/FileViewMode.cs`
- Create: `src/Nimbus.Core/Services/IViewPreferenceService.cs`
- Create: `src/Nimbus.Core/Services/ViewPreferenceService.cs`
- Modify: `src/Nimbus.Core/ViewModels/FileListViewModel.cs`
- Modify: `src/Nimbus.Core/ViewModels/MainPageViewModel.cs`
- Modify: `src/Nimbus.App/Views/FileListView.xaml`
- Modify: `src/Nimbus.App/App.xaml.cs`
- Test: `tests/Nimbus.Tests/ViewModels/FileListViewModelTests.cs`

**Step 1: Add failing tests**
- Require view mode change updates view state and default fallback is `List`.

**Step 2: Add core model/service**
- Add enum + per-path view preference service.

**Step 3: Bind UI view mode switcher**
- Add UI toggles and mode-specific templates in `FileListView`.

**Step 4: Run tests**
Run: `dotnet test Nimbus.sln`
Expected: tests pass with mode switching and fallback behavior.

### Task 3: Preview Pane + Quick Look

**Files:**
- Create: `src/Nimbus.Core/Services/IFilePreviewService.cs`
- Create: `src/Nimbus.Core/Services/FilePreviewService.cs`
- Create: `src/Nimbus.Core/Models/FilePreviewModel.cs`
- Modify: `src/Nimbus.Core/ViewModels/FileListViewModel.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Test: `tests/Nimbus.Tests/Services/FilePreviewServiceTests.cs`

**Step 1: Add failing tests**
- Preview model returns name/path/type/date and handles missing files safely.

**Step 2: Implement preview service**
- Build metadata preview for folders/files and lightweight text/image preview support.

**Step 3: Add quick look interaction**
- `Space` opens quick look panel/dialog for selected item.

**Step 4: Verify**
Run: `dotnet test Nimbus.sln`
Expected: preview tests pass; quick look can open/close from keyboard.

### Task 4: Tabbed Navigation

**Files:**
- Create: `src/Nimbus.Core/Models/ExplorerTabModel.cs`
- Create: `src/Nimbus.Core/ViewModels/TabsViewModel.cs`
- Modify: `src/Nimbus.Core/ViewModels/MainPageViewModel.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Test: `tests/Nimbus.Tests/ViewModels/TabsViewModelTests.cs`

**Step 1: Add failing tests**
- New tab opens same folder as current tab and close preserves neighboring active tab.

**Step 2: Implement tab VM**
- Add `New Tab`, `Close Tab`, `Switch Tab`, and tab-local navigation state.

**Step 3: Bind tab strip UI**
- Add tab strip and keyboard shortcuts (`Ctrl+T`, `Ctrl+W`, `Ctrl+Tab`).

**Step 4: Verify**
Run: `dotnet test Nimbus.sln`
Expected: tab tests pass; tab operations stable.

### Task 5: Tags + Smart Collections

**Files:**
- Create: `src/Nimbus.Core/Models/SavedSearchModel.cs`
- Create: `src/Nimbus.Core/Services/ISavedSearchService.cs`
- Create: `src/Nimbus.Core/Services/SavedSearchService.cs`
- Modify: `src/Nimbus.Core/ViewModels/SidebarViewModel.cs`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Test: `tests/Nimbus.Tests/Services/SavedSearchServiceTests.cs`

**Step 1: Add failing tests**
- Saved search can be created, listed, and resolved to root path + query.

**Step 2: Implement saved-search service**
- Persist saved searches in local app settings/json.

**Step 3: Add tags/smart collections sections**
- Render in sidebar and execute search when selected.

**Step 4: Verify**
Run: `dotnet test Nimbus.sln`
Expected: saved-search tests pass and sidebar collections execute correctly.

### Task 6: Finder-Like Polish and Regression

**Files:**
- Modify: `docs/regression-checklist-v1.md`
- Create: `docs/regression-checklist-v2-finder-parity.md`
- Modify: `ROADMAP.md`

**Step 1: Expand manual regression**
- Cover view switching, preview/quick look, tab operations, and saved searches.

**Step 2: Add parity acceptance criteria**
- Define screenshot-based checklist for shell hierarchy and interaction flow.

**Step 3: Final verify**
Run: `dotnet test Nimbus.sln`
Expected: all tests pass and parity checklist is executable.

## V2 Acceptance Criteria
- User can switch between at least 4 Finder-style views.
- Preview pane updates with selection and quick look is keyboard-first.
- Tabs behave independently with stable navigation history per tab.
- Sidebar has grouped hierarchy with tags and smart collections.
- Layout and interactions feel Finder-inspired rather than baseline list utility.
