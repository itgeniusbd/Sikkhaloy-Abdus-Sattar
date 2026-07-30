$json = (New-Object Net.WebClient).DownloadString('https://api.sikkhaloy.com/api/Users/1012/schedule')
$a = $json | ConvertFrom-Json
Write-Host "Count=$($a.Count)"
foreach ($row in $a) {
    $st = $row.startTime
    $type = if ($null -eq $st) { 'null' } else { $st.GetType().FullName }
    if ($type -ne 'System.String') {
        Write-Host "NonString startTime scheduleID=$($row.scheduleID) day=$($row.day) type=$type value=$st"
    }
}
Write-Host 'Done'
