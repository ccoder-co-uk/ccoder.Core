param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$expectedReferences = [ordered]@{
    "cCoder.AI" = "2026.9.9.920"
    "cCoder.AppSecurity" = "2026.9.9.859"
    "cCoder.ContentManagement" = "2026.9.10.1713"
    "cCoder.Data" = "2026.9.9.951"
    "cCoder.DocumentManagement" = "2026.9.10.1724"
    "cCoder.Eventing" = "2026.9.9.847"
    "cCoder.Eventing.AzureServiceBus" = "2026.9.9.847"
    "cCoder.Eventing.Http" = "2026.9.9.847"
    "cCoder.Logging" = "2026.9.9.1004"
    "cCoder.Mail" = "2026.9.9.833"
    "cCoder.Packaging" = "2026.9.9.848"
    "cCoder.Security" = "2026.9.9.902"
    "cCoder.Security.Data" = "2026.9.9.902"
    "cCoder.Workflow" = "2026.9.9.835"
    "cCoder.Workflow.Activities" = "2026.9.9.835"
    "cCoder.Workflow.Engine" = "2026.9.9.835"
}

$projectPath = Join-Path $RepositoryRoot "src/cCoder.Core/cCoder.Core.csproj"
[xml] $project = Get-Content -LiteralPath $projectPath -Raw
$actualReferences = @{}

foreach ($reference in $project.SelectNodes("//PackageReference")) {
    $packageId = $reference.GetAttribute("Include")

    if ($expectedReferences.Contains($packageId)) {
        $actualReferences[$packageId] = $reference.GetAttribute("Version")
    }
}

$missingReferences = @(
    $expectedReferences.Keys |
        Where-Object { -not $actualReferences.ContainsKey($_) }
)

if ($missingReferences.Count -gt 0) {
    throw "Missing direct cCoder domain package references: $($missingReferences -join ', ')."
}

$incorrectReferences = @(
    foreach ($packageId in $expectedReferences.Keys) {
        if ($actualReferences[$packageId] -ne $expectedReferences[$packageId]) {
            "$packageId=$($actualReferences[$packageId]) (expected $($expectedReferences[$packageId]))"
        }
    }
)

if ($incorrectReferences.Count -gt 0) {
    throw "Direct cCoder domain package references are out of date: $($incorrectReferences -join '; ')."
}

$expectedConsumerReferences = @(
    @{ Project = "src/cCoder.Core.Tests/cCoder.Core.Tests.csproj"; Package = "cCoder.AppSecurity"; Version = "2026.9.9.859" },
    @{ Project = "src/cCoder.Core.Tests/cCoder.Core.Tests.csproj"; Package = "cCoder.ContentManagement"; Version = "2026.9.10.1713" },
    @{ Project = "src/cCoder.Core.Tests/cCoder.Core.Tests.csproj"; Package = "cCoder.Data"; Version = "2026.9.9.951" },
    @{ Project = "src/Apps/HostedServices.AcceptanceTests/HostedServices.AcceptanceTests.csproj"; Package = "cCoder.Security.Data"; Version = "2026.9.9.902" },
    @{ Project = "src/Apps/Web.AcceptanceTests/Web.AcceptanceTests.csproj"; Package = "cCoder.Security.Data"; Version = "2026.9.9.902" },
    @{ Project = "src/Apps/cCoder.IntegrationTests/cCoder.IntegrationTests.csproj"; Package = "cCoder.Security.Data"; Version = "2026.9.9.902" },
    @{ Project = "src/Apps/cCoder.IntegrationTests/cCoder.IntegrationTests.csproj"; Package = "cCoder.Mail.Providers"; Version = "2026.9.9.833" }
)

$consumerDowngrades = @(
    foreach ($expectedReference in $expectedConsumerReferences) {
        $consumerProjectPath =
            Join-Path $RepositoryRoot $expectedReference.Project
        [xml] $consumerProject =
            Get-Content -LiteralPath $consumerProjectPath -Raw
        $packageReference = $consumerProject.SelectSingleNode(
            "//PackageReference[@Include='$($expectedReference.Package)']")

        if ($null -eq $packageReference) {
            "$($expectedReference.Project): missing $($expectedReference.Package)"
        }
        elseif ($packageReference.GetAttribute("Version") -ne $expectedReference.Version) {
            "$($expectedReference.Project): $($expectedReference.Package)=$($packageReference.GetAttribute('Version')) (expected $($expectedReference.Version))"
        }
    }
)

if ($consumerDowngrades.Count -gt 0) {
    throw "Direct consumer package downgrades remain: $($consumerDowngrades -join '; ')."
}
