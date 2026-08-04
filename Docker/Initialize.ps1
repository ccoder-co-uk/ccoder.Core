[CmdletBinding()]
param(
    [switch] $Force
)

$environmentPath = Join-Path $PSScriptRoot ".env"
$certificateDirectory = Join-Path $PSScriptRoot ".https"
$certificatePath = Join-Path $certificateDirectory "ccoder-localhost.pfx"

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
$certificatePassword = New-HexSecret -ByteCount 18

New-Item -ItemType Directory -Path $certificateDirectory -Force | Out-Null
$rsa = [Security.Cryptography.RSA]::Create(2048)

try {
    $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new(
        "CN=localhost",
        $rsa,
        [Security.Cryptography.HashAlgorithmName]::SHA256,
        [Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $names = [Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $names.AddDnsName("localhost")
    $names.AddDnsName("*.localhost")
    $names.AddIpAddress([Net.IPAddress]::Loopback)
    $names.AddIpAddress([Net.IPAddress]::IPv6Loopback)
    $request.CertificateExtensions.Add($names.Build())

    $serverAuthentication = [Security.Cryptography.OidCollection]::new()
    $serverAuthentication.Add([Security.Cryptography.Oid]::new("1.3.6.1.5.5.7.3.1")) | Out-Null
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new(
            $serverAuthentication,
            $true))

    $certificate = $request.CreateSelfSigned(
        [DateTimeOffset]::UtcNow.AddMinutes(-5),
        [DateTimeOffset]::UtcNow.AddYears(5))

    try {
        $bytes = $certificate.Export(
            [Security.Cryptography.X509Certificates.X509ContentType]::Pfx,
            $certificatePassword)
        [IO.File]::WriteAllBytes($certificatePath, $bytes)
    }
    finally {
        $certificate.Dispose()
    }
}
finally {
    $rsa.Dispose()
}

$content = @"
# Supply endpoints reachable from inside Docker containers.
# SQL on the host is normally Server=host.docker.internal,1433.
CCODER_CORE_CONNECTION_STRING=
CCODER_SECURITY_CONNECTION_STRING=
CCODER_AZURE_WEBJOBS_STORAGE=
CCODER_DECRYPTION_KEY=$decryptionKey
CCODER_HTTPS_PASSWORD=$certificatePassword
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
Write-Host "Created a localhost and *.localhost development certificate at $certificatePath"
Write-Host "Set the three blank connection settings in .env, then run: docker compose pull; docker compose up"
