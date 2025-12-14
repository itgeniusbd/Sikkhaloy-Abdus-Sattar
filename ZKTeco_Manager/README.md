# ?? ZKTeco Device Manager

**Professional ZKTeco Attendance Device Management System**

---

## ? Features

### ??? Easy-to-Use GUI Application
- ? **Device Management** - Add, Edit, Delete devices with simple forms
- ? **Device Status Monitor** - Real-time device connection monitoring (????? + English)
- ? **Service Control** - Start, Stop, Restart Windows Service from GUI
- ? **Connection Testing** - Test device connectivity before deployment
- ? **Real-time Status** - Live service status monitoring
- ? **Auto-Refresh** - Automatic device status updates every 5 seconds
- ? **User-Friendly Interface** - No technical knowledge required
- ? **Push API Server** - Receive real-time attendance data from devices (NEW!)

### ?? Current Version (Phase 1 - GUI Only)
?? version ? ??? **GUI Manager Application** ??? ?? device configuration manage ???? ??????

**???????????:**
- ? Device configuration CRUD operations
- ? **Device Status Monitor with Ping Test** (NEW!)
- ? **Bilingual Support (English + ?????)** (NEW!)
- ? **Push API Server** for real-time data (NEW!)
- ? Service status monitoring
- ? Device connection test interface
- ? Configuration file management

**??????? Phase ? ??? ???:**
- ? Windows Background Service
- ? ZKTeco SDK Integration
- ? Auto-sync functionality
- ? MSI Installer
- ? System Tray Application

---

## ?? Quick Start

### Prerequisites
- Windows 7 or later
- .NET Framework 4.7.2 or higher
- Visual Studio 2019 or later (for building)
- IIS 7.5 or higher (for Push API)

### Building the Application

1. **Open Solution:**
   ```
   Open ZKTeco_Manager.sln in Visual Studio
   ```

2. **Restore NuGet Packages:**
   ```
   Right-click Solution ? Restore NuGet Packages
   ```

3. **Build Solution:**
   ```
   Build ? Build Solution (Ctrl+Shift+B)
   ```

4. **Run:**
   ```
   Press F5 or click Start
   ```

### Deploying Push API Server

**Quick Deploy (Automated):**
```powershell
# Run as Administrator
cd ZKTeco_Manager
.\Deploy-PushAPI.ps1
```

**Manual Deploy:**
See `DEPLOYMENT_PUSHAPI.md` for detailed instructions.

---

## ?? Usage Guide

### Checking Device Connection Status (NEW!)

**?????? ??????? ???? ???? ????? ??? ?????:**

1. Launch **ZKTeco Device Manager**
2. Click **"?? Device Status Monitor"** button (bottom right, green button)
3. See real-time status:
   - **? Green** = Connected / ??????? ?
   - **? Red** = Disconnected / ????????? ?
4. Auto-refresh every 5 seconds (checkbox ON)
5. Check ping time for network quality

**Features:**
- Real-time connection monitoring
- Ping test for each device
- Auto-refresh every 5 seconds
- Bilingual interface (English + ?????)
- Color-coded status indicators

?? **Detailed Guide:** See `DEVICE_STATUS_MONITOR_GUIDE.md`

---

### Setting Up Push API (NEW!)

**ZKTeco devices can push attendance data in real-time:**

1. Deploy Push API server (see above)
2. Configure device to push data:
   - In device settings, set Push URL to: `http://[YOUR_SERVER]:8080/api/iclock`
3. Device will automatically push attendance records
4. API receives and processes data

**Features:**
- Real-time attendance data reception
- RESTful API endpoint
- Automatic data processing
- No polling required

?? **Detailed Guide:** See `DEPLOYMENT_PUSHAPI.md`

---

## ?? Project Structure

```
ZKTeco_Manager/
??? ZKTeco.Core/                    # Core library (Shared code)
?   ??? Models/
?   ?   ??? DeviceConfig.cs         # Device configuration model
?   ??? Services/
?       ??? DeviceConfigService.cs  # Configuration management service
?
??? ZKTeco.Manager/                 # Windows Forms GUI Application
?   ??? MainForm.cs                 # Main application window
?   ??? DeviceEditForm.cs           # Device add/edit dialog
?   ??? DeviceStatusForm.cs         # Device status monitor (NEW!)
?   ??? Program.cs                  # Application entry point
?
??? ZKTeco.PushAPI/                 # Push API Server (NEW!)
?   ??? Controllers/
?   ?   ??? IclockController.cs     # API endpoint for device push
?   ??? App_Start/
?   ?   ??? WebApiConfig.cs         # Web API configuration
?   ??? Web.config                  # IIS configuration
?   ??? Global.asax                 # Application startup
?
??? ZKTeco_Manager.sln              # Visual Studio Solution
??? Deploy-PushAPI.ps1              # Automated deployment script (NEW!)
??? DEPLOYMENT_PUSHAPI.md           # Push API deployment guide (NEW!)
```

---

## ?? Configuration Storage

Device configurations are stored in:
```
C:\ProgramData\ZKTeco Manager\devices.json
```

**Example Configuration:**
```json
[
  {
    "Id": 1,
    "DeviceName": "Main Entrance",
    "DeviceIP": "192.168.0.201",
    "Port": 4370,
    "CommKey": 0,
    "IsActive": true,
    "SchoolID": 1,
    "ApiBaseUrl": "https://api.example.com",
    "ApiKey": "your-api-key-here",
    "CreatedDate": "2025-12-09T20:00:00",
    "LastSyncTime": null,
    "LastSyncStatus": null
  }
]
```

---

## ?? Development Roadmap

### ? Phase 1 - GUI Manager (COMPLETED)
- Device configuration management
- Service status monitoring interface
- Configuration file handling
- Device status monitor with ping test
- Push API server

### ?? Phase 2 - Background Service (IN PROGRESS)
- Windows Service implementation
- ZKTeco SDK integration
- Attendance data sync
- Auto-reconnection logic

### ?? Phase 3 - Installer
- MSI Installer package
- Auto DLL registration
- Service installation
- Desktop shortcuts

### ?? Phase 4 - System Tray
- System tray application
- Real-time sync status
- Quick access menu

---

## ??? Technologies Used

- **Language:** C# (.NET Framework 4.7.2)
- **UI Framework:** Windows Forms
- **Web API:** ASP.NET Web API 2
- **JSON Library:** Newtonsoft.Json
- **Service Management:** System.ServiceProcess
- **Web Server:** IIS

---

## ?? License

Copyright © 2025. All Rights Reserved.

---

## ?? Contributing

This is currently a private project.

---

## ?? Support

For support and questions, please contact the development team.

---

## ? Quick Commands

### Build Release Version
```powershell
cd F:\SIKKHALOY-V3\ZKTeco_Manager
msbuild ZKTeco_Manager.sln /p:Configuration=Release
```

### Run Application
```powershell
.\ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe
```

### Deploy Push API
```powershell
# Run as Administrator
.\Deploy-PushAPI.ps1
```

### Test Push API
```powershell
# Test locally
.\Deploy-PushAPI.ps1 -TestOnly

# Deploy to custom port
.\Deploy-PushAPI.ps1 -Port 9090

# Deploy to custom path
.\Deploy-PushAPI.ps1 -PublishPath "D:\WebApps\ZKTecoPushAPI"
```

---

**Made with ?? for Educational Institutions**
