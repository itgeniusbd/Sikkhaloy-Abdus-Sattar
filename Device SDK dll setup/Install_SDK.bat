@echo off
setlocal

:: Re-launch as Administrator if needed
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting Administrator permission...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

set "SRC=%~dp0Copy Paste dll to System Folder"
if not exist "%SRC%\zkemkeeper.dll" (
    echo ERROR: zkemkeeper.dll not found in:
    echo %SRC%
    pause
    exit /b 1
)

if /i "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set "TARGET=%windir%\SysWOW64"
    echo Installing 32-bit ZKTeco SDK to %TARGET% ...
) else (
    set "TARGET=%windir%\System32"
    echo Installing ZKTeco SDK to %TARGET% ...
)

copy /Y "%SRC%\*.dll" "%TARGET%\" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy DLL files.
    pause
    exit /b 1
)

if not exist "%TARGET%\zkemkeeper.dll" (
    echo ERROR: zkemkeeper.dll was not copied.
    pause
    exit /b 1
)

echo Registering zkemkeeper.dll ...
regsvr32 /s "%TARGET%\zkemkeeper.dll"
if errorlevel 1 (
    echo ERROR: regsvr32 failed.
    pause
    exit /b 1
)

echo.
echo SUCCESS: ZKTeco SDK installed.
echo %TARGET%\zkemkeeper.dll
pause
