# Roadmap Hardening V1 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Stabilize Nimbus core navigation and search behavior, then finish V1 reliability and UX-critical tasks.

**Architecture:** Keep reliability logic in `Nimbus.Core` services/view-models and keep UI event handlers thin in `Nimbus.App`. Test all core behavior in `Nimbus.Tests` with deterministic temp-directory fixtures.

**Tech Stack:** C# (.NET 8), WinUI 3, xUnit.

---

### Task 1: Navigation Flow Hardening

**Files:**
- Modify: `src/Nimbus.Core/ViewModels/MainPageViewModel.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Test: `tests/Nimbus.Tests/ViewModels/MainPageViewModelTests.cs`

**Step 1: Write failing tests for missing path and history reload**
- Add tests that require `NavigateToAsync` to return `false` on invalid paths.
- Add tests that require `GoBackAsync`/`GoForwardAsync` to reload list state.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL with missing async navigation methods.

**Step 3: Write minimal implementation**
- Add `NavigateToAsync(...): Task<bool>`, `GoBackAsync`, `GoForwardAsync`.
- Update `MainPage.xaml.cs` to call these methods.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

### Task 2: Shell Enumeration Resilience

**Files:**
- Modify: `src/Nimbus.Core/Services/ShellItemService.cs`
- Test: `tests/Nimbus.Tests/Services/ShellItemServiceTests.cs`

**Step 1: Write failing tests for sorted output and missing folder behavior**
- Require folders before files, each alphabetical.
- Require empty result for missing path.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL on ordering/missing path behavior.

**Step 3: Write minimal implementation**
- Validate input path.
- Catch filesystem exceptions and return empty.
- Sort folders first, then alphabetical by display name.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

### Task 3: Search Query Semantics

**Files:**
- Modify: `src/Nimbus.Core/Services/SearchService.cs`
- Modify: `tests/Nimbus.Tests/Services/SearchServiceTests.cs`

**Step 1: Write failing tests for plain text query and missing root**
- `SearchAsync(root, "notes")` should match `project-notes.md`.
- Missing root should return empty.

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: FAIL on plain text and missing root expectations.

**Step 3: Write minimal implementation**
- Distinguish wildcard and plain text query modes.
- Use safe directory/file enumeration wrappers.
- Return deduplicated, ordered results.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Nimbus.Tests/Nimbus.Tests.csproj`
Expected: PASS.

### Task 4: Remaining V1 Reliability Work

**Files:**
- Modify: `src/Nimbus.Core/Services/IFileOperationsService.cs`
- Modify: `src/Nimbus.Core/Services/FileOperationsService.cs`
- Modify: `src/Nimbus.App/Views/MainPage.xaml`
- Modify: `src/Nimbus.App/Views/MainPage.xaml.cs`
- Test: `tests/Nimbus.Tests/Services/FileOperationsServiceTests.cs`

**Step 1: Add operation result contract**
- Introduce explicit operation results (`Success`, `Error`, `Message`) to avoid opaque exceptions in UI.

**Step 2: Add UI status rendering**
- Show operation/search/navigation failures in a status area.

**Step 3: Extend tests**
- Add failures for missing source, unauthorized paths, and overwrite conflicts.

**Step 4: Verify**

Run: `dotnet test Nimbus.sln`
Expected: PASS.
