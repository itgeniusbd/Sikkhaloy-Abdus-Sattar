# Run local SQLite cleanup scripts for AttendanceDevice.
# Usage:
#   powershell -ExecutionPolicy Bypass -File "Run-LocalCleanup.ps1" -Script OrphanSchedule
#   powershell -ExecutionPolicy Bypass -File "Run-LocalCleanup.ps1" -Script Date -AttendanceDate "17-Jul-26"
param(
    [ValidateSet('OrphanSchedule', 'Today', 'MarkSyncedToday', 'Date')]
    [string]$Script = 'OrphanSchedule',
    [string]$AttendanceDate = ''
)

$ErrorActionPreference = 'Stop'

$dbPath = Join-Path $env:LOCALAPPDATA 'SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db'
if (-not (Test-Path -LiteralPath $dbPath)) {
    $legacy = Get-ChildItem (Join-Path $env:LOCALAPPDATA 'Apps\2.0') -Filter 'SikkhaloyAppDB.db' -File -Recurse -Depth 12 -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if ($legacy) { $dbPath = $legacy }
}

if (-not (Test-Path -LiteralPath $dbPath)) {
    throw "SikkhaloyAppDB.db not found. Open AttendanceDevice once, login, then try again."
}

if (Get-Process -Name 'AttendanceDevice' -ErrorAction SilentlyContinue) {
    throw "Close AttendanceDevice first, then run this script again."
}

if ($Script -eq 'OrphanSchedule') {
    $sqlFile = Join-Path $PSScriptRoot 'Cleanup_OrphanSchedule_Attendance.sql'
} elseif ($Script -eq 'Today') {
    $sqlFile = Join-Path $PSScriptRoot 'Cleanup_Today_Local_Attendance.sql'
} elseif ($Script -eq 'Date') {
    $sqlFile = $null
} else {
    $sqlFile = $null
}

if ($Script -eq 'Date' -and [string]::IsNullOrWhiteSpace($AttendanceDate)) {
    throw 'Use -AttendanceDate "17-Jul-26" with -Script Date'
}

if ($Script -notin @('MarkSyncedToday', 'Date') -and -not (Test-Path -LiteralPath $sqlFile)) {
    throw "SQL file not found: $sqlFile"
}

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$dllCandidates = @(
    (Join-Path $repoRoot 'AttendanceDevice\bin\Release\System.Data.SQLite.dll'),
    (Join-Path $env:ProgramFiles 'SIKKHALOY\AttendanceDevice\System.Data.SQLite.dll')
)
$dllPath = $null
foreach ($candidate in $dllCandidates) {
    if (Test-Path -LiteralPath $candidate) {
        $dllPath = $candidate
        break
    }
}
if (-not $dllPath) {
    throw "System.Data.SQLite.dll not found. Build AttendanceDevice Release or install the app first."
}

Add-Type -Path $dllPath

function Invoke-SqliteBatch([string]$Path, [string]$Database) {
    $text = Get-Content -LiteralPath $Path -Raw
    $parts = [regex]::Split($text, '(?m)^\s*;\s*$')
    $conn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$Database;Version=3;")
    try {
        $conn.Open()
        foreach ($part in $parts) {
            $sql = ($part -replace '(?s)/\*.*?\*/', '' -replace '(?m)--.*$', '').Trim()
            if ([string]::IsNullOrWhiteSpace($sql)) { continue }
            $cmd = $conn.CreateCommand()
            try {
                $cmd.CommandText = $sql
                $reader = $cmd.ExecuteReader()
                if ($reader -and $reader.HasRows) {
                    $table = New-Object System.Data.DataTable
                    $table.Load($reader)
                    $table | Format-Table -AutoSize
                }
                if ($reader) { $reader.Close() }
            } finally {
                if ($cmd) { $cmd.Dispose() }
            }
        }
    } finally {
        if ($conn) { $conn.Close(); $conn.Dispose() }
    }
}

Write-Host "Database: $dbPath" -ForegroundColor Cyan

if ($Script -eq 'MarkSyncedToday') {
    Write-Host "Marking today's rows as synced (Is_Sent=1, Is_Updated=1)..." -ForegroundColor Cyan
    $conn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath;Version=3;")
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = @"
UPDATE Attendance_Record
SET Is_Sent = 1, Is_Updated = 1
WHERE AttendanceDate = strftime('%d-%b-%y', 'now', 'localtime');
SELECT changes() AS RowsUpdated;
"@
        $reader = $cmd.ExecuteReader()
        if ($reader -and $reader.HasRows) {
            $table = New-Object System.Data.DataTable
            $table.Load($reader)
            $table | Format-Table -AutoSize
        }
        if ($reader) { $reader.Close() }
    } finally {
        if ($conn) { $conn.Close(); $conn.Dispose() }
    }
    Write-Host "Done." -ForegroundColor Green
    exit 0
}

if ($Script -eq 'Date') {
    $dateText = $AttendanceDate.Trim()
    Write-Host "Deleting local attendance for: $dateText" -ForegroundColor Cyan
    $conn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath;Version=3;")
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = @"
DELETE FROM Attendance_Record WHERE AttendanceDate = @d;
DELETE FROM Attendance_Log_Backup WHERE Entry_Date = @d;
SELECT 'Attendance_Record left' AS Info, COUNT(*) AS Cnt FROM Attendance_Record;
"@
        $null = $cmd.Parameters.AddWithValue('@d', $dateText)
        $reader = $cmd.ExecuteReader()
        do {
            if ($reader -and $reader.HasRows) {
                $table = New-Object System.Data.DataTable
                $table.Load($reader)
                $table | Format-Table -AutoSize
            }
        } while ($reader.NextResult())
        if ($reader) { $reader.Close() }
    } finally {
        if ($conn) { $conn.Close(); $conn.Dispose() }
    }
    Write-Host "Done." -ForegroundColor Green
    exit 0
}

Write-Host "Running:  $sqlFile" -ForegroundColor Cyan
Invoke-SqliteBatch -Path $sqlFile -Database $dbPath
Write-Host "Done." -ForegroundColor Green
