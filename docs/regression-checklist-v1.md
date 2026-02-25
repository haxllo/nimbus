# Nimbus V1 Regression Checklist

Date: 2026-02-25
Scope: Large folders and permission boundaries

## Prerequisites
- Windows 10/11 machine
- `dotnet test Nimbus.sln` passes
- Run Nimbus in unpackaged debug mode

## Fixture Setup
1. Recommended: generate fixtures with:
```powershell
pwsh -File .\scripts\setup-regression-fixtures.ps1 -CreateRestrictedFolder
```
2. Manual alternative: create a temp root folder, for example `C:\Temp\nimbus-regression`.
3. Inside it, create:
- `large-a` with at least 2,000 files and 200 subfolders.
- `large-b` with at least 5,000 files mixed across nested directories.
- `restricted` folder with denied read permission for the current user.
4. Add files with mixed names/extensions (`.txt`, `.json`, `.png`) to test wildcard and plain text search.

## Navigation and Rendering
1. Open `large-a` using the path box and press Enter.
- Expected: app remains responsive; list loads without crash.
2. Open several nested subfolders, then use `Alt+Left` and `Alt+Right`.
- Expected: history updates correctly and list view matches active path.
3. Press `F5` repeatedly in a large folder.
- Expected: no crash and status bar stays informative.

## Search Behavior
1. In `large-b`, search for `*.txt`.
- Expected: only matching files returned.
2. Search for plain text such as `report`.
- Expected: case-insensitive file-name matches returned.
3. Search while folder contains inaccessible descendants.
- Expected: search completes and skips inaccessible paths without crashing.

## File Operations
1. In a writable folder, create a new folder with `Ctrl+N`.
- Expected: folder appears and rename prompt opens immediately.
2. Rename selected item with `F2`.
- Expected: successful rename updates list and status bar.
3. Delete selected item with `Delete` and confirm.
- Expected: item removed and status bar reports success.
4. Trigger a conflict (rename to existing name / create duplicate folder).
- Expected: conflict prompt appears and allows retry/cancel.

## Permission Boundaries
1. Navigate directly to `restricted` path.
- Expected: app does not crash; list remains safe (empty or previous state) with error status.
2. Start a search from parent containing `restricted`.
- Expected: results exclude denied subtree; app remains responsive.
3. Attempt create/rename/delete on denied locations.
- Expected: operation fails with actionable status message, no unhandled exception.

## Pass Criteria
- No application crash across all steps.
- Status feedback shown for success and failure paths.
- Navigation, search, and file operations stay consistent after failures.

## Cleanup
```powershell
pwsh -File .\scripts\cleanup-regression-fixtures.ps1
```
