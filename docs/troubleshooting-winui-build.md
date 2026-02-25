# WinUI Build Troubleshooting

## Symptom
- `XamlCompiler.exe ... exited with code 1`
- `Microsoft.UI.Xaml.Markup.Compiler.interop.targets` failure during `dotnet build` or `dotnet run`.

## Recovery Steps
1. Run a full app clean/build:
   - `pwsh -File scripts/rebuild-app.ps1`
2. Run the app with the correct command:
   - `dotnet run --project src/Nimbus.App/Nimbus.App.csproj`

## Important Command Note
- Avoid `dotnet run build --project ...`.
- Use `dotnet build ...` and `dotnet run --project ...` as separate commands.

## If It Still Fails
Inspect XAML compiler log entries from `output.json`:

```powershell
$out = '.\src\Nimbus.App\obj\Debug\net8.0-windows10.0.19041.0\output.json'
(Get-Content $out -Raw | ConvertFrom-Json).MSBuildLogEntries |
  Where-Object { $_.ErrorCode -or ($_.Message -match 'error|exception') } |
  Format-List
```
