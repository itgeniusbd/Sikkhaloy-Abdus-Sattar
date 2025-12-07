@echo off
echo =============================================
echo SIKKHALOY Database Index Optimization
echo =============================================
echo.

REM Database configuration - ????? ????? ???? ???
set SERVER_NAME=LOOPS-IT-VM-1
set DATABASE_NAME=SIKKHALOY_DB
set USERNAME=sa
set PASSWORD=YourPassword

echo Server: %SERVER_NAME%
echo Database: %DATABASE_NAME%
echo.
echo Script ?????? ?????...
echo.

REM SQL Server ?? path (??????? ?? paths ? ????)
set SQLCMD="C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"
if not exist %SQLCMD% set SQLCMD="C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SQLCMD.EXE"
if not exist %SQLCMD% set SQLCMD="C:\Program Files\Microsoft SQL Server\140\Tools\Binn\SQLCMD.EXE"
if not exist %SQLCMD% set SQLCMD="C:\Program Files\Microsoft SQL Server\130\Tools\Binn\SQLCMD.EXE"
if not exist %SQLCMD% set SQLCMD="sqlcmd.exe"

REM Script ?????
%SQLCMD% -S %SERVER_NAME% -d %DATABASE_NAME% -U %USERNAME% -P %PASSWORD% -i "Complete_Index_Optimization.sql" -o "Optimization_Log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
  echo =============================================
    echo ??????? ??????? ??????!
    echo =============================================
    echo.
    echo Log file: Optimization_Log.txt
    echo.
    echo ??????? ???????:
    echo 1. IIS Application Pool Restart ????
    echo 2. Browser Cache Clear ????
    echo 3. Application Test ????
    echo.
) else (
  echo.
  echo =============================================
    echo Error ??????!
    echo =============================================
    echo.
    echo ???? ???:
    echo 1. Server name ????? ????
    echo 2. Database name ????? ????
    echo 3. Username/Password ????? ????
    echo 4. SQL Server ???? ??? ???? ?????
  echo.
    echo ???? SSMS ???? manually ?????
    echo.
)

pause
