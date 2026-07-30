# Sikkhaloy SmsSenderApp - one-click rebuild + install + start
param(
    [string]$Configuration = "Release",
    [string]$InstallPath = "C:\Sikkhaloy\SmsSenderApp",
    [switch]$KeepConfig,
    [switch]$NoStart
)

$ErrorActionPreference = "Stop"

$installerDir = $PSScriptRoot
$projectRoot = Split-Path -Parent $installerDir
$projectFile = Join-Path $projectRoot "SmsSenderApp.csproj"
$repoRoot = Split-Path -Parent $projectRoot
$releaseDir = Join-Path $projectRoot "bin\$Configuration"
$exeName = "SmsSenderApp.exe"
$processName = "SmsSenderApp"

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host $Message -ForegroundColor Cyan
}

function Find-MsBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($path) { return $path }
    }

    $fallbacks = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($candidate in $fallbacks) {
        if (Test-Path $candidate) { return $candidate }
    }

    return $null
}

function Stop-SmsSenderProcess {
    $running = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if (-not $running) {
        Write-Host "  SmsSenderApp is not running." -ForegroundColor Gray
        return
    }

    foreach ($proc in $running) {
        Write-Host "  Stopping SmsSenderApp (PID $($proc.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }

    Start-Sleep -Seconds 3

    $stillRunning = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($stillRunning) {
        throw "SmsSenderApp is still running. Close it from the system tray (Exit), then run this script again."
    }
}

function Ensure-NuGetPackages {
    $packagesDir = Join-Path $repoRoot "packages"
    $efProps = Join-Path $packagesDir "EntityFramework.6.4.4\build\EntityFramework.props"
    if (Test-Path $efProps) {
        Write-Host "  NuGet packages already present." -ForegroundColor Gray
        return
    }

    Write-Host "  Restoring NuGet packages..." -ForegroundColor Yellow
    $nugetExe = Join-Path $env:TEMP "nuget.exe"
    if (-not (Test-Path $nugetExe)) {
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetExe
    }

    $solutionFile = Join-Path $repoRoot "SIKKHALOY.sln"
    if (Test-Path $solutionFile) {
        & $nugetExe restore $solutionFile | Out-Host
    }
    else {
        & $nugetExe restore $projectFile | Out-Host
    }
}

function Install-ReleaseOutput {
    param(
        [string]$SourceDir,
        [string]$TargetDir
    )

    if (-not (Test-Path $TargetDir)) {
        New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
    }

    $existingConfig = Join-Path $TargetDir "SmsSenderApp.exe.config"
    $backupConfig = Join-Path $env:TEMP "SmsSenderApp.exe.config.bak"
    if ($KeepConfig -and (Test-Path $existingConfig)) {
        Copy-Item $existingConfig $backupConfig -Force
        Write-Host "  Existing config backed up (KeepConfig)." -ForegroundColor Yellow
    }

    Write-Host "  Copying files to $TargetDir ..." -ForegroundColor Yellow
    robocopy $SourceDir $TargetDir /MIR /XF *.pdb *.vshost.* /NFL /NDL /NJH /NJS /NC /NS | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "File copy failed (robocopy exit code $LASTEXITCODE)."
    }

    if ($KeepConfig -and (Test-Path $backupConfig)) {
        Copy-Item $backupConfig $existingConfig -Force
        Remove-Item $backupConfig -Force -ErrorAction SilentlyContinue
    }

    $logDir = Join-Path $TargetDir "Log"
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
}

function Update-StartupShortcut([string]$TargetExe) {
    $startupFolder = [Environment]::GetFolderPath("Startup")
    $shortcutPath = Join-Path $startupFolder "SikkhaloySmsSender.lnk"
    $targetDir = Split-Path -Parent $TargetExe
    $iconPath = Join-Path $targetDir "Resources\Sikkhaloy.ico"

    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $TargetExe
    $shortcut.WorkingDirectory = $targetDir
    $shortcut.Description = "Sikkhaloy SMS Sender - Auto Start"
    if (Test-Path $iconPath) {
        $shortcut.IconLocation = $iconPath
    }
    else {
        $shortcut.IconLocation = "$TargetExe,0"
    }
    $shortcut.Save()

    Write-Host "  Startup shortcut updated: $shortcutPath" -ForegroundColor Green
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Sikkhaloy SmsSenderApp - Build and Install" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

Write-Step "Step 1/5: Locate MSBuild"
$msbuild = Find-MsBuild
if (-not $msbuild) {
    throw "MSBuild not found. Install Visual Studio 2019/2022 with .NET desktop development workload."
}
Write-Host "  $msbuild" -ForegroundColor Green

Write-Step "Step 2/6: Stop running SmsSenderApp (required before build)"
Stop-SmsSenderProcess

Write-Step "Step 3/6: Restore packages"
Ensure-NuGetPackages

Write-Step "Step 4/6: Build $Configuration"
& $msbuild $projectFile /t:Rebuild /p:Configuration=$Configuration /p:Platform=AnyCPU /v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

$builtExe = Join-Path $releaseDir $exeName
if (-not (Test-Path $builtExe)) {
    throw "Build output not found: $builtExe"
}

$buildInfo = Get-Item $builtExe
Write-Host "  Built: $($buildInfo.FullName)" -ForegroundColor Green
Write-Host "  Time : $($buildInfo.LastWriteTime)" -ForegroundColor Green

Write-Step "Step 5/6: Install to $InstallPath"
Stop-SmsSenderProcess
Install-ReleaseOutput -SourceDir $releaseDir -TargetDir $InstallPath

$installedExe = Join-Path $InstallPath $exeName
if (-not (Test-Path $installedExe)) {
    throw "Install failed: $installedExe not found."
}

Update-StartupShortcut -TargetExe $installedExe

Write-Step "Step 6/6: Start application"
if ($NoStart) {
    Write-Host "  Skipped start (-NoStart)." -ForegroundColor Yellow
}
else {
    Start-Process -FilePath $installedExe -WorkingDirectory $InstallPath
    Write-Host "  SmsSenderApp started." -ForegroundColor Green
    Write-Host "  Check tray icon + Log\log.txt for timer activity." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Install folder: $InstallPath" -ForegroundColor White
Write-Host ""
Write-Host "Next (after Attendance_API requeue):" -ForegroundColor Yellow
Write-Host "  SELECT COUNT(*) FROM Attendance_SMS WHERE SchoolID = 1012;" -ForegroundColor Gray
