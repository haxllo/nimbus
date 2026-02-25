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
$stdoutPath = Join-Path $repoRoot "xamlcompiler-stdout.log"
$stderrPath = Join-Path $repoRoot "xamlcompiler-stderr.log"
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

if (Test-Path $logPath)
{
    Remove-Item $logPath -Force
}

if (Test-Path $stdoutPath)
{
    Remove-Item $stdoutPath -Force
}

if (Test-Path $stderrPath)
{
    Remove-Item $stderrPath -Force
}

Push-Location $appProjectDir
try
{
    $process = Start-Process `
        -FilePath $compilerPath `
        -ArgumentList @($inputPath, $outputPath) `
        -Wait `
        -NoNewWindow `
        -PassThru `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath

    $exitCode = $process.ExitCode
}
finally
{
    Pop-Location
}

$stdout = if (Test-Path $stdoutPath) { Get-Content -Path $stdoutPath -Raw } else { "" }
$stderr = if (Test-Path $stderrPath) { Get-Content -Path $stderrPath -Raw } else { "" }

@(
    "==== STDOUT ===="
    $stdout
    "==== STDERR ===="
    $stderr
) | Set-Content -Path $logPath

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

$hasTextOutput =
    -not [string]::IsNullOrWhiteSpace($stdout) -or
    -not [string]::IsNullOrWhiteSpace($stderr)

if ($hasTextOutput)
{
    Get-Content -Path $logPath -Tail 200
}
else
{
    Write-Host ""
    Write-Host "No stdout/stderr from XamlCompiler.exe. Checking recent Application event log entries..."

    $startTime = (Get-Date).AddMinutes(-15)
    $events = Get-WinEvent `
        -FilterHashtable @{ LogName = "Application"; StartTime = $startTime } `
        -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.ProviderName -in @(".NET Runtime", "Application Error", "Windows Error Reporting")) -and
            $_.Message -match "XamlCompiler\\.exe|Microsoft\\.UI\\.Xaml\\.Markup\\.Compiler"
        } |
        Select-Object -First 15

    if ($events.Count -eq 0)
    {
        Write-Host "No related event log entries found in the last 15 minutes."
    }
    else
    {
        $events | Format-List TimeCreated, ProviderName, Id, LevelDisplayName, Message
    }
}

if ($exitCode -ne 0)
{
    exit $exitCode
}
