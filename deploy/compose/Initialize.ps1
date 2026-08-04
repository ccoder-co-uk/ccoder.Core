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
$certificatePassword = New-HexSecret -ByteCount 18
$certificateDirectory = Join-Path $PSScriptRoot ".https"
$certificatePath = Join-Path $certificateDirectory "ccoder-localhost.pfx"

New-Item -ItemType Directory -Path $certificateDirectory -Force | Out-Null

$rsa = [Security.Cryptography.RSA]::Create(2048)

try {
    $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new(
        "CN=localhost",
        $rsa,
        [Security.Cryptography.HashAlgorithmName]::SHA256,
        [Security.Cryptography.RSASignaturePadding]::Pkcs1)

    $subjectAlternativeNames =
        [Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $subjectAlternativeNames.AddDnsName("localhost")
    $subjectAlternativeNames.AddDnsName("*.localhost")
    $subjectAlternativeNames.AddIpAddress([Net.IPAddress]::Loopback)
    $subjectAlternativeNames.AddIpAddress([Net.IPAddress]::IPv6Loopback)
    $request.CertificateExtensions.Add($subjectAlternativeNames.Build())
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new(
            $false,
            $false,
            0,
            $true))
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
            [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor
                [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment,
            $true))

    $serverAuthentication = [Security.Cryptography.OidCollection]::new()
    $serverAuthentication.Add(
        [Security.Cryptography.Oid]::new("1.3.6.1.5.5.7.3.1")) | Out-Null
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new(
            $serverAuthentication,
            $true))

    $certificate = $request.CreateSelfSigned(
        [DateTimeOffset]::UtcNow.AddMinutes(-5),
        [DateTimeOffset]::UtcNow.AddYears(5))

    try {
        $certificateBytes = $certificate.Export(
            [Security.Cryptography.X509Certificates.X509ContentType]::Pfx,
            $certificatePassword)
        [IO.File]::WriteAllBytes($certificatePath, $certificateBytes)
    }
    finally {
        $certificate.Dispose()
    }
}
finally {
    $rsa.Dispose()
}

$content = @"
CCODER_SQL_PASSWORD=$sqlPassword
CCODER_DECRYPTION_KEY=$decryptionKey
CCODER_HTTPS_PASSWORD=$certificatePassword
CCODER_SQL_PORT=1433
CCODER_WEB_HTTP_PORT=80
CCODER_WEB_HTTPS_PORT=443
CCODER_WORKFLOW_HTTP_PORT=800
CCODER_WORKFLOW_HTTPS_PORT=4433
"@

Set-Content -LiteralPath $environmentPath -Value $content -Encoding utf8NoBOM
Write-Host "Created $environmentPath with generated development-only secrets."
Write-Host "Created wildcard development certificate $certificatePath for localhost and *.localhost."
