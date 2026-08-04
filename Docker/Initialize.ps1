[CmdletBinding()]
param(
    [switch] $Force
)

$environmentPath = Join-Path $PSScriptRoot ".env"
if ((Test-Path -LiteralPath $environmentPath) -and -not $Force) {
    throw "'$environmentPath' already exists. Use -Force only when you intend to replace its local secrets."
}

function New-HexSecret {
    param(
        [Parameter(Mandatory)]
        [int] $ByteCount
    )

    $bytes = New-Object byte[] $ByteCount
    $random = [Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $random.GetBytes($bytes)
    }
    finally {
        $random.Dispose()
    }

    return ([BitConverter]::ToString($bytes) -replace '-', '').ToLowerInvariant()
}

$decryptionKey = New-HexSecret -ByteCount 24

$content = @"
# Supply endpoints reachable from inside Docker containers.
# SQL on the host is normally Server=host.docker.internal,1433.
CCODER_CORE_CONNECTION_STRING=
CCODER_SECURITY_CONNECTION_STRING=
CCODER_DECRYPTION_KEY=$decryptionKey
CCODER_WEB_HTTP_PORT=80
CCODER_WEB_HTTPS_PORT=443
CCODER_WORKFLOW_HTTP_PORT=800
CCODER_WORKFLOW_HTTPS_PORT=4433
"@

[IO.File]::WriteAllText(
    $environmentPath,
    $content,
    [Text.UTF8Encoding]::new($false))

Write-Host "Created local Docker configuration at $environmentPath"
Write-Host "The Application container will generate a localhost and *.localhost certificate on first startup."
Write-Host "Set the two blank SQL connection strings in .env, then run: docker compose pull; docker compose up"
