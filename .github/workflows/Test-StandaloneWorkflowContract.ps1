[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $RepositoryRoot,
    [string] $WorkflowPublishPath
)

$ErrorActionPreference = 'Stop'
$issues = [Collections.Generic.List[string]]::new()
function Read-RepositoryFile([string] $Path) {
    Get-Content -LiteralPath (Join-Path $RepositoryRoot $Path) -Raw
}
function Require([bool] $Condition, [string] $Message) {
    if (!$Condition) { $issues.Add($Message) }
}

$project = Read-RepositoryFile 'src/Apps/Workflow/Workflow.csproj'
Require ($project -match '<Project Sdk="Microsoft.NET.Sdk.Web"') 'Workflow must be an ASP.NET Core Web project.'
Require ($project -notmatch 'AzureFunctionsVersion|Microsoft\.Azure\.Functions') 'Workflow still references Azure Functions.'

$windowsBuild = Read-RepositoryFile '.github/workflows/publish.yml'
Require ($windowsBuild -notmatch 'azure-functions-core-tools|FUNCTIONS_CORE_TOOLS_PATH') 'Windows build still installs Azure Functions Core Tools.'

$dockerfile = Read-RepositoryFile 'Docker/build/Dockerfile.workflow'
Require ($dockerfile -notmatch '(?i)azure-functions') 'Workflow container still uses an Azure Functions base image.'
Require ($dockerfile -match 'ENTRYPOINT\s*\[\s*"dotnet"\s*,\s*"Workflow\.dll"\s*\]') 'Workflow container must start Workflow.dll directly.'

foreach ($path in 'Docker/compose.yml', 'Docker/ci/compose.yml') {
    $compose = Read-RepositoryFile $path
    $worker = [regex]::Match($compose, '(?ms)^  workflow:\s*\r?\n(?<service>.*?)(?=^  \S|^\S|\z)')
    Require $worker.Success "$path does not define the workflow service."
    Require ($worker.Groups['service'].Value -notmatch '(?i)AzureWebJobsStorage|FUNCTIONS_WORKER_RUNTIME|ConnectionString|DecryptionKey|service:sqlserver') "$path passes Functions or database configuration to Workflow."
    Require ($worker.Groups['service'].Value -match '(?m)^\s+networks:\s*\[workflow-transport\]\s*$') "$path must place Workflow only on the app-facing transport network."
    Require ($worker.Groups['service'].Value -notmatch '(?i)\.key\b|\.pfx\b|KeyPath|local-https:') "$path mounts Web TLS private material into Workflow."
    Require ($worker.Groups['service'].Value -match '(?m)^\s+Workflow__ExecutionToken:') "$path does not configure authenticated worker ingress."
    Require ($worker.Groups['service'].Value -match '(?m)^\s+Workflow__TrustedWebOrigins:') "$path does not configure approved Web origins."
    Require ($compose -match '(?m)^\s+Workflow__ServiceUrl:\s+http://workflow:800/api/\s*$') "$path does not address the standalone worker API."
    $database = [regex]::Match($compose, '(?ms)^  sqlserver:\s*\r?\n(?<service>.*?)(?=^  \S|^\S|\z)')
    Require ($database.Groups['service'].Value -notmatch 'workflow-transport') "$path must not expose the SQL container on the Workflow network."
}

if ($WorkflowPublishPath) {
    Require (Test-Path -LiteralPath (Join-Path $WorkflowPublishPath 'Workflow.dll')) 'Published worker is missing Workflow.dll.'
    Require (Test-Path -LiteralPath (Join-Path $WorkflowPublishPath 'Workflow.runtimeconfig.json')) 'Published worker is missing its .NET runtime configuration.'
    $functionsFiles = @(Get-ChildItem -LiteralPath $WorkflowPublishPath -File -Recurse |
        Where-Object Name -Match '(?i)Azure\.Functions|^host\.json$|^functions\.metadata$|^worker\.config\.json$')
    Require ($functionsFiles.Count -eq 0) 'Published worker contains Functions files.'
    $deps = Get-Content -LiteralPath (Join-Path $WorkflowPublishPath 'Workflow.deps.json') -Raw
    Require ($deps -notmatch '(?i)Microsoft\.Azure\.Functions') 'Published worker dependencies contain Azure Functions.'
}

if ($issues.Count -gt 0) {
    throw ("Standalone Workflow contract failed:`n - " + ($issues -join "`n - "))
}

'Standalone Workflow release contract passed.'
