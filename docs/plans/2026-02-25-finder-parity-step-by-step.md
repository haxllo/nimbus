# Finder Parity Recovery + Execution Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Complete Finder Parity V2 in safe, build-stable increments after rolling back to the last known-good UI state.

**Architecture:** Keep all filesystem/search/saved-search logic in `Nimbus.Core`, then bind thin WinUI handlers in `Nimbus.App`. Re-introduce risky UI features (preview quick-look and tabs) in very small slices to avoid silent WinUI XAML compiler failures.

**Tech Stack:** C# (.NET 8), WinUI 3, Windows App SDK, xUnit.

---

## Task 5: Tags + Smart Collections

### Task 5A: Saved Search Core (Completed)
**Files:**
- Create: `src/Nimbus.Core/Models/SavedSearchModel.cs`
- Create: `src/Nimbus.Core/Services/ISavedSearchService.cs`
- Create: `src/Nimbus.Core/Services/SavedSearchService.cs`
- Test: `tests/Nimbus.Tests/Services/SavedSearchServiceTests.cs`

### Task 5B: Sidebar Smart Collections (Completed)
**Files:**
- Modify: `src/Nimbus.Core/ViewModels/SidebarViewModel.cs`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml.cs`
- Modify: `src/Nimbus.App/App.xaml.cs`
- Test updates: `tests/Nimbus.Tests/Services/SidebarViewModelTests.cs`
- Test updates: `tests/Nimbus.Tests/ViewModels/MainPageViewModelTests.cs`
- Test updates: `tests/Nimbus.Tests/ViewModels/MainPageViewModelOperationsTests.cs`

### Task 5C: Execute Smart Collection Selection (Completed)
**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`

### Task 5D: Manual Verification (Pending)
**Steps:**
1. Build and run app.
2. Click each Smart Collection in sidebar.
3. Confirm root path opens, search query is executed, and result list updates.
4. Verify failure message appears when saved-search root path is unavailable.

### Task 5E: Replace Placeholder Tags with Real Tag Model (Completed)
**Files:**
- Create: `src/Nimbus.Core/Models/FileTagModel.cs`
- Create: `src/Nimbus.Core/Services/ITagService.cs`
- Create: `src/Nimbus.Core/Services/TagService.cs`
- Modify: `src/Nimbus.Core/ViewModels/SidebarViewModel.cs`
- Modify: `src/Nimbus.App/App.xaml.cs`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml`
- Modify: `src/Nimbus.App/Views/SidebarView.xaml.cs`
- Test: `tests/Nimbus.Tests/Services/TagServiceTests.cs`
- Test: `tests/Nimbus.Tests/Services/SidebarViewModelTests.cs`

---

## Task 4 Recovery: Tab UI Re-Integration (Pending)

### Task 4R1: Re-introduce tab controls without new XAML event signatures (Completed)
**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`

### Task 4R2: Add safe tab keyboard handling in code-behind (Completed)
**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`

### Task 4R3: Regression check for XAML compiler stability
**Steps:**
1. `dotnet build src/Nimbus.App/Nimbus.App.csproj`
2. If failed: run `pwsh -File scripts/diagnose-xaml-compiler.ps1`
3. Keep reducing tab UI markup until build stability is confirmed.

---

## Task 3 Recovery: Quick Look UI Re-Integration (Pending)

### Task 3R1: Restore `Space` accelerator and preview dialog handler (Completed)
**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`

### Task 3R2: Bind preview pane to `CurrentPreview` model (Completed)
**Files:**
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`

---

## Task 6: Finder Parity Regression + Docs (In Progress)

### Task 6A: Add parity checklist document
**Files:**
- Create: `docs/regression-checklist-v2-finder-parity.md`

Status: Completed

### Task 6B: Update roadmap with true status and next queue
**Files:**
- Modify: `ROADMAP.md`

Status: Completed

---

## Post-Plan Enhancements (Completed)

### Enhancement A: Sort Selector Behavior
**Files:**
- Create: `src/Nimbus.Core/Models/FileSortField.cs`
- Modify: `src/Nimbus.Core/ViewModels/FileListViewModel.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Modify: `tests/Nimbus.Tests/ViewModels/FileListViewModelTests.cs`

### Enhancement B: File List Icon/Thumbnail Fallback
**Files:**
- Modify: `src/Nimbus.Core/Models/ShellItemModel.cs`
- Modify: `src/Nimbus.Core/Services/ShellItemService.cs`
- Modify: `src/Nimbus.App/Views/FileListView.xaml`
- Modify: `tests/Nimbus.Tests/Services/ShellItemServiceTests.cs`

### Enhancement C: Resizable + Persisted Pane Layout
**Files:**
- Create: `src/Nimbus.Core/Models/PaneLayoutModel.cs`
- Create: `src/Nimbus.Core/Services/IPaneLayoutService.cs`
- Create: `src/Nimbus.Core/Services/PaneLayoutService.cs`
- Modify: `src/Nimbus.App/App.xaml.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Create: `tests/Nimbus.Tests/Services/PaneLayoutServiceTests.cs`
