param(
    [string]$Configuration = "Debug",
    [string]$TargetFramework = "net8.0-windows10.0.19041.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$appProjectDir = Join-Path $repoRoot "src\Nimbus.App"
$objDir = Join-Path $appProjectDir "obj\$Configuration\$TargetFramework"
$inputPath = Join-Path $objDir "input.json"
$outputPath = Join-Path $objDir "output.json"
$logPath = Join-Path $repoRoot "xamlcompiler-direct.log"
$compilerPath = Join-Path $env:USERPROFILE ".nuget\packages\microsoft.windowsappsdk.winui\1.8.260204000\tools\net472\XamlCompiler.exe"

if (-not (Test-Path $appProjectDir))
{
    throw "Nimbus.App project directory not found: $appProjectDir"
}

if (-not (Test-Path $compilerPath))
{
    throw "XamlCompiler.exe not found at: $compilerPath"
}

if (-not (Test-Path $inputPath))
{
    Write-Host "input.json not found. Running build once to generate it..."
    Push-Location $appProjectDir
    try
    {
        dotnet build .\Nimbus.App.csproj
    }
    finally
    {
        Pop-Location
    }
}

if (-not (Test-Path $inputPath))
{
    throw "input.json still missing after build: $inputPath"
}

Set-Content -Path $logPath -Value ""

Push-Location $appProjectDir
try
{
    & $compilerPath $inputPath $outputPath *>> $logPath
    $exitCode = $LASTEXITCODE
}
finally
{
    Pop-Location
}

Write-Host "XamlCompiler exit code: $exitCode"
Write-Host "Log: $logPath"

if (Test-Path $outputPath)
{
    Write-Host "output.json generated: $outputPath"
}
else
{
    Write-Host "output.json was not generated."
}

Get-Content -Path $logPath -Tail 200

if ($exitCode -ne 0)
{
    exit $exitCode
}
