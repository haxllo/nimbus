# Nimbus Roadmap (Quality-Gated)

## Roadmap Quality Check (2026-02-25)
- Previous roadmap had solid feature direction.
- Previous roadmap did not define implementation status, quality gates, or test coverage targets.
- Roadmap is now rewritten to be execution-ready: milestone status, acceptance criteria, and concrete tasks.

## Product Goal
- Ship a stable WinUI 3 file explorer with Finder-inspired workflow on Windows 10/11, while keeping architecture split between `Nimbus.App` (UI) and `Nimbus.Core` (testable logic).

## Engineering Principles
- Keep domain and filesystem logic in `Nimbus.Core`.
- UI should orchestrate only; services handle edge cases and filesystem exceptions.
- Every milestone must include automated tests in `tests/Nimbus.Tests`.
- Prefer incremental slices that are shippable and reversible.

## Baseline Status (Current)
- Implemented:
  - Core models (`ShellItemModel`, `SidebarLocation`).
  - Navigation, file operations, and search services.
  - Main page with sidebar + file list + path/search inputs.
- Gaps before V1 quality:
  - Error-resilient navigation and search behavior was incomplete.
  - Tests were minimal and did not cover key failure/edge paths.
  - Visual parity with Finder-style hierarchy and view behavior is still low.

## Milestones

### M1: Core Stability and Reliability (In Progress)
Scope:
- Harden folder enumeration and search behavior against missing/inaccessible paths.
- Ensure navigation history reloads the visible file list.
- Expand deterministic tests for service and view-model behavior.

Status:
- [x] `ShellItemService` now returns safe empty results for invalid/unavailable paths and sorts output.
- [x] `SearchService` supports wildcard and plain-text queries; safely skips inaccessible directories.
- [x] `SearchService` scan path optimized to reduce per-directory allocations and improve cancellation responsiveness.
- [x] Search execution moved off UI thread with explicit cancel/replace flow in `MainPage`.
- [x] `MainPageViewModel` now has safe async navigation/back/forward flows that reload list state.
- [x] Added tests for shell enumeration ordering/failures, search query modes, and main-page navigation behavior.
- [x] Added file operation error-result model (`FileOperationResult`) with explicit error codes.
- [x] Added UI status bar feedback for navigation, search, and file operation failures.
- [x] Added file operation failure tests for missing source, destination conflicts, and unauthorized access mapping.
- [x] Added rename/delete/cancellation edge-case tests in `FileOperationsServiceTests`.
- [x] Added integration-style `MainPageViewModel` tests for create/rename/delete operation flows.

Exit Criteria:
- No unhandled exceptions when navigating missing or inaccessible folders.
- Search behaves consistently for both `*.ext` and plain text queries.
- New test suite covers these scenarios.

### M2: Finder-Like Essentials (Planned)
Scope:
- Breadcrumb UI with clickable segments.
- Toolbar actions for New Folder, Rename, Delete, Refresh.
- Keyboard shortcuts (`Alt+Left`, `Alt+Right`, `F2`, `F5`, `Ctrl+N`, `Delete`, `Ctrl+F`).

Status:
- [x] Breadcrumb UI with clickable segments.
- [x] Toolbar actions for New Folder, Rename, Delete, Refresh.
- [x] Keyboard shortcuts (`Alt+Left`, `Alt+Right`, `F5`, `Ctrl+N`, `Delete`, `Ctrl+F`).
- [x] Rename shortcut (`F2`).
- [x] Added shortcut help tooltips in toolbar/search UI.

Exit Criteria:
- Full keyboard navigation for core actions.
- Breadcrumb clicks navigate correctly and update history/list.

### M3: V1 Completion (In Progress)
Scope:
- File operation UX (confirmation, conflict handling, status messages).
- Packaging and branding polish (`Package.appxmanifest`, assets).
- Final regression pass on large folders and permission boundaries.

Status:
- [x] Delete confirmation dialog.
- [x] Conflict-resolution prompts for create-folder and rename flows.
- [x] Rich status feedback with severity levels (InfoBar).
- [x] Manifest naming polish (`DisplayName`: `Nimbus`, description: `Nimbus File Explorer`).
- [x] Replaced default package assets with Nimbus-branded icons/splash art.
- [x] Added regression-oriented service tests for large-folder enumeration/search consistency.
- [x] Added regression fixture scripts (`scripts/setup-regression-fixtures.ps1`, `scripts/cleanup-regression-fixtures.ps1`).
- [x] Packaging and branding polish (`Package.appxmanifest`, assets).

Exit Criteria:
- V1 acceptance criteria met in manual verification + automated tests.

### M4: Finder Parity V2 (Next)
Scope:
- Finder-like application shell hierarchy (grouped sidebar, central browser, preview pane host).
- View mode system (`Icon`, `List`, `Column`, `Gallery`) with per-path preference memory.
- Preview pane + quick look (`Space`) and quick actions.
- Tabbed navigation (`Ctrl+T`, `Ctrl+W`, `Ctrl+Tab`).
- Sidebar `Tags` and saved-search collections (smart folders).

Status:
- [x] Plan created: `docs/plans/2026-02-25-finder-parity-next-feature-set.md`.
- [x] Task 1: Finder-like shell layout (grouped sidebar, top view/sort controls, preview host first pass).
- [x] Task 2: View modes (`FileViewMode` + path preference service + UI binding first pass).
- [x] Task 3: Preview pane + quick look (`IFilePreviewService` + selection preview loading + `Space` quick look first pass).
- [x] Task 4: Tabbed navigation (`TabsViewModel`, tab strip, `Ctrl+T`/`Ctrl+W`/`Ctrl+Tab` first pass).
- [ ] Task 5: Tags + smart collections.
- [ ] Task 6: Finder parity regression and acceptance pass.

Exit Criteria:
- App interaction flow feels Finder-inspired in layout, browsing, preview, and keyboard usage.
- All new parity flows covered by tests and manual checklist.

## V1 Acceptance Criteria
- Navigate common roots (`C:\`, user profile, Documents, Downloads, Pictures) without app crash.
- Back/forward history always updates the list view to the active location.
- Search returns expected matches for wildcard and plain-text queries.
- File operations succeed in temp directories and provide actionable errors on failures.

## Immediate Next Tasks
1. [ ] Complete M3 regression pass (`docs/regression-checklist-v1.md`).
2. [ ] Execute M4 Task 5: tags + smart collections.
3. [ ] Execute M4 Task 6: finder parity regression and acceptance pass.
