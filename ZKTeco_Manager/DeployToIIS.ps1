# IIS ? ZKTeco Push API Deploy ???? Script
# Run as Administrator

Write-Host "=== ZKTeco Push API - IIS Deployment ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$siteName = "ZKTeco.PushAPI"
$appPoolName = "ZKTeco.PushAPI.AppPool"
$physicalPath = "C:\inetpub\ZKTecoPushAPI"
$sourcePath = "F:\SIKKHALOY-V3\ZKTeco_Manager\ZKTeco.PushAPI\bin\Release\Publish"
$port = 8080
$hostName = "api.sikkhaloy.com"

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit
}

# Import IIS Module
Import-Module WebAdministration -ErrorAction SilentlyContinue

# Check if IIS is installed
if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
    Write-Host "ERROR: IIS is not installed!" -ForegroundColor Red
    Write-Host "Install IIS from: Control Panel > Programs > Turn Windows features on or off" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "Step 1: Creating Application Pool..." -ForegroundColor Green

# Remove existing app pool if exists
if (Test-Path IIS:\AppPools\$appPoolName) {
    Write-Host "  - Removing existing app pool..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $appPoolName
}

# Create new app pool
New-WebAppPool -Name $appPoolName
Set-ItemProperty IIS:\AppPools\$appPoolName -Name "managedRuntimeVersion" -Value "v4.0"
Set-ItemProperty IIS:\AppPools\$appPoolName -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty IIS:\AppPools\$appPoolName -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty IIS:\AppPools\$appPoolName -Name "enable32BitAppOnWin64" -Value $false

Write-Host "  - App Pool created successfully!" -ForegroundColor Green

Write-Host ""
Write-Host "Step 2: Creating Website..." -ForegroundColor Green

# Remove existing site if exists
if (Get-Website -Name $siteName -ErrorAction SilentlyContinue) {
    Write-Host "  - Removing existing website..." -ForegroundColor Yellow
    Remove-Website -Name $siteName
}

# Create new website
New-Website -Name $siteName `
    -PhysicalPath $physicalPath `
    -ApplicationPool $appPoolName `
    -Port $port `
    -Force

Write-Host "  - Website created successfully!" -ForegroundColor Green

Write-Host ""
Write-Host "Step 3: Configuring Bindings..." -ForegroundColor Green

# Add binding for localhost
New-WebBinding -Name $siteName -Protocol http -Port $port -IPAddress "*" -HostHeader "" -Force

# Add binding for api.sikkhaloy.com (if needed)
# New-WebBinding -Name $siteName -Protocol http -Port 80 -HostHeader $hostName -Force

Write-Host "  - Bindings configured!" -ForegroundColor Green

Write-Host ""
Write-Host "Step 4: Setting Permissions..." -ForegroundColor Green

# Set folder permissions
$acl = Get-Acl $physicalPath
$permission = "IIS_IUSRS", "Read,ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $physicalPath $acl

$permission2 = "IUSR", "Read,ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule2 = New-Object System.Security.AccessControl.FileSystemAccessRule $permission2
$acl.SetAccessRule($accessRule2)
Set-Acl $physicalPath $acl

Write-Host "  - Permissions set!" -ForegroundColor Green

Write-Host ""
Write-Host "Step 5: Starting Website..." -ForegroundColor Green

Start-Website -Name $siteName
Start-WebAppPool -Name $appPoolName

Write-Host "  - Website started!" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Access your API at:" -ForegroundColor Yellow
Write-Host "  http://localhost:$port" -ForegroundColor White
Write-Host "  http://localhost:$port/iclock/ping" -ForegroundColor White
Write-Host "  http://localhost:$port/index.html" -ForegroundColor White
Write-Host ""
Write-Host "Test URLs:" -ForegroundColor Yellow
Write-Host "  Ping: http://localhost:$port/iclock/ping" -ForegroundColor Cyan
Write-Host "  Handshake: http://localhost:$port/iclock/cdata?SN=TEST123&options=all" -ForegroundColor Cyan
Write-Host "  Test Panel: http://localhost:$port/index.html" -ForegroundColor Cyan
Write-Host ""

# Test the API
Write-Host "Testing API..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:$port/iclock/ping" -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "SUCCESS! API is responding: $($response.Content)" -ForegroundColor Green
    }
} catch {
    Write-Host "WARNING: Could not test API automatically. Please test manually." -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to open Test Panel in browser..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Start-Process "http://localhost:$port/index.html"

Write-Host ""
Write-Host "To remove this site, run:" -ForegroundColor Yellow
Write-Host "  Remove-Website -Name '$siteName'" -ForegroundColor White
Write-Host "  Remove-WebAppPool -Name '$appPoolName'" -ForegroundColor White
Write-Host ""
