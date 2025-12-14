# ?? Device Management - Quick Reference

## ?????? ?: Device ??? School ?? ????? ???????

**?????:** Device table ? DeviceSerial ????? SchoolID map ??? ????

```sql
-- Device register ????
INSERT INTO Device (DeviceSerial, DeviceName, DeviceIP, Port, SchoolID)
VALUES ('BBKB193260050', 'Main Gate', '192.168.0.201', 4370, 1)

-- Device Serial ????? School ??????
SELECT SchoolID FROM Device WHERE DeviceSerial = 'BBKB193260050'
```

---

## ?????? ?: ????????? ???? ???? Device ? ?????? ???????

**?????:** Device Sync Tool ??????? ?????

### Quick Steps:

#### 1. API ???? Data ???:
```http
GET /api/device/{deviceId}/users
GET /api/device/{deviceId}/fingerprints
```

#### 2. Sync Tool ?????:
```powershell
cd ZKTeco.DeviceSync\bin\Debug
.\ZKTeco.DeviceSync.exe
# Enter Device ID: 1
```

#### 3. Done! ?

---

## ?? ?????: AttendanceDevice vs New System

| Feature | AttendanceDevice | New System |
|---------|------------------|------------|
| Data Source | SQLite Local | SQL Server (API) |
| User Sync | Local app | Sync Tool / API |
| Attendance | Periodic sync | Real-time push |
| Remote Access | ? | ? |
| Multi-device | ? | ? |

---

## ?? Setup (5 Minutes)

### 1. Database Setup (1 min)
```sql
INSERT INTO Device (DeviceSerial, DeviceName, DeviceIP, Port, SchoolID)
VALUES ('YOUR_DEVICE_SERIAL', 'Main Gate', '192.168.0.201', 4370, 1)
```

### 2. Deploy API (2 min)
```powershell
.\Deploy-PushAPI.ps1
```

### 3. Configure Sync Tool (1 min)
Edit `App.config`:
```xml
<add key="ApiBaseUrl" value="http://YOUR_SERVER:8080/" />
```

### 4. Sync Data (1 min)
```powershell
.\ZKTeco.DeviceSync.exe
# Enter Device ID
```

---

## ?? Common Commands

### Get Device Info:
```powershell
Invoke-RestMethod "http://localhost:8080/api/device/school/1"
```

### Get Users:
```powershell
Invoke-RestMethod "http://localhost:8080/api/device/1/users"
```

### Sync to Device:
```powershell
.\ZKTeco.DeviceSync.exe
```

---

## ? Checklist

- [ ] Device table ? device info ???
- [ ] Student/Employee ? DeviceID set ???
- [ ] API deploy ??????
- [ ] Sync tool configure ???
- [ ] Test ??? ??????

---

**Full Guide:** `DEVICE_MANAGEMENT_GUIDE.md`

**???, ?????, ??? ???????! ??**
