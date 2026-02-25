# Nimbus Roadmap

## Scope and Principles
- Deliver a fast, Windows-native file explorer inspired by Finder behavior without copying branding.
- Keep core logic in `src/Nimbus.Core` and UI in `src/Nimbus.App`.
- Prefer shell-backed operations and handle permission errors gracefully.

## V1 Milestones
1. Navigation and layout
- Sidebar favorites, breadcrumb path bar, back/forward history.
- List view with basic metadata columns.

2. File operations
- Copy, move, rename, delete with safe error handling.
- Drag-and-drop between folders and within the list.

3. Search
- Basic filesystem search with cancellation.
- Optional Windows Search index integration for speed.

4. UI polish
- Toolbar actions and keyboard shortcuts.
- App branding and icons in `Package.appxmanifest` and `Assets`.

## V1 Acceptance Criteria
- Navigate `C:\` and user profile without crashes.
- Back/forward works across multiple folders.
- Sidebar opens Documents, Downloads, Pictures.
- File operations succeed in a temp folder.
- Search returns expected results.

## V2 Roadmap
1. Preview and metadata
- Preview pane for images, text, and common document types.
- Extended metadata columns and custom sort.

2. Finder-like views
- Column view and optional tabs.
- Quick Look style viewer.

3. Shell integration
- Context menus, file associations, and properties dialog.

## Risks and Dependencies
- Windows Shell API integration complexity and permissions.
- UI virtualization performance with large folders.
- Windows Search availability and indexing state.
