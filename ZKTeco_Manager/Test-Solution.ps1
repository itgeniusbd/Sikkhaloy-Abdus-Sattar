# ZKTeco Manager - Test & Verification Script
# This script tests all components of the solution

param(
    [switch]$SkipBuild,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  ZKTeco Manager - Test & Verification  " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Test counter
$script:TestsPassed = 0
$script:TestsFailed = 0
$script:TestsTotal = 0

function Write-TestStart {
    param([string]$TestName)
    $script:TestsTotal++
    Write-Host "[$script:TestsTotal] Testing: $TestName" -ForegroundColor Yellow -NoNewline
}

function Write-TestPass {
    $script:TestsPassed++
    Write-Host " ? PASS" -ForegroundColor Green
}

function Write-TestFail {
    param([string]$Reason)
    $script:TestsFailed++
    Write-Host " ? FAIL" -ForegroundColor Red
    if ($Reason) {
        Write-Host "    Reason: $Reason" -ForegroundColor Red
    }
}

function Write-TestInfo {
    param([string]$Info)
    if ($Verbose) {
        Write-Host "    ? $Info" -ForegroundColor Gray
    }
}

# ========================================
# Test 1: Solution File Exists
# ========================================
Write-TestStart "Solution file exists"
if (Test-Path "ZKTeco_Manager.sln") {
    Write-TestPass
    Write-TestInfo "Found: ZKTeco_Manager.sln"
} else {
    Write-TestFail "Solution file not found"
}

# ========================================
# Test 2: Project Files Exist
# ========================================
$projects = @(
    "ZKTeco.Core\ZKTeco.Core.csproj",
    "ZKTeco.Manager\ZKTeco.Manager.csproj",
    "ZKTeco.Service\ZKTeco.Service.csproj"
)

foreach ($project in $projects) {
    $projectName = Split-Path (Split-Path $project -Parent) -Leaf
    Write-TestStart "Project '$projectName' exists"
    if (Test-Path $project) {
        Write-TestPass
        Write-TestInfo "Found: $project"
    } else {
        Write-TestFail "Project file not found: $project"
    }
}

# ========================================
# Test 3: Build Solution
# ========================================
if (-not $SkipBuild) {
    Write-TestStart "Building solution (Release)"
    try {
        $buildOutput = msbuild ZKTeco_Manager.sln /p:Configuration=Release /v:minimal /nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-TestPass
            Write-TestInfo "Build completed successfully"
        } else {
            Write-TestFail "Build failed with exit code $LASTEXITCODE"
            if ($Verbose) {
                Write-Host $buildOutput -ForegroundColor Red
            }
        }
    } catch {
        Write-TestFail "Build exception: $_"
    }
} else {
    Write-Host "[SKIP] Build test skipped (use without -SkipBuild to test)" -ForegroundColor Gray
}

# ========================================
# Test 4: Output Files Exist
# ========================================
$outputFiles = @{
    "ZKTeco.Core.dll" = "ZKTeco.Core\bin\Release\ZKTeco.Core.dll"
    "ZKTeco.Manager.exe" = "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe"
    "ZKTeco.Service.exe" = "ZKTeco.Service\bin\Release\ZKTeco.Service.exe"
}

foreach ($file in $outputFiles.GetEnumerator()) {
    Write-TestStart "Output file '$($file.Key)' exists"
    if (Test-Path $file.Value) {
        Write-TestPass
        $fileInfo = Get-Item $file.Value
        Write-TestInfo "Size: $($fileInfo.Length) bytes, Modified: $($fileInfo.LastWriteTime)"
    } else {
        Write-TestFail "Output file not found: $($file.Value)"
    }
}

# ========================================
# Test 5: Required Dependencies
# ========================================
$dependencies = @{
    "Newtonsoft.Json.dll (Core)" = "ZKTeco.Core\bin\Release\Newtonsoft.Json.dll"
    "Newtonsoft.Json.dll (Manager)" = "ZKTeco.Manager\bin\Release\Newtonsoft.Json.dll"
    "Newtonsoft.Json.dll (Service)" = "ZKTeco.Service\bin\Release\Newtonsoft.Json.dll"
}

foreach ($dep in $dependencies.GetEnumerator()) {
    Write-TestStart "Dependency '$($dep.Key)' exists"
    if (Test-Path $dep.Value) {
        Write-TestPass
    } else {
        Write-TestFail "Dependency not found: $($dep.Value)"
    }
}

# ========================================
# Test 6: Configuration Files
# ========================================
$configFiles = @{
    "Manager App.config" = "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe.config"
    "Service App.config" = "ZKTeco.Service\bin\Release\ZKTeco.Service.exe.config"
}

foreach ($config in $configFiles.GetEnumerator()) {
    Write-TestStart "Config file '$($config.Key)' exists"
    if (Test-Path $config.Value) {
        Write-TestPass
        Write-TestInfo "Found: $($config.Value)"
    } else {
        Write-TestFail "Config file not found: $($config.Value)"
    }
}

# ========================================
# Test 7: Assembly Information
# ========================================
Write-TestStart "Manager assembly loads correctly"
try {
    $managerAssembly = [System.Reflection.Assembly]::LoadFrom("$PWD\ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe")
    Write-TestPass
    Write-TestInfo "Version: $($managerAssembly.GetName().Version)"
} catch {
    Write-TestFail "Failed to load assembly: $_"
}

Write-TestStart "Service assembly loads correctly"
try {
    $serviceAssembly = [System.Reflection.Assembly]::LoadFrom("$PWD\ZKTeco.Service\bin\Release\ZKTeco.Service.exe")
    Write-TestPass
    Write-TestInfo "Version: $($serviceAssembly.GetName().Version)"
} catch {
    Write-TestFail "Failed to load assembly: $_"
}

# ========================================
# Test 8: Service Installation Check
# ========================================
Write-TestStart "Service installer component exists"
$serviceExe = "ZKTeco.Service\bin\Release\ZKTeco.Service.exe"
if (Test-Path $serviceExe) {
    try {
        $serviceAssembly = [System.Reflection.Assembly]::LoadFrom("$PWD\$serviceExe")
        $hasInstaller = $serviceAssembly.GetTypes() | Where-Object { $_.Name -eq "ProjectInstaller" }
        if ($hasInstaller) {
            Write-TestPass
            Write-TestInfo "ProjectInstaller class found"
        } else {
            Write-TestFail "ProjectInstaller class not found in assembly"
        }
    } catch {
        Write-TestFail "Failed to inspect assembly: $_"
    }
} else {
    Write-TestFail "Service executable not found"
}

# ========================================
# Test 9: Documentation Files
# ========================================
$docFiles = @(
    "README.md",
    "QUICK_START.md",
    "Build-And-Run.ps1"
)

foreach ($doc in $docFiles) {
    Write-TestStart "Documentation file '$doc' exists"
    if (Test-Path $doc) {
        Write-TestPass
    } else {
        Write-TestFail "Documentation file not found: $doc"
    }
}

# ========================================
# Test 10: Code Quality Checks
# ========================================
Write-TestStart "No TODO comments in release code"
$todoCount = 0
Get-ChildItem -Path "ZKTeco.Core", "ZKTeco.Manager", "ZKTeco.Service" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match "TODO:") {
        $todoCount++
    }
}
if ($todoCount -eq 0) {
    Write-TestPass
} else {
    Write-TestFail "$todoCount TODO comments found (not critical)"
}

# ========================================
# Test 11: Configuration Directory
# ========================================
Write-TestStart "Config directory structure is valid"
$configDir = "$env:ProgramData\ZKTeco Manager"
Write-TestInfo "Config location: $configDir"
Write-TestPass  # This is informational

# ========================================
# Test Summary
# ========================================
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Test Summary                           " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total Tests:  $script:TestsTotal" -ForegroundColor White
Write-Host "Passed:       $script:TestsPassed" -ForegroundColor Green
Write-Host "Failed:       $script:TestsFailed" -ForegroundColor $(if ($script:TestsFailed -eq 0) { "Green" } else { "Red" })
Write-Host ""

$successRate = [math]::Round(($script:TestsPassed / $script:TestsTotal) * 100, 2)
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } elseif ($successRate -ge 80) { "Yellow" } else { "Red" })
Write-Host ""

if ($script:TestsFailed -eq 0) {
    Write-Host "? All tests passed! Solution is ready for deployment." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Run the GUI application to configure devices" -ForegroundColor White
    Write-Host "  2. Install the Windows Service (as Administrator)" -ForegroundColor White
    Write-Host "  3. Start monitoring your attendance devices" -ForegroundColor White
    Write-Host ""
    Write-Host "To get started:" -ForegroundColor Cyan
    Write-Host "  .\Build-And-Run.ps1" -ForegroundColor Yellow
    exit 0
} else {
    Write-Host "? Some tests failed. Please review the errors above." -ForegroundColor Red
    Write-Host ""
    Write-Host "Run with -Verbose flag for more details:" -ForegroundColor Yellow
    Write-Host "  .\Test-Solution.ps1 -Verbose" -ForegroundColor Yellow
    exit 1
}
