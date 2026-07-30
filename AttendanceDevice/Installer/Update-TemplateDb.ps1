$ErrorActionPreference = 'Stop'
Add-Type -Path 'F:\SIKKHALOY-V3\AttendanceDevice\bin\Release\System.Data.SQLite.dll'
$db = 'F:\SIKKHALOY-V3\AttendanceDevice\Database\SikkhaloyAppDB.template.db'
$c = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$db")
$c.Open()
function Run([string]$sql) {
    $cmd = $c.CreateCommand()
    $cmd.CommandText = $sql
    try {
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "OK: $sql"
    }
    catch {
        Write-Host "SKIP: $($_.Exception.Message)"
    }
}
Run 'ALTER TABLE Schedule_Day ADD COLUMN ScheduleName TEXT'
Run @'
CREATE TABLE IF NOT EXISTS User_Schedule (
    UserScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID INTEGER NOT NULL,
    ScheduleID INTEGER NOT NULL,
    Is_Student INTEGER NOT NULL DEFAULT 1
)
'@
$c.Close()
