@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Remove-Old-SmsSender.ps1" %*
if errorlevel 1 (
    echo.
    echo Remove failed.
    pause
    exit /b 1
)
echo.
pause
