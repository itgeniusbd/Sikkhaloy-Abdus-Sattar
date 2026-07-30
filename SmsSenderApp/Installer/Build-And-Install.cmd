@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-And-Install.ps1" %*
if errorlevel 1 (
    echo.
    echo Build/Install failed.
    pause
    exit /b 1
)
echo.
pause
