# ?? ZKTeco.PushAPI - Quick Reference Card

## ?? ????? Deploy ???? (1 ?????)

```powershell
# 1. Administrator PowerShell ?????
# 2. ZKTeco_Manager ???????? ???
cd F:\SIKKHALOY-V3\ZKTeco_Manager

# 3. Deploy ????
.\Deploy-PushAPI.ps1

# 4. Test ????
Start-Process "http://localhost:8080/api/iclock"
```

---

## ?? ?????? Setup

**Push URL:**
```
http://[Server_IP]:8080/api/iclock
```

**Examples:**
- Local: `http://192.168.0.100:8080/api/iclock`
- Public: `http://103.45.67.89:8080/api/iclock`

---

## ??? ?????? ??????

### Deploy Commands
```powershell
# ?????? Deploy
.\Deploy-PushAPI.ps1

# ???? ?????? Deploy
.\Deploy-PushAPI.ps1 -Port 9090

# ???? Test
.\Deploy-PushAPI.ps1 -TestOnly

# ????? ????? ??? Deploy
.\Deploy-PushAPI.ps1 -SkipBuild
```

### Website Control
```powershell
# ???? ????
Start-Website -Name "ZKTecoPushAPI"

# ???? ????
Stop-Website -Name "ZKTecoPushAPI"

# ????????? ????
Restart-Website -Name "ZKTecoPushAPI"

# ????????? ?????
Get-Website -Name "ZKTecoPushAPI"
```

### Logs
```powershell
# IIS Logs
Get-Content C:\inetpub\logs\LogFiles\W3SVC*\*.log -Tail 50

# Application Logs
Get-EventLog -LogName Application -Source "ZKTeco*" -Newest 20
```

---

## ?? Troubleshooting

| ?????? | ?????? |
|--------|--------|
| Build ?????? | `nuget restore ZKTeco_Manager.sln` |
| Port blocked | `.\Deploy-PushAPI.ps1 -Port 9090` |
| Site ?? ???? | `Start-Website -Name "ZKTecoPushAPI"` |
| Firewall | `New-NetFirewallRule -DisplayName "ZKTeco" -LocalPort 8080 -Action Allow` |
| ???? ???? ?? | Device Push URL ??? ???? |

---

## ?? Testing

### Local Test
```powershell
Invoke-WebRequest -Uri "http://localhost:8080/api/iclock"
```

### Remote Test
```powershell
Invoke-WebRequest -Uri "http://192.168.0.100:8080/api/iclock"
```

### Browser Test
```
http://localhost:8080/api/iclock
```

---

## ?? Important Paths

| Item | Path |
|------|------|
| Published Files | `C:\inetpub\wwwroot\ZKTecoPushAPI` |
| IIS Logs | `C:\inetpub\logs\LogFiles\W3SVC*\` |
| Web.config | `C:\inetpub\wwwroot\ZKTecoPushAPI\Web.config` |
| Project | `F:\SIKKHALOY-V3\ZKTeco_Manager\ZKTeco.PushAPI\` |

---

## ?? URLs

| Type | URL |
|------|-----|
| Local | `http://localhost:8080/api/iclock` |
| LAN | `http://[Server_IP]:8080/api/iclock` |
| Public | `http://[Public_IP]:8080/api/iclock` |

---

## ?? Security Checklist

- [ ] HTTPS enabled (Production)
- [ ] IP Restriction configured
- [ ] Authentication added
- [ ] Firewall rules set
- [ ] Regular backups scheduled
- [ ] Logs monitored

---

## ?? Support Files

1. **????? ????**: `DEPLOYMENT_PUSHAPI_BANGLA.md`
2. **English Guide**: `DEPLOYMENT_PUSHAPI.md`
3. **Deploy Script**: `Deploy-PushAPI.ps1`
4. **Main README**: `README.md`

---

**Quick Deploy = Happy Users! ??**

Print this card and keep it handy! ??
