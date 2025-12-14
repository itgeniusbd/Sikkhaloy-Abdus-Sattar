@echo off
echo ==========================================
echo ZKTeco Push API - IIS Deployment
echo ==========================================
echo.
echo This will deploy ZKTeco Push API to IIS
echo.
echo Requirements:
echo - Run as Administrator
echo - IIS must be installed
echo.
pause

PowerShell -NoProfile -ExecutionPolicy Bypass -File "%~dp0DeployToIIS.ps1"

pause
