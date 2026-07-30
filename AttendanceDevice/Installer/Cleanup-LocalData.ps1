# Removes all SIKKHALOY Attendance Device local data (handles Windows long-path / WebView2 cache).
param(
    [switch]$Quiet
)

$ErrorActionPreference = 'SilentlyContinue'

function Write-Info([string]$Message) {
    if (-not $Quiet) { Write-Host $Message }
}

function Stop-AttendanceProcesses {
    Get-Process -Name 'AttendanceDevice' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
    & taskkill.exe /F /IM AttendanceDevice.exe /T 2>$null | Out-Null
}

function Remove-TreeForce([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { return }
    if (-not (Test-Path -LiteralPath $Path)) { return }

    $fullPath = (Resolve-Path -LiteralPath $Path).Path
    Write-Info "Removing: $fullPath"

    $emptyDir = Join-Path $env:TEMP ("sikkhaloy_empty_{0}" -f [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $emptyDir -Force | Out-Null
    & robocopy.exe $emptyDir $fullPath /MIR /R:0 /W:0 /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null
    Remove-Item -LiteralPath $emptyDir -Force -Recurse -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fullPath -Force -Recurse -ErrorAction SilentlyContinue

    if (Test-Path -LiteralPath $fullPath) {
        cmd.exe /c "rmdir /s /q `"$fullPath`"" | Out-Null
    }
}

function Get-ClickOnceAttendanceRoots {
    $roots = New-Object System.Collections.Generic.HashSet[string]
    $appsRoot = Join-Path $env:LOCALAPPDATA 'Apps\2.0'
    if (-not (Test-Path -LiteralPath $appsRoot)) { return @() }

    $needles = @('AttendanceDevice.exe', 'AttendanceDevice.application', 'SikkhaloyAppDB.db')
    foreach ($needle in $needles) {
        Get-ChildItem -LiteralPath $appsRoot -Filter $needle -File -Recurse -Depth 12 -ErrorAction SilentlyContinue |
            ForEach-Object {
                $dir = $_.Directory.FullName
                while ($dir -and ($dir.Length -gt $appsRoot.Length)) {
                    if ($dir -match 'AttendanceDevice|SikkhaloyAppDB|attendanc') {
                        [void]$roots.Add($dir)
                    }
                    $parent = Split-Path -Parent $dir
                    if ($parent -eq $dir) { break }
                    $dir = $parent
                }
            }
    }

    return $roots | Sort-Object { $_.Length } -Descending
}

Stop-AttendanceProcesses

$targets = @(
    (Join-Path $env:LOCALAPPDATA 'SIKKHALOY\AttendanceDevice'),
    (Join-Path $env:LOCALAPPDATA 'SIKKHALOY'),
    (Join-Path $env:LOCALAPPDATA 'SikkhaloyAttendance'),
    (Join-Path $env:LOCALAPPDATA 'Deployment'),
    (Join-Path $env:LOCALAPPDATA 'IT Genius\SIKKHALOY Attendance Device')
)

$programFilesX86 = ${env:ProgramFiles(x86)}
if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
    $targets += (Join-Path $programFilesX86 'SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db')
    $targets += (Join-Path $programFilesX86 'SIKKHALOY\AttendanceDevice\Database\SikkhaloyAppDB.db')
}

$programFiles = $env:ProgramFiles
if (-not [string]::IsNullOrWhiteSpace($programFiles)) {
    $targets += (Join-Path $programFiles 'SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db')
}

$targets += Get-ClickOnceAttendanceRoots

foreach ($target in ($targets | Select-Object -Unique)) {
    if ($target -match '\.db$') {
        if (Test-Path -LiteralPath $target) {
            Write-Info "Removing file: $target"
            Remove-Item -LiteralPath $target -Force -ErrorAction SilentlyContinue
        }
        continue
    }

    Remove-TreeForce -Path $target
}

Write-Info 'Local data cleanup finished.'
