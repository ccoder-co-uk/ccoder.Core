param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string] $SettingsPath,

    [string] $ResultsDirectory,

    [switch] $CollectCoverage
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$testProjects = @(
    @{ Name = "unit"; Path = "src/cCoder.Core.Tests/cCoder.Core.Tests.csproj" },
    @{ Name = "integration"; Path = "src/Apps/cCoder.IntegrationTests/cCoder.IntegrationTests.csproj" },
    @{ Name = "hosted-acceptance"; Path = "src/Apps/HostedServices.AcceptanceTests/HostedServices.AcceptanceTests.csproj" },
    @{ Name = "web-acceptance"; Path = "src/Apps/Web.AcceptanceTests/Web.AcceptanceTests.csproj" }
)

$temporaryRunRoot = [string]::IsNullOrWhiteSpace($ResultsDirectory)
$runRoot = if ($temporaryRunRoot) {
    Join-Path ([System.IO.Path]::GetTempPath()) "ccoder-core-tests-$([Guid]::NewGuid().ToString('N'))"
} else {
    $ResultsDirectory
}

New-Item -ItemType Directory -Path $runRoot -Force | Out-Null

function Quote-Argument([string] $Value) {
    return '"' + $Value.Replace('"', '\"') + '"'
}

$executions = foreach ($testProject in $testProjects) {
    $projectResults = Join-Path $runRoot $testProject.Name
    New-Item -ItemType Directory -Path $projectResults -Force | Out-Null

    $arguments = @(
        "test",
        (Join-Path $RepositoryRoot $testProject.Path),
        "-c", "Release",
        "--no-build",
        "--no-restore",
        "--settings", $SettingsPath,
        "--filter", "Category!=ExternalIntegration",
        "--results-directory", $projectResults,
        "--logger", "trx;LogFileName=$($testProject.Name).trx"
    )

    if ($CollectCoverage) {
        $arguments += '--collect:XPlat Code Coverage'
    }

    $argumentLine = ($arguments | ForEach-Object { Quote-Argument $_ }) -join " "

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = "dotnet"
    $startInfo.Arguments = $argumentLine
    $startInfo.WorkingDirectory = $RepositoryRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo

    Write-Host "Starting $($testProject.Name) tests."
    if (-not $process.Start()) {
        throw "Could not start $($testProject.Name) tests."
    }

    [pscustomobject]@{
        Name = $testProject.Name
        Process = $process
        StandardOutput = $process.StandardOutput.ReadToEndAsync()
        StandardError = $process.StandardError.ReadToEndAsync()
        ResultsDirectory = $projectResults
    }
}

$failed = $false

foreach ($execution in $executions) {
    $execution.Process.WaitForExit()
    $exitCode = $execution.Process.ExitCode
    $standardOutput = $execution.StandardOutput.GetAwaiter().GetResult()
    $standardError = $execution.StandardError.GetAwaiter().GetResult()

    [System.IO.File]::WriteAllText(
        (Join-Path $execution.ResultsDirectory "stdout.log"),
        $standardOutput)

    [System.IO.File]::WriteAllText(
        (Join-Path $execution.ResultsDirectory "stderr.log"),
        $standardError)

    Write-Host "`n===== $($execution.Name) tests ====="
    Write-Host $standardOutput

    if (-not [string]::IsNullOrWhiteSpace($standardError)) {
        Write-Host $standardError
    }

    if ($exitCode -ne 0) {
        Write-Error "$($execution.Name) tests exited with code $exitCode." -ErrorAction Continue
        $failed = $true
    }
}

if ($failed) {
    exit 1
}

if ($temporaryRunRoot) {
    Remove-Item -LiteralPath $runRoot -Recurse -Force
}
