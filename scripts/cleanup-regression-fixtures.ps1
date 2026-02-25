param(
    [string]$RootPath = "$env:TEMP\nimbus-regression"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $RootPath))
{
    Write-Host "Nothing to clean at: $RootPath"
    exit 0
}

$restricted = Join-Path $RootPath "restricted"
if (Test-Path -LiteralPath $restricted)
{
    $identity = "$env:USERDOMAIN\$env:USERNAME"
    & icacls $restricted /remove:d "$identity" | Out-Null
    & icacls $restricted /inheritance:e | Out-Null
}

Remove-Item -LiteralPath $RootPath -Recurse -Force
Write-Host "Regression fixtures removed: $RootPath"
