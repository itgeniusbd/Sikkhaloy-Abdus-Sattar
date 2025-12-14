@echo off
echo Stopping ZKTeco Service before build...
net stop "ZKTeco.Service" 2>nul
taskkill /F /IM ZKTeco.Service.exe /T 2>nul
taskkill /F /IM w3wp.exe /T 2>nul
timeout /t 2 /nobreak >nul
echo Ready to build!
