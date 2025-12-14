# ZKTeco.PushAPI Deployment Script
# ?? ??????????? Administrator ?????? ?????

param(
    [string]$PublishPath = "C:\inetpub\wwwroot\ZKTecoPushAPI",
    [int]$Port = 8080,
    [string]$SiteName = "ZKTecoPushAPI",
    [switch]$SkipBuild,
    [switch]$TestOnly
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  ZKTeco.PushAPI Deployment Script" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "? Error: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = $scriptPath
$projectDir = Join-Path $solutionDir "ZKTeco.PushAPI"
$projectFile = Join-Path $projectDir "ZKTeco.PushAPI.csproj"

Write-Host "?? Solution Directory: $solutionDir" -ForegroundColor Gray
Write-Host "?? Project Directory: $projectDir" -ForegroundColor Gray
Write-Host "?? Publish Path: $PublishPath" -ForegroundColor Gray
Write-Host "?? Port: $Port" -ForegroundColor Gray
Write-Host ""

# Check if project exists
if (-not (Test-Path $projectFile)) {
    Write-Host "? Error: Project file not found at $projectFile" -ForegroundColor Red
    exit 1
}

# Test-only mode
if ($TestOnly) {
    Write-Host "?? Test Mode: Only checking if service is accessible" -ForegroundColor Yellow
    Write-Host ""
    
    try {
        $url = "http://localhost:$Port/api/iclock"
        Write-Host "Testing: $url" -ForegroundColor Gray
        $response = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 5
        Write-Host "? Service is accessible! Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "? Service is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    }
    exit 0
}

# Step 1: Build the project
if (-not $SkipBuild) {
    Write-Host "?? Step 1: Building project..." -ForegroundColor Yellow
    
    try {
        # Find MSBuild
        $msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
            -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
        
        if (-not $msbuildPath) {
            Write-Host "? MSBuild not found. Please install Visual Studio." -ForegroundColor Red
            exit 1
        }
        
        Write-Host "   Using MSBuild: $msbuildPath" -ForegroundColor Gray
        
        # Restore NuGet packages
        Write-Host "   Restoring NuGet packages..." -ForegroundColor Gray
        & nuget restore "$solutionDir\ZKTeco_Manager.sln"
        
        # Build the project
        Write-Host "   Building project..." -ForegroundColor Gray
        & $msbuildPath $projectFile /p:Configuration=Release /p:DeployOnBuild=true /p:PublishUrl=$PublishPath /t:Rebuild /v:minimal
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "? Build failed!" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "? Build successful!" -ForegroundColor Green
    }
    catch {
        Write-Host "? Build error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}
else {
    Write-Host "??  Skipping build step" -ForegroundColor Yellow
    Write-Host ""
}

# Step 2: Check/Install IIS
Write-Host "?? Step 2: Checking IIS installation..." -ForegroundColor Yellow

$iisFeature = Get-WindowsFeature -Name Web-Server -ErrorAction SilentlyContinue
if ($null -eq $iisFeature -or -not $iisFeature.Installed) {
    Write-Host "   Installing IIS..." -ForegroundColor Gray
    Install-WindowsFeature -Name Web-Server -IncludeManagementTools
    Write-Host "? IIS installed!" -ForegroundColor Green
}
else {
    Write-Host "? IIS already installed" -ForegroundColor Green
}

# Install ASP.NET 4.x
$aspNetFeature = Get-WindowsFeature -Name Web-Asp-Net45 -ErrorAction SilentlyContinue
if ($null -eq $aspNetFeature -or -not $aspNetFeature.Installed) {
    Write-Host "   Installing ASP.NET 4.x..." -ForegroundColor Gray
    Install-WindowsFeature -Name Web-Asp-Net45
}

Write-Host ""

# Step 3: Create publish directory
Write-Host "?? Step 3: Creating publish directory..." -ForegroundColor Yellow

if (-not (Test-Path $PublishPath)) {
    New-Item -Path $PublishPath -ItemType Directory -Force | Out-Null
    Write-Host "? Directory created: $PublishPath" -ForegroundColor Green
}
else {
    Write-Host "? Directory exists: $PublishPath" -ForegroundColor Green
}
Write-Host ""

# Step 4: Copy files to publish directory
Write-Host "?? Step 4: Copying files to publish directory..." -ForegroundColor Yellow

try {
    $binPath = Join-Path $projectDir "bin"
    
    if (Test-Path $binPath) {
        # Copy all files
        Copy-Item -Path "$projectDir\*" -Destination $PublishPath -Recurse -Force -Exclude @("obj", "*.user", "*.csproj*")
        Write-Host "? Files copied successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "??  Warning: bin folder not found. Please build the project first." -ForegroundColor Yellow
    }
}
catch {
    Write-Host "? Error copying files: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 5: Configure IIS
Write-Host "?? Step 5: Configuring IIS..." -ForegroundColor Yellow

Import-Module WebAdministration

# Check if site already exists
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Write-Host "   Website '$SiteName' already exists. Updating configuration..." -ForegroundColor Gray
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
    Remove-Website -Name $SiteName
}

# Check if app pool already exists
if (Test-Path "IIS:\AppPools\$SiteName") {
    Write-Host "   Application Pool '$SiteName' already exists. Recreating..." -ForegroundColor Gray
    Stop-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
    Remove-WebAppPool -Name $SiteName
}

# Create Application Pool
Write-Host "   Creating Application Pool..." -ForegroundColor Gray
New-WebAppPool -Name $SiteName
Set-ItemProperty "IIS:\AppPools\$SiteName" -Name managedRuntimeVersion -Value "v4.0"
Set-ItemProperty "IIS:\AppPools\$SiteName" -Name managedPipelineMode -Value "Integrated"

# Create Website
Write-Host "   Creating Website..." -ForegroundColor Gray
New-Website -Name $SiteName `
            -Port $Port `
            -PhysicalPath $PublishPath `
            -ApplicationPool $SiteName `
            -Force

# Start the website
Start-Website -Name $SiteName
Start-WebAppPool -Name $SiteName

Write-Host "? IIS configured successfully!" -ForegroundColor Green
Write-Host ""

# Step 6: Configure Firewall
Write-Host "?? Step 6: Configuring Firewall..." -ForegroundColor Yellow

$firewallRule = Get-NetFirewallRule -DisplayName $SiteName -ErrorAction SilentlyContinue
if ($null -eq $firewallRule) {
    Write-Host "   Creating firewall rule for port $Port..." -ForegroundColor Gray
    New-NetFirewallRule -DisplayName $SiteName `
                        -Direction Inbound `
                        -Protocol TCP `
                        -LocalPort $Port `
                        -Action Allow `
                        -Profile Any
    Write-Host "? Firewall rule created!" -ForegroundColor Green
}
else {
    Write-Host "? Firewall rule already exists" -ForegroundColor Green
}
Write-Host ""

# Step 7: Test the deployment
Write-Host "?? Step 7: Testing deployment..." -ForegroundColor Yellow

Start-Sleep -Seconds 3

try {
    $url = "http://localhost:$Port/api/iclock"
    Write-Host "   Testing: $url" -ForegroundColor Gray
    $response = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "? Service is accessible! Status: $($response.StatusCode)" -ForegroundColor Green
}
catch {
    Write-Host "??  Warning: Could not test service: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   The service might still be starting up." -ForegroundColor Gray
}
Write-Host ""

# Summary
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Deployment Summary" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "? Site Name: $SiteName" -ForegroundColor Green
Write-Host "? Port: $Port" -ForegroundColor Green
Write-Host "? Physical Path: $PublishPath" -ForegroundColor Green
Write-Host "? URL: http://localhost:$Port/api/iclock" -ForegroundColor Green
Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Test the API: http://localhost:$Port/api/iclock" -ForegroundColor Gray
Write-Host "   2. Configure ZKTeco device Push URL" -ForegroundColor Gray
Write-Host "   3. Monitor IIS logs: C:\inetpub\logs\LogFiles" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Useful Commands:" -ForegroundColor Yellow
Write-Host "   - View IIS logs:" -ForegroundColor Gray
Write-Host "     Get-Content C:\inetpub\logs\LogFiles\W3SVC*\*.log -Tail 50" -ForegroundColor Cyan
Write-Host "   - Restart website:" -ForegroundColor Gray
Write-Host "     Restart-Website -Name $SiteName" -ForegroundColor Cyan
Write-Host "   - View site status:" -ForegroundColor Gray
Write-Host "     Get-Website -Name $SiteName" -ForegroundColor Cyan
Write-Host ""
Write-Host "? Deployment completed successfully!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan
