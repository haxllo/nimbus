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
  - UX milestones (breadcrumb actions, file operation UI flow, shortcuts) are incomplete.

## Milestones

### M1: Core Stability and Reliability (In Progress)
Scope:
- Harden folder enumeration and search behavior against missing/inaccessible paths.
- Ensure navigation history reloads the visible file list.
- Expand deterministic tests for service and view-model behavior.

Status:
- [x] `ShellItemService` now returns safe empty results for invalid/unavailable paths and sorts output.
- [x] `SearchService` supports wildcard and plain-text queries; safely skips inaccessible directories.
- [x] `MainPageViewModel` now has safe async navigation/back/forward flows that reload list state.
- [x] Added tests for shell enumeration ordering/failures, search query modes, and main-page navigation behavior.
- [ ] Add file operation error-result model (non-throwing UI feedback contract).

Exit Criteria:
- No unhandled exceptions when navigating missing or inaccessible folders.
- Search behaves consistently for both `*.ext` and plain text queries.
- New test suite covers these scenarios.

### M2: Finder-Like Essentials (Planned)
Scope:
- Breadcrumb UI with clickable segments.
- Toolbar actions for New Folder, Rename, Delete, Refresh.
- Keyboard shortcuts (`Alt+Left`, `Alt+Right`, `F2`, `Delete`, `Ctrl+F`).

Exit Criteria:
- Full keyboard navigation for core actions.
- Breadcrumb clicks navigate correctly and update history/list.

### M3: V1 Completion (Planned)
Scope:
- File operation UX (confirmation, conflict handling, status messages).
- Packaging and branding polish (`Package.appxmanifest`, assets).
- Final regression pass on large folders and permission boundaries.

Exit Criteria:
- V1 acceptance criteria met in manual verification + automated tests.

## V1 Acceptance Criteria
- Navigate common roots (`C:\`, user profile, Documents, Downloads, Pictures) without app crash.
- Back/forward history always updates the list view to the active location.
- Search returns expected matches for wildcard and plain-text queries.
- File operations succeed in temp directories and provide actionable errors on failures.

## Immediate Next Tasks
1. Add operation result model (`Success`, `ErrorCode`, `Message`) for `IFileOperationsService`.
2. Surface operation/search/navigation errors in UI status area instead of silent failures.
3. Implement breadcrumb segment control with click-to-navigate.
4. Add targeted tests for file operation failures (`UnauthorizedAccessException`, missing source, conflicts).
