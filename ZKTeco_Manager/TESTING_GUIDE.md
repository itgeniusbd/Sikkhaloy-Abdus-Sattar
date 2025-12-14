# ?? Manual Testing Guide

## Test Results Summary
? **Automated Tests:** 19/20 Passed (95% Success Rate)  
?? **Minor Issue:** 3 TODO comments in code (non-critical, for future enhancements)

---

## ?? Manual Testing Checklist

### ? Phase 1: GUI Application Testing

#### Test 1.1: Application Launches
```powershell
# Run the application
cd ZKTeco.Manager\bin\Release
.\ZKTeco.Manager.exe
```

**Expected:**
- ? Application window opens
- ? Main form displays with empty device grid
- ? Menu bar visible (File, Devices, Service, Help)
- ? Status bar shows "Ready"
- ? Service status shows "Service: Not Installed" (if not installed yet)

**Screenshot Location:** `[Document this]`

---

#### Test 1.2: Add Device
1. Click `Devices` ? `Add Device...`
2. Fill in test data:
   ```
   Device Name:    Test Device
   Device IP:      192.168.1.100
   Port:           4370
   Comm Key:       0
   School ID:      1
   API Base URL:   http://localhost:5000
   API Key:        test-api-key-12345
   ```
3. Click `Save`

**Expected:**
- ? Device appears in the grid
- ? Success message shows: "Device added successfully!"
- ? Status bar updates: "Loaded 1 device(s)"

**Actual Result:** `[Fill after testing]`

---

#### Test 1.3: Edit Device
1. Select the device in grid
2. Click `Edit` button
3. Modify Device Name to: "Test Device (Modified)"
4. Click `Save`

**Expected:**
- ? Device name updates in grid
- ? Success message shows
- ? Changes persist after refresh

**Actual Result:** `[Fill after testing]`

---

#### Test 1.4: Test Connection
1. Select device in grid
2. Click `Test Connection` button

**Expected:**
- ? Ping test runs
- ? Shows connection result (success/fail based on IP reachability)
- ? Message displays device details

**Actual Result:** `[Fill after testing]`

---

#### Test 1.5: Delete Device
1. Select device in grid
2. Click `Delete` button
3. Confirm deletion

**Expected:**
- ? Confirmation dialog appears
- ? Device removed from grid after confirmation
- ? Success message shows
- ? Status bar updates

**Actual Result:** `[Fill after testing]`

---

#### Test 1.6: Persistence Check
1. Add a device
2. Close application
3. Reopen application

**Expected:**
- ? Device configuration persists
- ? Device loads automatically
- ? Configuration file exists at: `C:\ProgramData\ZKTeco Manager\devices.json`

**Actual Result:** `[Fill after testing]`

---

### ? Phase 2: Windows Service Testing

#### Test 2.1: Service Installation
```powershell
# Run PowerShell as Administrator
cd ZKTeco.Service\bin\Release
installutil.exe ZKTeco.Service.exe
```

**Expected:**
- ? Installation succeeds
- ? Service appears in Services console (services.msc)
- ? Service properties show:
  - Name: ZKTeco Device Service
  - Startup Type: Automatic (Delayed Start)
  - Status: Stopped

**Actual Result:** `[Fill after testing]`

---

#### Test 2.2: Service Start via GUI
1. Open ZKTeco Manager
2. Click `Service` ? `Start Service`

**Expected:**
- ? Service starts successfully
- ? Success message appears
- ? Service status updates to: "Service: Running" (green)
- ? Status updates every 2 seconds

**Actual Result:** `[Fill after testing]`

---

#### Test 2.3: Service Event Logs
```powershell
# Check event logs
Get-EventLog -LogName Application -Source "ZKTeco Device Service" -Newest 10
```

**Expected:**
- ? Service start event logged
- ? "ZKTeco Device Service starting..." message
- ? "ZKTeco Device Service started successfully" message
- ? Monitoring cycle logs appear

**Actual Result:** `[Fill after testing]`

---

#### Test 2.4: Service Monitoring Cycle
**Prerequisites:** At least one active device configured

**Wait 1 minute, then check logs:**
```powershell
Get-EventLog -LogName Application -Source "ZKTeco Device Service" -Newest 5
```

**Expected:**
- ? "Starting monitoring cycle..." log
- ? "Found X active device(s)" log
- ? "Processing device: [DeviceName]" log
- ? "Monitoring cycle completed" log

**Actual Result:** `[Fill after testing]`

---

#### Test 2.5: Service Stop
1. In GUI: Click `Service` ? `Stop Service`

**Expected:**
- ? Service stops successfully
- ? Success message appears
- ? Service status updates to: "Service: Stopped" (red)
- ? Event log shows stop message

**Actual Result:** `[Fill after testing]`

---

#### Test 2.6: Service Restart
1. Start service
2. Click `Service` ? `Restart Service`

**Expected:**
- ? Service stops and starts
- ? Success message appears
- ? Event logs show stop and start events
- ? Monitoring resumes

**Actual Result:** `[Fill after testing]`

---

### ? Phase 3: Configuration & Data Testing

#### Test 3.1: Configuration File Structure
```powershell
# View configuration
cat "C:\ProgramData\ZKTeco Manager\devices.json"
```

**Expected:**
```json
[
  {
    "Id": 1,
    "DeviceName": "Test Device",
    "DeviceIP": "192.168.1.100",
    "Port": 4370,
    "CommKey": 0,
    "IsActive": true,
    "SchoolID": 1,
    "ApiBaseUrl": "http://localhost:5000",
    "ApiKey": "test-api-key-12345",
    "CreatedDate": "2025-01-15T...",
    "LastSyncTime": null,
    "LastSyncStatus": null
  }
]
```

**Actual Result:** `[Fill after testing]`

---

#### Test 3.2: Multiple Devices
1. Add 3 devices with different configurations
2. Verify all appear in grid
3. Deactivate one device (uncheck "Is Active")
4. Check service only processes active devices

**Expected:**
- ? All 3 devices stored in JSON
- ? Only 2 active devices processed by service
- ? Event log shows: "Found 2 active device(s)"

**Actual Result:** `[Fill after testing]`

---

### ? Phase 4: Error Handling Testing

#### Test 4.1: Invalid IP Address
1. Add device with invalid IP: "999.999.999.999"
2. Try to save

**Expected:**
- ? Validation error appears
- ? Cannot save invalid IP
- ? Focus returns to IP field

**Actual Result:** `[Fill after testing]`

---

#### Test 4.2: Invalid API URL
1. Add device with invalid URL: "not-a-url"
2. Try to save

**Expected:**
- ? Validation error appears
- ? Message: "Please enter a valid API Base URL"
- ? Cannot save

**Actual Result:** `[Fill after testing]`

---

#### Test 4.3: Empty Required Fields
1. Try to save device without filling required fields

**Expected:**
- ? Validation errors for each empty field
- ? Specific error messages
- ? Focus moves to first error field

**Actual Result:** `[Fill after testing]`

---

#### Test 4.4: Service Not Installed
1. Try to start service when not installed
2. Click `Service` ? `Start Service`

**Expected:**
- ? Error message: "Service is not installed. Please run the installer first."
- ? Service status shows: "Not Installed"

**Actual Result:** `[Fill after testing]`

---

### ? Phase 5: Integration Testing

#### Test 5.1: API Endpoint (Mock)
**Setup:** Create a simple API endpoint to receive data

```csharp
// Mock API endpoint for testing
// POST http://localhost:5000/api/Attendance/1/Students
```

**Expected:**
- ? Service sends attendance data
- ? API receives POST request
- ? Headers include Authorization: Bearer {ApiKey}
- ? Body contains attendance records

**Actual Result:** `[Fill after testing]`

---

#### Test 5.2: Unreachable Device
1. Configure device with unreachable IP
2. Wait for monitoring cycle

**Expected:**
- ? Service logs: "Error processing device"
- ? Continues to next device
- ? Service doesn't crash

**Actual Result:** `[Fill after testing]`

---

### ? Phase 6: Performance Testing

#### Test 6.1: Large Device List
1. Add 10+ devices
2. Monitor memory usage
3. Check monitoring cycle time

**Expected:**
- ? All devices load in < 2 seconds
- ? Grid scrolls smoothly
- ? Memory usage reasonable (< 100MB)
- ? Monitoring cycle completes in < 30 seconds

**Actual Result:** `[Fill after testing]`

---

## ?? Test Results Template

### Summary
- **Date Tested:** `[Fill in]`
- **Tester:** `[Your name]`
- **Environment:** Windows [Version]
- **Total Tests:** 25
- **Passed:** `[Fill in]`
- **Failed:** `[Fill in]`
- **Success Rate:** `[Fill in]%`

### Issues Found
1. `[Issue description]`
   - Severity: `[High/Medium/Low]`
   - Steps to reproduce: `[Steps]`
   - Expected: `[Expected behavior]`
   - Actual: `[Actual behavior]`

### Recommendations
- `[List any recommendations]`

---

## ?? Verification Commands

```powershell
# Check if all builds exist
Test-Path "ZKTeco.Core\bin\Release\ZKTeco.Core.dll"
Test-Path "ZKTeco.Manager\bin\Release\ZKTeco.Manager.exe"
Test-Path "ZKTeco.Service\bin\Release\ZKTeco.Service.exe"

# Check service status
Get-Service "ZKTeco Device Service"

# View recent service logs
Get-EventLog -LogName Application -Source "ZKTeco Device Service" -Newest 20

# Check configuration
Get-Content "C:\ProgramData\ZKTeco Manager\devices.json" | ConvertFrom-Json

# Service operations
sc start "ZKTeco Device Service"
sc stop "ZKTeco Device Service"
sc query "ZKTeco Device Service"
```

---

## ? Sign-off

**Tested by:** `_________________________`  
**Date:** `_________________________`  
**Signature:** `_________________________`  

**Approved for:** ? Development  ? Testing  ? Production

---

**Next Steps After Testing:**
1. Fix any critical issues found
2. Document known limitations
3. Prepare deployment package
4. Create user training materials
5. Deploy to production environment
