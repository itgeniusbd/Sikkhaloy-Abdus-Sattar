# ZKTeco Manager - Build and Run Script
# This script builds the solution and runs the application

Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?        ZKTeco Device Manager - Build & Run Script         ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$scriptPath = $PSScriptRoot
$solutionFile = Join-Path $scriptPath "ZKTeco_Manager.sln"
$outputExe = Join-Path $scriptPath "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe"

# Step 1: Check if solution exists
Write-Host "Step 1: Checking solution file..." -ForegroundColor Yellow
if (Test-Path $solutionFile) {
    Write-Host "  ? Solution found: ZKTeco_Manager.sln" -ForegroundColor Green
} else {
    Write-Host "  ? Solution file not found!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 2: Find MSBuild
Write-Host ""
Write-Host "Step 2: Locating MSBuild..." -ForegroundColor Yellow

$msbuildPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $null
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        $msbuild = $path
        break
    }
}

if ($msbuild) {
    Write-Host "  ? MSBuild found: $msbuild" -ForegroundColor Green
} else {
    Write-Host "  ? MSBuild not found!" -ForegroundColor Red
    Write-Host "  Please install Visual Studio 2019 or 2022" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 3: Restore NuGet packages
Write-Host ""
Write-Host "Step 3: Restoring NuGet packages..." -ForegroundColor Yellow

$nugetExe = Join-Path $env:TEMP "nuget.exe"
if (-not (Test-Path $nugetExe)) {
    Write-Host "  Downloading NuGet.exe..." -ForegroundColor Gray
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetExe
}

& $nugetExe restore $solutionFile
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ? NuGet packages restored" -ForegroundColor Green
} else {
    Write-Host "  ??  NuGet restore had warnings (continuing...)" -ForegroundColor Yellow
}

# Step 4: Build solution
Write-Host ""
Write-Host "Step 4: Building solution..." -ForegroundColor Yellow

& $msbuild $solutionFile /p:Configuration=Release /p:Platform="Any CPU" /v:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ? Build successful!" -ForegroundColor Green
} else {
    Write-Host "  ? Build failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 5: Check output
Write-Host ""
Write-Host "Step 5: Verifying output..." -ForegroundColor Yellow

if (Test-Path $outputExe) {
    $fileInfo = Get-Item $outputExe
    Write-Host "  ? Executable created: $($fileInfo.Name)" -ForegroundColor Green
    Write-Host "  ?? Location: $($fileInfo.DirectoryName)" -ForegroundColor Cyan
    Write-Host "  ?? Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
    Write-Host "  ?? Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Cyan
} else {
    Write-Host "  ? Executable not found!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Summary
Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                    Build Summary                           ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "? Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Application: ZKTeco Device Manager" -ForegroundColor White
Write-Host "Executable: $outputExe" -ForegroundColor White
Write-Host ""

# Ask to run
$response = Read-Host "Do you want to run the application now? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Write-Host ""
    Write-Host "?? Launching ZKTeco Device Manager..." -ForegroundColor Green
    Start-Process $outputExe
} else {
    Write-Host ""
    Write-Host "You can run the application later from:" -ForegroundColor Yellow
    Write-Host $outputExe -ForegroundColor White
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "Thank you for using ZKTeco Device Manager!" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
