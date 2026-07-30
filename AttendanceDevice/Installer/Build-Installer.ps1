# Builds Release output and compiles the Inno Setup installer (if ISCC is installed).
param(
    [string]$Configuration = "Release",
    [string]$InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    [switch]$BumpVersion
)

$ErrorActionPreference = "Stop"
$installerDir = $PSScriptRoot
$projectRoot = Split-Path -Parent $installerDir
$projectFile = Join-Path $projectRoot "AttendanceDevice.csproj"
$versionFile = Join-Path $installerDir "app.version"
$assemblyInfo = Join-Path $projectRoot "Properties\AssemblyInfo.cs"

function Get-AppVersion {
    if (-not (Test-Path $versionFile)) { return "4.0.0" }
    return (Get-Content $versionFile -Raw).Trim()
}

function Set-AppVersion([string]$Version) {
    Set-Content -Path $versionFile -Value $Version -NoNewline
}

function Bump-AppVersion([string]$Version) {
    $parts = $Version.Split('.')
    while ($parts.Length -lt 3) { $parts += "0" }
    $parts[2] = ([int]$parts[2] + 1).ToString()
    return ($parts[0..2] -join '.')
}

$appVersion = Get-AppVersion
if ($BumpVersion) {
    $appVersion = Bump-AppVersion $appVersion
    Set-AppVersion $appVersion
    Write-Host "Version bumped to $appVersion" -ForegroundColor Yellow
}

$assemblyVersion = "$appVersion.0"
if ($assemblyInfo) {
    $ai = Get-Content $assemblyInfo -Raw
    $ai = [regex]::Replace($ai, '\[assembly: AssemblyVersion\("[^"]+"\)\]', "[assembly: AssemblyVersion(`"$assemblyVersion`")]")
    $ai = [regex]::Replace($ai, '\[assembly: AssemblyFileVersion\("[^"]+"\)\]', "[assembly: AssemblyFileVersion(`"$assemblyVersion`")]")
    Set-Content -Path $assemblyInfo -Value $ai -NoNewline
}

$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild not found. Build from Visual Studio first, then compile the .iss file manually."
}

$zkSdkSource = Join-Path $projectRoot "libs\Zktec 32bit\zkemkeeper.dll"
if (-not (Test-Path $zkSdkSource)) {
    throw "ZKTeco SDK missing. Copy vendor DLLs into AttendanceDevice\libs\Zktec 32bit\ before building the installer."
}

Write-Host "Building AttendanceDevice $appVersion ($Configuration)..." -ForegroundColor Cyan
& $msbuild $projectFile /p:Configuration=$Configuration /p:Platform=AnyCPU /t:Rebuild /v:minimal

$releaseDir = Join-Path $projectRoot "bin\$Configuration"
if (-not (Test-Path (Join-Path $releaseDir "AttendanceDevice.exe"))) {
    throw "Build failed: AttendanceDevice.exe not found in $releaseDir"
}

$zkSdkOutput = Join-Path $releaseDir "libs\Zktec 32bit\zkemkeeper.dll"
if (-not (Test-Path $zkSdkOutput)) {
    throw "Build failed: ZKTeco SDK was not copied to $releaseDir\libs\Zktec 32bit"
}

if (-not (Test-Path $InnoSetupPath)) {
    Write-Host ""
    Write-Host "Inno Setup not found at:" -ForegroundColor Yellow
    Write-Host "  $InnoSetupPath"
    Write-Host ""
    Write-Host "Release build is ready at:" -ForegroundColor Green
    Write-Host "  $releaseDir"
    Write-Host ""
    Write-Host "Install Inno Setup 6, then compile:" -ForegroundColor Yellow
    Write-Host "  AttendanceDevice\Installer\SikkhaloyAttendanceDevice.iss"
    exit 0
}

$issFile = Join-Path $installerDir "SikkhaloyAttendanceDevice.iss"
Write-Host "Compiling installer v$appVersion ..." -ForegroundColor Cyan
& $InnoSetupPath "/DMyAppVersion=$appVersion" $issFile

$outputDir = Join-Path $installerDir "Output"
Write-Host ""
Write-Host "Installer ready:" -ForegroundColor Green
Get-ChildItem $outputDir -Filter "*Setup*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 3 | ForEach-Object { Write-Host "  $($_.FullName)" }
