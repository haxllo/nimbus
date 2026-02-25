# Repository Guidelines

## Project Structure & Module Organization
- `src/Nimbus.App` contains the WinUI 3 desktop application (XAML views, app bootstrap, assets).
- `src/Nimbus.Core` contains testable domain logic (models, services, view models).
- `tests/Nimbus.Tests` contains xUnit tests targeting core logic.
- `docs/` contains planning and supporting documentation.
- Solution file: `Nimbus.sln` at the repo root.

## Build, Test, and Development Commands
- `dotnet build Nimbus.sln` builds the full solution.
- `dotnet test Nimbus.sln` runs all tests.
- `dotnet build src/Nimbus.App/Nimbus.App.csproj -c Release` builds a release WinUI app.

## Coding Style & Naming Conventions
- Language: C# with nullable reference types enabled.
- Indentation: 4 spaces in C#, 4 spaces in XAML.
- Naming: `PascalCase` for classes/public members, `camelCase` for locals/fields.
- Namespaces: `Nimbus.App` for UI, `Nimbus.Core` for shared logic, `Nimbus.Tests` for tests.

## Testing Guidelines
- Framework: xUnit.
- Naming: `*Tests.cs` with test classes named after the target type (e.g., `SearchServiceTests`).
- Prefer testing `Nimbus.Core` types; keep UI logic thin.
- Run tests with `dotnet test Nimbus.sln` before any PR.

## Commit & Pull Request Guidelines
- This repo does not contain git history yet, so no established commit convention exists.
- Recommended commit style: short, imperative subject (e.g., `feat: add breadcrumb navigation`).
- PRs should include: summary, testing notes, and screenshots for UI changes.

## Security & Configuration Tips
- Avoid hard-coding absolute paths; use `Environment.SpecialFolder` and `Path` helpers.
- Handle `UnauthorizedAccessException` in filesystem operations.
- Keep shell integration and privileged operations isolated in services.
