param(
    [switch]$Run
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $repoRoot "src\Nimbus.App\Nimbus.App.csproj"
$appProjectDir = Split-Path -Parent $appProject

Write-Host "Nimbus: cleaning WinUI build artifacts..."
if (Test-Path (Join-Path $appProjectDir "bin"))
{
    Remove-Item -Path (Join-Path $appProjectDir "bin") -Recurse -Force
}

if (Test-Path (Join-Path $appProjectDir "obj"))
{
    Remove-Item -Path (Join-Path $appProjectDir "obj") -Recurse -Force
}

Write-Host "Nimbus: dotnet clean..."
dotnet clean $appProject

Write-Host "Nimbus: dotnet build..."
dotnet build $appProject

if ($Run)
{
    Write-Host "Nimbus: dotnet run..."
    dotnet run --project $appProject
}
