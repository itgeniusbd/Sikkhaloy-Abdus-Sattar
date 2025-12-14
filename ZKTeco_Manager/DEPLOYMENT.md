# ?? Deployment Checklist

## Pre-Deployment

### ? Code Quality
- [x] All projects build successfully
- [x] No compilation errors
- [x] No critical warnings
- [ ] Code review completed
- [ ] All TODOs addressed or documented

### ? Testing
- [x] Automated tests passed (95%)
- [ ] Manual testing completed
- [ ] Integration testing done
- [ ] Performance testing satisfactory
- [ ] Security review completed

### ? Documentation
- [x] README.md complete
- [x] QUICK_START.md available
- [x] TESTING_GUIDE.md created
- [ ] API documentation prepared
- [ ] User manual created

### ? Configuration
- [x] App.config files verified
- [x] Connection strings reviewed
- [ ] API endpoints configured
- [ ] Security keys updated
- [ ] Logging levels set

---

## Build for Production

### 1. Clean Build
```powershell
# Clean previous builds
msbuild ZKTeco_Manager.sln /t:Clean /p:Configuration=Release

# Rebuild solution
msbuild ZKTeco_Manager.sln /t:Rebuild /p:Configuration=Release /v:minimal
```

### 2. Verify Outputs
```powershell
# Check all binaries exist
Test-Path "ZKTeco.Core\bin\Release\ZKTeco.Core.dll"
Test-Path "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe"
Test-Path "ZKTeco.Service\bin\Release\ZKTeco.Service.exe"

# Check dependencies
Test-Path "ZKTeco.Manager\bin\Release\Newtonsoft.Json.dll"
Test-Path "ZKTeco.Manager\bin\Release\ZKTeco.Core.dll"
```

### 3. Code Signing (Optional but Recommended)
```powershell
# Sign executables with certificate
signtool sign /f "certificate.pfx" /p "password" /t "http://timestamp.server.com" `
    "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe"
    
signtool sign /f "certificate.pfx" /p "password" /t "http://timestamp.server.com" `
    "ZKTeco.Service\bin\Release\ZKTeco.Service.exe"
```

---

## Package for Distribution

### Option 1: ZIP Package (Simple)

```powershell
# Create deployment folder
$deployPath = "Deploy\ZKTeco_Manager_v1.0"
New-Item -ItemType Directory -Path $deployPath -Force

# Copy Manager
Copy-Item "ZKTeco.Manager\bin\Release\*" "$deployPath\Manager\" -Recurse

# Copy Service
Copy-Item "ZKTeco.Service\bin\Release\*" "$deployPath\Service\" -Recurse

# Copy Documentation
Copy-Item "README.md" "$deployPath\"
Copy-Item "QUICK_START.md" "$deployPath\"
Copy-Item "TESTING_GUIDE.md" "$deployPath\"

# Create installation scripts
@"
@echo off
echo Installing ZKTeco Device Service...
cd Service
installutil.exe ZKTeco.Service.exe
echo.
echo Service installed successfully!
echo.
echo To start the service:
echo   sc start "ZKTeco Device Service"
echo.
pause
"@ | Out-File "$deployPath\Install-Service.bat" -Encoding ASCII

@"
@echo off
echo Uninstalling ZKTeco Device Service...
cd Service
installutil.exe /u ZKTeco.Service.exe
echo.
echo Service uninstalled successfully!
pause
"@ | Out-File "$deployPath\Uninstall-Service.bat" -Encoding ASCII

# Create ZIP
Compress-Archive -Path "$deployPath\*" -DestinationPath "Deploy\ZKTeco_Manager_v1.0.zip" -Force
```

### Option 2: MSI Installer (Advanced)

**Prerequisites:**
- WiX Toolset 3.11+ installed
- Visual Studio WiX Extension

**Create WiX Project:**

1. Add new project: "ZKTeco.Installer" (WiX Setup Project)

2. Create Product.wxs:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" 
           Name="ZKTeco Device Manager" 
           Language="1033" 
           Version="1.0.0.0" 
           Manufacturer="Your Company" 
           UpgradeCode="PUT-GUID-HERE">
    
    <Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />
    
    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="ZKTeco Manager" Level="1">
      <ComponentGroupRef Id="ManagerFiles" />
      <ComponentGroupRef Id="ServiceFiles" />
      <ComponentRef Id="ServiceInstall" />
    </Feature>

    <!-- Install directories -->
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="ZKTeco Manager">
          <Directory Id="ManagerDir" Name="Manager" />
          <Directory Id="ServiceDir" Name="Service" />
        </Directory>
      </Directory>
      <Directory Id="ProgramMenuFolder" />
      <Directory Id="DesktopFolder" />
    </Directory>

    <!-- Start menu shortcut -->
    <DirectoryRef Id="ProgramMenuFolder">
      <Component Id="ApplicationShortcut" Guid="PUT-GUID-HERE">
        <Shortcut Id="ApplicationStartMenuShortcut"
                  Name="ZKTeco Device Manager"
                  Description="Manage ZKTeco attendance devices"
                  Target="[ManagerDir]ZKTeco.Manager.exe"
                  WorkingDirectory="ManagerDir" />
        <RemoveFolder Id="CleanUpShortcut" Directory="ProgramMenuFolder" On="uninstall" />
        <RegistryValue Root="HKCU" 
                       Key="Software\ZKTeco\Manager" 
                       Name="installed" 
                       Type="integer" 
                       Value="1" 
                       KeyPath="yes" />
      </Component>
    </DirectoryRef>

    <!-- Service installation -->
    <DirectoryRef Id="ServiceDir">
      <Component Id="ServiceInstall" Guid="PUT-GUID-HERE">
        <File Id="ServiceExe" 
              Source="$(var.ZKTeco.Service.TargetPath)" 
              KeyPath="yes" />
        
        <ServiceInstall Id="ServiceInstaller"
                        Name="ZKTeco Device Service"
                        DisplayName="ZKTeco Device Service"
                        Description="Monitors ZKTeco devices and syncs attendance"
                        Type="ownProcess"
                        Start="auto"
                        ErrorControl="normal"
                        Account="LocalSystem" />
        
        <ServiceControl Id="ServiceControl"
                        Name="ZKTeco Device Service"
                        Start="install"
                        Stop="both"
                        Remove="uninstall" />
      </Component>
    </DirectoryRef>

  </Product>

  <Fragment>
    <ComponentGroup Id="ManagerFiles" Directory="ManagerDir">
      <Component Id="ManagerExe" Guid="PUT-GUID-HERE">
        <File Source="$(var.ZKTeco.Manager.TargetPath)" />
      </Component>
      <!-- Add other Manager files -->
    </ComponentGroup>

    <ComponentGroup Id="ServiceFiles" Directory="ServiceDir">
      <!-- Service files already included in ServiceInstall component -->
    </ComponentGroup>
  </Fragment>
</Wix>
```

---

## Deployment Steps

### Target Environment Requirements

#### Minimum Requirements:
- **OS:** Windows 7 SP1 / Windows Server 2008 R2 or higher
- **.NET Framework:** 4.7.2 or higher
- **RAM:** 512 MB minimum, 1 GB recommended
- **Disk Space:** 50 MB
- **Network:** TCP/IP connectivity to devices
- **Privileges:** Administrator rights for service installation

#### Software Dependencies:
- [x] .NET Framework 4.7.2+
- [x] Visual C++ Redistributable (if needed)

### Installation Steps

#### 1. Pre-Installation
```powershell
# Check .NET Framework version
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\' | 
    Get-ItemPropertyValue -Name Version
```

#### 2. Copy Files
```powershell
# Option A: From ZIP
Expand-Archive -Path "ZKTeco_Manager_v1.0.zip" -DestinationPath "C:\Program Files\ZKTeco Manager"

# Option B: Using MSI
msiexec /i ZKTeco_Manager_v1.0.msi /qn /l*v install.log
```

#### 3. Install Service (Manual Installation)
```powershell
# Run as Administrator
cd "C:\Program Files\ZKTeco Manager\Service"
installutil.exe ZKTeco.Service.exe

# Verify installation
Get-Service "ZKTeco Device Service"
```

#### 4. Configure Application
1. Run ZKTeco.Manager.exe
2. Add first device
3. Test connection
4. Save configuration

#### 5. Start Service
```powershell
Start-Service "ZKTeco Device Service"

# Verify running
Get-Service "ZKTeco Device Service"
Get-EventLog -LogName Application -Source "ZKTeco Device Service" -Newest 5
```

---

## Post-Deployment Verification

### 1. Service Status
```powershell
# Check service is running
$service = Get-Service "ZKTeco Device Service"
Write-Host "Service Status: $($service.Status)"
Write-Host "Startup Type: $($service.StartType)"
```

### 2. Application Launches
```powershell
# Test GUI application
Start-Process "C:\Program Files\ZKTeco Manager\Manager\ZKTeco.Manager.exe"
```

### 3. Configuration Exists
```powershell
# Check config directory
Test-Path "C:\ProgramData\ZKTeco Manager"
Test-Path "C:\ProgramData\ZKTeco Manager\devices.json"
```

### 4. Event Logs Working
```powershell
# Check event log entries
Get-EventLog -LogName Application -Source "ZKTeco Device Service" -Newest 10
```

### 5. Network Connectivity
```powershell
# Test device connectivity (example)
Test-NetConnection -ComputerName "192.168.1.100" -Port 4370
```

---

## Rollback Plan

### If Deployment Fails:

#### 1. Stop Service
```powershell
Stop-Service "ZKTeco Device Service" -Force
```

#### 2. Uninstall Service
```powershell
cd "C:\Program Files\ZKTeco Manager\Service"
installutil.exe /u ZKTeco.Service.exe
```

#### 3. Remove Files
```powershell
Remove-Item "C:\Program Files\ZKTeco Manager" -Recurse -Force
```

#### 4. Clean Registry (if needed)
```powershell
Remove-Item "HKCU:\Software\ZKTeco" -Recurse -ErrorAction SilentlyContinue
```

#### 5. Restore Previous Version
```powershell
# Restore from backup
Copy-Item "Backup\ZKTeco Manager\*" "C:\Program Files\ZKTeco Manager\" -Recurse
```

---

## Monitoring & Maintenance

### Daily Checks
- [ ] Service is running
- [ ] No error events in log
- [ ] Devices are reachable
- [ ] Data sync is working

### Weekly Checks
- [ ] Review event logs
- [ ] Check disk space
- [ ] Verify API connectivity
- [ ] Update device configurations

### Monthly Checks
- [ ] Review performance metrics
- [ ] Update documentation
- [ ] Plan upgrades
- [ ] Backup configurations

---

## Support & Troubleshooting

### Common Issues

#### Service Won't Start
**Solution:**
1. Check event logs
2. Verify .NET Framework installed
3. Check file permissions
4. Reinstall service

#### Can't Connect to Device
**Solution:**
1. Ping device IP
2. Check firewall settings
3. Verify device is powered on
4. Check network configuration

#### API Sync Fails
**Solution:**
1. Verify API URL and key
2. Check network connectivity
3. Review API logs
4. Check authentication

### Support Contacts
- **Technical Support:** [Email/Phone]
- **Documentation:** [URL]
- **Bug Reports:** [URL]

---

## Deployment Sign-off

**Deployed by:** `_________________________`  
**Date:** `_________________________`  
**Version:** `1.0.0.0`  
**Environment:** ? Development  ? Testing  ? Production  

**Verification Completed:** ? Yes  ? No  
**Issues Found:** ? None  ? See notes below  

**Notes:**
```
[Add any deployment notes here]
```

**Approved by:** `_________________________`  
**Date:** `_________________________`  
**Signature:** `_________________________`
