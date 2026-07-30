# Removes legacy SikkhaloyAppDB.db from the install folder (Program Files). Requires admin.
param(
    [Parameter(Mandatory = $true)]
    [string]$InstallDir
)

$ErrorActionPreference = 'SilentlyContinue'

$targets = @(
    (Join-Path $InstallDir 'SikkhaloyAppDB.db'),
    (Join-Path $InstallDir 'Database\SikkhaloyAppDB.db')
)

foreach ($path in $targets) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
    }
}
