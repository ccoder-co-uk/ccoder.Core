param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$publishWorkflowPath = Join-Path $RepositoryRoot ".github/workflows/publish.yml"
$releaseWorkflowPath = Join-Path $RepositoryRoot ".github/workflows/application-release.yml"
$publishWorkflow = Get-Content -LiteralPath $publishWorkflowPath -Raw
$releaseWorkflow = Get-Content -LiteralPath $releaseWorkflowPath -Raw

if ($publishWorkflow -notmatch [regex]::Escape('path: ${{ env.APPLICATION_DIRECTORY }}/publish/latest')) {
    throw "The applications artifact must continue to upload the contents of publish/latest."
}

$requiredReleasePaths = @(
    '${{ env.APPLICATION_DIRECTORY }}/Web/release-manifest.json',
    'context="${APPLICATION_DIRECTORY}"',
    '"${{ env.APPLICATION_DIRECTORY }}" `',
    'cp Docker/build/start-application.sh',
    '--file Docker/build/Dockerfile.application',
    '--file Docker/build/Dockerfile.workflow',
    'pwsh -File Docker/ci/Initialize.ps1',
    '--env-file Docker/ci/.env',
    '--file Docker/ci/compose.yml'
)

foreach ($requiredPath in $requiredReleasePaths) {
    if (-not $releaseWorkflow.Contains($requiredPath)) {
        throw "application-release.yml does not use the required downloaded-artifact/repository path: $requiredPath"
    }
}

$invalidReleasePaths = @(
    '${{ env.APPLICATION_DIRECTORY }}/publish/latest',
    '${APPLICATION_DIRECTORY}/publish/latest',
    'path: ccoder.Core',
    'ccoder.Core/Docker/'
)

foreach ($invalidPath in $invalidReleasePaths) {
    if ($releaseWorkflow.Contains($invalidPath)) {
        throw "application-release.yml still contains the invalid path: $invalidPath"
    }
}
