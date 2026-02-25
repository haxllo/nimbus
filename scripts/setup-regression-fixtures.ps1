param(
    [string]$RootPath = "$env:TEMP\nimbus-regression",
    [int]$LargeAFolderCount = 200,
    [int]$LargeAFileCountPerFolder = 10,
    [int]$LargeBBranchCount = 50,
    [int]$LargeBDepth = 5,
    [int]$LargeBFilesPerNode = 20,
    [switch]$CreateRestrictedFolder
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (Test-Path -LiteralPath $RootPath)
{
    Remove-Item -LiteralPath $RootPath -Recurse -Force
}

$null = New-Item -ItemType Directory -Path $RootPath -Force

$largeA = Join-Path $RootPath "large-a"
$largeB = Join-Path $RootPath "large-b"
$restricted = Join-Path $RootPath "restricted"

$null = New-Item -ItemType Directory -Path $largeA -Force
$null = New-Item -ItemType Directory -Path $largeB -Force

for ($i = 0; $i -lt $LargeAFolderCount; $i++)
{
    $folder = Join-Path $largeA ("folder-{0:D3}" -f $i)
    $null = New-Item -ItemType Directory -Path $folder -Force

    for ($j = 0; $j -lt $LargeAFileCountPerFolder; $j++)
    {
        $extension = switch ($j % 3)
        {
            0 { ".txt" }
            1 { ".json" }
            default { ".png" }
        }

        $filePath = Join-Path $folder ("report-{0:D3}-{1:D3}{2}" -f $i, $j, $extension)
        Set-Content -Path $filePath -Value "nimbus" -NoNewline
    }
}

for ($branch = 0; $branch -lt $LargeBBranchCount; $branch++)
{
    $branchRoot = Join-Path $largeB ("branch-{0:D3}" -f $branch)
    $null = New-Item -ItemType Directory -Path $branchRoot -Force
    $cursor = $branchRoot

    for ($depth = 1; $depth -le $LargeBDepth; $depth++)
    {
        $cursor = Join-Path $cursor ("level-{0:D2}" -f $depth)
        $null = New-Item -ItemType Directory -Path $cursor -Force

        for ($k = 0; $k -lt $LargeBFilesPerNode; $k++)
        {
            $name = if ($k -eq 0)
            {
                "target-{0:D3}-{1:D2}-report.txt" -f $branch, $depth
            }
            else
            {
                "noise-{0:D3}-{1:D2}-{2:D3}.log" -f $branch, $depth, $k
            }

            Set-Content -Path (Join-Path $cursor $name) -Value "nimbus" -NoNewline
        }
    }
}

if ($CreateRestrictedFolder)
{
    $null = New-Item -ItemType Directory -Path $restricted -Force
    Set-Content -Path (Join-Path $restricted "restricted.txt") -Value "restricted" -NoNewline

    $identity = "$env:USERDOMAIN\$env:USERNAME"
    & icacls $restricted /inheritance:r | Out-Null
    & icacls $restricted /grant:r "$identity:(OI)(CI)M" | Out-Null
    & icacls $restricted /deny "$identity:(OI)(CI)RX" | Out-Null
}

Write-Host "Regression fixtures created at: $RootPath"
Write-Host "large-a folders: $LargeAFolderCount (files each: $LargeAFileCountPerFolder)"
Write-Host "large-b branches: $LargeBBranchCount (depth: $LargeBDepth, files each level: $LargeBFilesPerNode)"
if ($CreateRestrictedFolder)
{
    Write-Host "restricted folder created with read deny ACL for current user."
}
