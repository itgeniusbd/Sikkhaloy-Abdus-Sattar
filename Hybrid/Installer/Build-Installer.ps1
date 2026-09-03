# Publishes SIKKHALOY Hybrid (self-contained win-x64) and compiles the Inno Setup installer.
param(
    [string]$Configuration = "Release",
    [string]$InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    [switch]$BumpVersion
)

$ErrorActionPreference = "Stop"
$installerDir = $PSScriptRoot
$hybridRoot = Split-Path -Parent $installerDir
$projectFile = Join-Path $hybridRoot "src\Sikkhaloy.Client\Sikkhaloy.Client.csproj"
$versionFile = Join-Path $installerDir "app.version"
$publishDir = Join-Path $hybridRoot "dist\SikkhaloyHybrid"
$issFile = Join-Path $installerDir "SikkhaloyHybrid.iss"

function Get-AppVersion {
    if (-not (Test-Path $versionFile)) { return "1.0.0" }
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

Get-Process -Name "SikkhaloyHybrid" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Stopping running Hybrid (PID $($_.Id))..." -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 1

Write-Host "Publishing SIKKHALOY Hybrid $appVersion (self-contained win-x64)..." -ForegroundColor Cyan
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

dotnet publish $projectFile `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:Version=$appVersion `
    -p:FileVersion="$appVersion.0" `
    -p:InformationalVersion=$appVersion `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

$exe = Join-Path $publishDir "SikkhaloyHybrid.exe"
if (-not (Test-Path $exe)) {
    throw "Publish failed: SikkhaloyHybrid.exe not found in $publishDir"
}

Get-ChildItem -Path $publishDir -Filter "*.pdb" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
$junk = Join-Path $publishDir "_buildcheck"
if (Test-Path $junk) {
    Remove-Item -Recurse -Force $junk
}

if (-not (Test-Path $InnoSetupPath)) {
    Write-Host ""
    Write-Host "Inno Setup not found at:" -ForegroundColor Yellow
    Write-Host "  $InnoSetupPath"
    Write-Host ""
    Write-Host "Published files are ready at:" -ForegroundColor Green
    Write-Host "  $publishDir"
    Write-Host ""
    Write-Host "Install Inno Setup 6, then compile:" -ForegroundColor Yellow
    Write-Host "  $issFile"
    exit 0
}

$outDir = Join-Path $installerDir "Output"
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

Write-Host "Compiling installer..." -ForegroundColor Cyan
& $InnoSetupPath "/DMyAppVersion=$appVersion" $issFile
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compile failed."
}

$setup = Join-Path $outDir "SIKKHALOY_Hybrid_Setup_$appVersion.exe"
if (-not (Test-Path $setup)) {
    throw "Installer was not created: $setup"
}

Write-Host ""
Write-Host "Installer ready:" -ForegroundColor Green
Write-Host "  $setup"
Write-Host ""
Write-Host "Install this Setup.exe. It puts the app in Program Files, creates a Desktop shortcut,"
Write-Host "and can be removed from Windows Settings > Apps (Add/Remove Programs)."
