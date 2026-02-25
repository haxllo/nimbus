# Nimbus Finder Parity V2 Regression Checklist

Date: 2026-02-25

## 1. Build and Test Baseline
1. Run `dotnet build Nimbus.sln`.
2. Run `dotnet test Nimbus.sln`.
3. If WinUI build fails with `MSB3073` in `XamlCompiler.exe`:
   - Ensure no running Nimbus process is locking `bin/obj`.
   - Run `pwsh -File scripts/diagnose-xaml-compiler.ps1`.
   - Capture `xamlcompiler-direct.log`, `xamlcompiler-stdout.log`, `xamlcompiler-stderr.log`.

Pass criteria:
- Build succeeds with zero errors.
- Tests pass.

## 2. Smart Collections
1. Open app and navigate to sidebar `Smart Collections`.
2. Click each saved search.
3. Confirm the app opens the saved search root path and runs the saved query.
4. Confirm result list updates and status bar reports success/warning correctly.
5. Test a saved search with an unavailable root path and confirm status error appears.

Pass criteria:
- Saved search navigation + execution are deterministic.
- Invalid root path does not crash app.

## 3. Tabs
1. Press `Ctrl+T` to open a new tab.
2. Press `Ctrl+Tab` to cycle to the next tab.
3. Press `Ctrl+Shift+Tab` to cycle to the previous tab.
4. Press `Ctrl+W` to close the active tab.
5. Confirm closing last tab is blocked.
6. Click tab buttons in the tab strip and verify path/file list state restores per tab.

Pass criteria:
- Tab keyboard and mouse flows work.
- Tab switch restores navigation state and list contents.

## 4. Preview Pane and Quick Look
1. Select a text file and verify `CurrentPreview` fields populate in the right pane.
2. Select an image and verify preview metadata updates.
3. Press `Space` on selected item to open Quick Look dialog.
4. Confirm Quick Look closes cleanly and app remains responsive.
5. Confirm changing selection rapidly does not crash and stale preview content is not shown.

Pass criteria:
- Preview updates on selection.
- Quick Look works from keyboard without exceptions.

## 5. Finder-Style Navigation Regression
1. Validate breadcrumb navigation by clicking multiple breadcrumb segments.
2. Validate back/forward buttons and `Alt+Left` / `Alt+Right`.
3. Validate create folder, rename, delete with status feedback.
4. Validate `Ctrl+F` search focus behavior and Enter-to-search flow.
5. Change sort mode in toolbar and verify list updates for:
   - Name (A-Z / Z-A)
   - Date Modified (Oldest / Newest)
   - Size (Smallest / Largest)
6. Verify file list icon rendering:
   - folders show folder glyph
   - known file types show non-generic glyphs
   - image files render thumbnail fallback in icon/gallery/list templates
7. Drag sidebar and preview splitters, close app, relaunch app, verify pane widths are restored.

Pass criteria:
- Core file browsing and operations continue to work after parity features.

## 6. Exit Decision
- Mark V2 parity slice as accepted only when sections 1-5 pass.
- If any section fails, log failing step, path used, and exact status text/error message.
