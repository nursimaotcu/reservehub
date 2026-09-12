$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$target = Join-Path $root '.env'
if (Test-Path -LiteralPath $target) { Write-Host '.env already exists; preserved.'; exit 0 }
function New-Secret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes); return [Convert]::ToBase64String($bytes) } finally { $rng.Dispose() }
}
@("DB_PASSWORD=$(New-Secret)", "JWT_KEY=$(New-Secret)", 'ADMIN_EMAIL=admin@example.test', "ADMIN_PASSWORD=$(New-Secret)") | Set-Content -LiteralPath $target -Encoding ASCII
Write-Host 'Created .env. Local admin credentials are in this ignored file.'
