Add-Type -Path 'F:\SIKKHALOY-V3\AttendanceDevice\bin\Release\Newtonsoft.Json.dll'
$json = [IO.File]::ReadAllText('F:\SIKKHALOY-V3\AttendanceDevice\Installer\schedule-sample.json')
$array = [Newtonsoft.Json.Linq.JArray]::Parse($json)
$ok = 0; $fail = 0
foreach ($item in $array) {
  try {
    $st = $item['startTime']
    if ($null -eq $st) { throw 'no startTime' }
    $text = $st.ToString()
    $null = [TimeSpan]::Parse($text)
    $ok++
  } catch {
    $fail++
    Write-Host "Fail scheduleID=$($item['scheduleID']) $($_.Exception.Message)"
  }
}
Write-Host "ok=$ok fail=$fail"
