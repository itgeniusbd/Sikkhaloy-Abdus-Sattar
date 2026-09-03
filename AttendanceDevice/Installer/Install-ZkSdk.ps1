# Installs a new ZKTeco Standalone SDK (32-bit) for AttendanceDevice.
# Run as Administrator after copying vendor DLLs from ZKTeco dealer.
#
# Example:
#   powershell -ExecutionPolicy Bypass -File ".\Install-ZkSdk.ps1" -SourcePath "D:\ZKTeco\SenseFace3A-SDK\32bit"
#
param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,

    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"

function Get-ZkVersion([string]$DllPath) {
    if (-not (Test-Path $DllPath)) { return $null }
    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($DllPath)
    return [PSCustomObject]@{
        Path            = $DllPath
        FileVersion     = $info.FileVersion
        ProductVersion  = $info.ProductVersion
        Size            = (Get-Item $DllPath).Length
    }
}

function Require-Admin {
    $current = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    if (-not $current.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script as Administrator (right-click PowerShell -> Run as administrator)."
    }
}

Require-Admin

$sourcePath = (Resolve-Path $SourcePath).Path
$targetDir = Join-Path $ProjectRoot "libs\Zktec 32bit"
$releaseDir = Join-Path $ProjectRoot "bin\Release\libs\Zktec 32bit"
$regApp = Join-Path $ProjectRoot "bin\Release\ZKdllRegistrationApp.exe"

if (-not (Test-Path (Join-Path $sourcePath "zkemkeeper.dll"))) {
    throw "Source folder must contain zkemkeeper.dll. Got: $sourcePath"
}

if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

$oldDll = Join-Path $targetDir "zkemkeeper.dll"
Write-Host "Current SDK:" -ForegroundColor Cyan
Get-ZkVersion $oldDll | Format-List

$backupDir = Join-Path $ProjectRoot ("libs\Zktec 32bit_backup_" + (Get-Date -Format "yyyyMMdd_HHmmss"))
Write-Host "Backing up existing SDK to:" -ForegroundColor Yellow
Write-Host "  $backupDir"
Copy-Item -Path $targetDir -Destination $backupDir -Recurse -Force

$sourceDlls = Get-ChildItem -Path $sourcePath -Filter "*.dll"
Write-Host "Copying $($sourceDlls.Count) DLL(s) from vendor package..." -ForegroundColor Cyan
foreach ($dll in $sourceDlls) {
    Copy-Item -Path $dll.FullName -Destination (Join-Path $targetDir $dll.Name) -Force
}

if (Test-Path $releaseDir) {
    foreach ($dll in $sourceDlls) {
        Copy-Item -Path $dll.FullName -Destination (Join-Path $releaseDir $dll.Name) -Force
    }
}

Write-Host "New SDK in project:" -ForegroundColor Green
Get-ZkVersion (Join-Path $targetDir "zkemkeeper.dll") | Format-List

if (-not (Test-Path $regApp)) {
    Write-Host "ZKdllRegistrationApp.exe not found. Building Release first..." -ForegroundColor Yellow
    $msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    if (-not $msbuild) { throw "MSBuild not found. Build AttendanceDevice Release first." }
    & $msbuild (Join-Path $ProjectRoot "AttendanceDevice.csproj") /p:Configuration=Release /p:Platform=AnyCPU /v:minimal | Out-Null
    $regApp = Join-Path $ProjectRoot "bin\Release\ZKdllRegistrationApp.exe"
}

if (-not (Test-Path $regApp)) {
    throw "Registration helper missing: $regApp"
}

Write-Host "Registering zkemkeeper.dll to Windows (SysWOW64)..." -ForegroundColor Cyan
& $regApp $targetDir
if ($LASTEXITCODE -ne 0) {
    throw "SDK registration failed. Exit code: $LASTEXITCODE"
}

$sysDll = Join-Path ${env:Windir} "SysWOW64\zkemkeeper.dll"
Write-Host "Registered SDK:" -ForegroundColor Green
Get-ZkVersion $sysDll | Format-List

Write-Host ""
Write-Host "Done. Next steps:" -ForegroundColor Green
Write-Host "  1. Close Attendance Device completely."
Write-Host "  2. Device: Standalone Communication ON, HTTPS OFF, Comm Key 342015, Port 4370."
Write-Host "  3. Run AttendanceDevice and try Add Device again."
Write-Host "  4. If Interop errors appear after major SDK upgrade, rebuild AttendanceDevice in Visual Studio."
