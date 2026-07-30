$ErrorActionPreference = 'Stop'
$json = (New-Object Net.WebClient).DownloadString('https://api.sikkhaloy.com/api/Users/1012/schedule')
$out = Join-Path $PSScriptRoot 'schedule-sample.json'
[System.IO.File]::WriteAllText($out, $json, [Text.UTF8Encoding]::new($false))
Write-Host "Saved $($json.Length) bytes to $out"
$a = $json | ConvertFrom-Json
Write-Host "Count=$($a.Count)"
foreach ($row in $a) {
    foreach ($p in @('scheduleDayID','scheduleID','schoolID','startTime','lateEntryTime','endTime','is_OnDay','isOnDay','day','scheduleName')) {
        $v = $row.$p
        if ($null -eq $v) { continue }
        $t = $v.GetType().FullName
        if ($t -ne 'System.String' -and $t -ne 'System.Boolean' -and $t -ne 'System.Int32' -and $t -ne 'System.Int64') {
            Write-Host "NonScalar $p scheduleID=$($row.scheduleID) day=$($row.day) type=$t value=$v"
        }
    }
}
Write-Host 'Done'
