$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Get-Content (Join-Path $root '.env') | ForEach-Object {
    if ($_ -match '^(ADMIN_EMAIL|ADMIN_PASSWORD)=(.*)$') { [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process') }
}
python (Join-Path $root 'tests/integration.py')
if ($LASTEXITCODE -ne 0) { throw 'Integration tests failed.' }
