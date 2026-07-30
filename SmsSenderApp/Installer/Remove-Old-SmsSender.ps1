# Remove old Sikkhaloy SmsSenderApp from this server (stop app, shortcuts, install folder).
param(
    [string[]]$InstallPaths = @(
        "C:\Sikkhaloy\SmsSenderApp",
        "C:\Program Files\Sikkhaloy\SmsSenderApp",
        "C:\Program Files (x86)\Sikkhaloy\SmsSenderApp",
        "D:\Sikkhaloy\SmsSenderApp"
    ),
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$processName = "SmsSenderApp"

function Confirm-Action([string]$Message) {
    if ($Force) { return $true }
    $answer = Read-Host "$Message (Y/N)"
    return ($answer -eq "Y" -or $answer -eq "y")
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Remove old Sikkhaloy SmsSenderApp" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "Step 1/4: Stop running app" -ForegroundColor Cyan
$running = Get-Process -Name $processName -ErrorAction SilentlyContinue
if ($running) {
    if (Confirm-Action "Stop SmsSenderApp now?") {
        $running | Stop-Process -Force
        Start-Sleep -Seconds 2
        Write-Host "  Process stopped." -ForegroundColor Green
    }
    else {
        throw "Cancelled. Stop the app manually first."
    }
}
else {
    Write-Host "  No running SmsSenderApp process found." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Step 2/4: Remove startup/desktop shortcuts" -ForegroundColor Cyan
$shortcutNames = @(
    "SikkhaloySmsSender.lnk",
    "Sikkhaloy SMS Sender.lnk"
)

$shortcutFolders = @(
    [Environment]::GetFolderPath("Startup"),
    [Environment]::GetFolderPath("CommonStartup"),
    [Environment]::GetFolderPath("Desktop"),
    [Environment]::GetFolderPath("CommonDesktopDirectory")
)

foreach ($folder in $shortcutFolders) {
    foreach ($name in $shortcutNames) {
        $path = Join-Path $folder $name
        if (Test-Path $path) {
            Remove-Item $path -Force
            Write-Host "  Removed: $path" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "Step 3/4: Find old install folders" -ForegroundColor Cyan
$foundPaths = New-Object System.Collections.Generic.List[string]

foreach ($path in $InstallPaths) {
    if (Test-Path $path) {
        $foundPaths.Add($path) | Out-Null
        Write-Host "  Found: $path" -ForegroundColor Yellow
    }
}

# Also detect from startup shortcut target if still present in registry-less scan
$extraCandidates = @(
    "C:\SmsSenderApp",
    "C:\Apps\SmsSenderApp"
)
foreach ($path in $extraCandidates) {
    if ((Test-Path $path) -and -not $foundPaths.Contains($path)) {
        $exe = Join-Path $path "SmsSenderApp.exe"
        if (Test-Path $exe) {
            $foundPaths.Add($path) | Out-Null
            Write-Host "  Found: $path" -ForegroundColor Yellow
        }
    }
}

if ($foundPaths.Count -eq 0) {
    Write-Host "  No known install folder found." -ForegroundColor Gray
    Write-Host "  If old app was ClickOnce, uninstall from Settings > Apps." -ForegroundColor Gray
}
else {
    Write-Host ""
    Write-Host "Step 4/4: Delete install folder(s)" -ForegroundColor Cyan
    foreach ($path in $foundPaths) {
        if (Confirm-Action "Delete folder '$path' ?") {
            Remove-Item $path -Recurse -Force
            Write-Host "  Deleted: $path" -ForegroundColor Green
        }
        else {
            Write-Host "  Skipped: $path" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Optional: ClickOnce old install" -ForegroundColor Cyan
Write-Host "  Settings > Apps > search 'SmsSenderApp' > Uninstall" -ForegroundColor Gray
Write-Host "  Or run: appwiz.cpl and remove Sikkhaloy SMS Sender" -ForegroundColor Gray

Write-Host ""
Write-Host "Done. Now install the new build:" -ForegroundColor Green
Write-Host "  SmsSenderApp\Installer\Build-And-Install.cmd" -ForegroundColor White
Write-Host ""
