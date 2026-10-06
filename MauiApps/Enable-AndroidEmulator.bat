@echo off
:: Right-click -> Run as administrator
:: Fixes: "Android Emulator hypervisor driver is not installed" / Hyper-V disabled
echo ============================================
echo  Enable Android Emulator virtualization
echo ============================================
echo.

dism /online /enable-feature /featurename:HypervisorPlatform /all /norestart
if errorlevel 1 (
  echo FAILED to enable HypervisorPlatform. Run this as Administrator.
  pause
  exit /b 1
)

dism /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart

echo.
echo SUCCESS. You MUST restart Windows now.
echo After reboot:
echo   1. Open InstitutionWebViewApps.sln
echo   2. Start Pixel 7 - API 34 emulator
echo   3. Select net10.0-android + emulator, Run MgcApp
echo.
set /p R=Restart now? (Y/N): 
if /I "%R%"=="Y" shutdown /r /t 0
pause
