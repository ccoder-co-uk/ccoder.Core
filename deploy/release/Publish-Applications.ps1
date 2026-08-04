[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $Version,

    [string] $OutputRoot,

    [string] $Commit,

    [switch] $NoBuild
)

$repositoryRoot = [IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\.."))

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repositoryRoot "artifacts\applications"
}

$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
$publishRoot = Join-Path $OutputRoot "publish"
$versionRoot = Join-Path $publishRoot $Version
$latestRoot = Join-Path $publishRoot "latest"
$archiveRoot = Join-Path $OutputRoot "archives"

# Windows PowerShell 5 does not preload the .NET Framework assembly that
# contains ZipFile, even though the type is available once explicitly loaded.
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Assert-ReleaseTarget {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $resolvedPath = [IO.Path]::GetFullPath($Path)
    $expectedPrefix = $OutputRoot.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar

    if (-not $resolvedPath.StartsWith(
        $expectedPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to replace release path outside '$OutputRoot': $resolvedPath"
    }
}

foreach ($target in @($versionRoot, $latestRoot, $archiveRoot)) {
    Assert-ReleaseTarget -Path $target

    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Recurse -Force
    }

    New-Item -ItemType Directory -Path $target -Force | Out-Null
}

$applications = [ordered]@{
    Web = "src\Apps\Web\Web.csproj"
    HostedServices = "src\Apps\HostedServices\HostedServices.csproj"
    Workflow = "src\Apps\Workflow\Workflow.csproj"
}

foreach ($application in $applications.GetEnumerator()) {
    $projectPath = Join-Path $repositoryRoot $application.Value
    $applicationOutput = Join-Path $versionRoot $application.Key

    $publishArguments = @(
        "publish",
        $projectPath,
        "--configuration", "Release",
        "--output", $applicationOutput,
        "/p:Version=$Version")

    if ($NoBuild) {
        $publishArguments += @("--no-build", "--no-restore")
    }

    & dotnet @publishArguments

    if ($LASTEXITCODE -ne 0) {
        throw "Publishing $($application.Key) failed with exit code $LASTEXITCODE."
    }
}

if ([string]::IsNullOrWhiteSpace($Commit)) {
    $gitCommand = Get-Command git -ErrorAction SilentlyContinue

    if ($null -eq $gitCommand) {
        throw "Supply -Commit when git is not available on PATH."
    }

    $Commit = git -C $repositoryRoot rev-parse HEAD

    if ($LASTEXITCODE -ne 0) {
        throw "Unable to resolve the source commit."
    }
}

$manifest = [ordered]@{
    version = $Version
    commit = $Commit.Trim()
    publishedUtc = [DateTimeOffset]::UtcNow.ToString("O")
    applications = @($applications.Keys)
}

$manifestJson = $manifest | ConvertTo-Json -Depth 3

foreach ($applicationName in $applications.Keys) {
    $applicationDirectory = Join-Path $versionRoot $applicationName
    $manifestPath = Join-Path $applicationDirectory "release-manifest.json"
    [IO.File]::WriteAllText(
        $manifestPath,
        $manifestJson,
        [Text.UTF8Encoding]::new($false))
}

Copy-Item -Path (Join-Path $versionRoot "*") -Destination $latestRoot -Recurse -Force

foreach ($applicationName in $applications.Keys) {
    $versionedArchive = Join-Path $archiveRoot (
        "cCoder.Core-{0}-{1}.zip" -f $applicationName, $Version)
    $latestArchive = Join-Path $archiveRoot (
        "cCoder.Core-{0}-latest.zip" -f $applicationName)
    $versionedApplication = Join-Path $versionRoot $applicationName
    [IO.Compression.ZipFile]::CreateFromDirectory(
        $versionedApplication,
        $versionedArchive,
        [IO.Compression.CompressionLevel]::Optimal,
        $false)

    Copy-Item -LiteralPath $versionedArchive -Destination $latestArchive
}

$checksumLines = Get-ChildItem -LiteralPath $archiveRoot -Filter "*.zip" |
    Sort-Object Name |
    ForEach-Object {
        $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
        "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
    }

Set-Content `
    -LiteralPath (Join-Path $archiveRoot "SHA256SUMS.txt") `
    -Value $checksumLines `
    -Encoding ascii

Write-Host "Published application release $Version to $versionRoot"
Write-Host "Updated latest application output at $latestRoot"
