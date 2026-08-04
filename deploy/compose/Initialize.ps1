[CmdletBinding()]
param(
    [switch] $Force
)

$environmentPath = Join-Path $PSScriptRoot ".env"

if ((Test-Path -LiteralPath $environmentPath) -and -not $Force) {
    throw "The Compose environment already exists at '$environmentPath'. Use -Force to replace it."
}

function New-HexSecret {
    param(
        [Parameter(Mandatory)]
        [int] $ByteCount
    )

    $bytes = [byte[]]::new($ByteCount)
    [Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToHexString($bytes).ToLowerInvariant()
}

$sqlPassword = "Cc!$(New-HexSecret -ByteCount 18)"
$decryptionKey = New-HexSecret -ByteCount 24

$content = @"
CCODER_SQL_PASSWORD=$sqlPassword
CCODER_DECRYPTION_KEY=$decryptionKey
CCODER_SQL_PORT=1433
CCODER_WEB_PORT=5099
CCODER_HOSTED_SERVICES_PORT=5100
CCODER_WORKFLOW_PORT=7071
"@

Set-Content -LiteralPath $environmentPath -Value $content -Encoding utf8NoBOM
Write-Host "Created $environmentPath with generated development-only secrets."
