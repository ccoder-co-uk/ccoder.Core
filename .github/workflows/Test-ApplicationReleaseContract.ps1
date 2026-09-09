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
$composeFiles = @(
    "Docker/compose.yml",
    "Docker/ci/compose.yml"
)

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

$retiredPersistenceOwners = @(
    "Data",
    "AppSecurity",
    "ContentManagement",
    "DocumentManagement",
    "Logging",
    "Mail",
    "Packaging",
    "Workflow",
    "Security"
)

foreach ($composeFile in $composeFiles) {
    $composePath = Join-Path $RepositoryRoot $composeFile
    $compose = Get-Content -LiteralPath $composePath -Raw

    foreach ($requiredPersistenceOwner in @("CoreData", "SecurityData")) {
        if ($compose -notmatch "(?m)^\s+$($requiredPersistenceOwner)__ConnectionString:") {
            throw "$composeFile does not provide the required $requiredPersistenceOwner connection setting."
        }
    }

    $coreDataSettingCount =
        [regex]::Matches(
            $compose,
            '(?m)^\s+CoreData__ConnectionString:').Count

    if ($coreDataSettingCount -lt 2) {
        throw "$composeFile must provide CoreData to the application and workflow images."
    }

    foreach ($retiredPersistenceOwner in $retiredPersistenceOwners) {
        if ($compose -match "(?m)^\s+$($retiredPersistenceOwner)__ConnectionString:") {
            throw "$composeFile still provides the retired $retiredPersistenceOwner connection setting."
        }
    }
}
