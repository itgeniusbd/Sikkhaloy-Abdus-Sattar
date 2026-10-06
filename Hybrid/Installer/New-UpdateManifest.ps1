# Writes SIKKHALOY_Hybrid_Update.json (version, file name, size, SHA-256) next to a Hybrid setup exe.
# Upload BOTH files to SIKKHALOY V2\Home\App_For_Download\ on the server (exe first, json last).
# The desktop app's auto-updater reads them through https://sikkhaloy.com/Handeler/Desktop_App_Update.ashx
# Usage: .\New-UpdateManifest.ps1 -Setup .\Output\SIKKHALOY_Hybrid_Setup_1.0.9.exe [-Notes "text"]
param(
    [Parameter(Mandatory = $true)][string]$Setup,
    [string]$Version,
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"
$file = Get-Item -LiteralPath $Setup
if (-not $Version) {
    if ($file.Name -match '^SIKKHALOY_Hybrid_Setup_(\d+(?:\.\d+){1,3})\.exe$') { $Version = $Matches[1] }
    else { throw "Cannot read the version from '$($file.Name)'. Pass -Version." }
}

$sha = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$manifest = [ordered]@{
    version  = $Version
    fileName = $file.Name
    fileSize = $file.Length
    sha256   = $sha
    notes    = $Notes
    builtUtc = (Get-Date).ToUniversalTime().ToString("o")
}
$out = Join-Path $file.DirectoryName "SIKKHALOY_Hybrid_Update.json"
[System.IO.File]::WriteAllText($out, ($manifest | ConvertTo-Json) + [Environment]::NewLine, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "Update manifest written:" -ForegroundColor Green
Write-Host "  $out"
Write-Host "  version $Version, $($file.Length) bytes, SHA-256 $sha"
$out
