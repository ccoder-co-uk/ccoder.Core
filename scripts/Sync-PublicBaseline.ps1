[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUri,

    [Parameter(Mandatory = $true)]
    [string]$Username,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [int]$AppId = 1,

    [string]$OutputRoot = "",

    [switch]$ApplyCommonObjects,

    [switch]$ApplyPackages,

    [switch]$CapturePageSources
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\public-baseline-sync"
}
$baselineRoot = Join-Path $repoRoot "src\cCoder.Core\Setup\Assets\Baseline"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runRoot = Join-Path $OutputRoot $timestamp
$snapshotRoot = Join-Path $runRoot "snapshot"
$baselineSnapshotRoot = Join-Path $runRoot "baseline"

$commonObjectFiles = @(
    @{
        Type = "Core/Resource"
        Path = Join-Path $baselineRoot "Core.Resource.json"
    },
    @{
        Type = "Core/Script"
        Path = Join-Path $baselineRoot "Core.Script.json"
    }
)

$appPackageNames = @(
    "Layouts",
    "Templates",
    "Components",
    "Pages",
    "PageRoles"
)

function Write-Status {
    param([string]$Message)
    Write-Host "==> $Message"
}

function Get-NormalizedBaseUri {
    param([string]$Uri)

    return $Uri.TrimEnd("/")
}

function Invoke-ApiJson {
    param(
        [ValidateSet("GET", "POST")]
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers,
        [object]$Body = $null
    )

    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers
    }

    $json = $Body | ConvertTo-Json -Depth 100
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers -ContentType "application/json; charset=utf-8" -Body $bytes
}

function Save-Json {
    param(
        [string]$Path,
        [object]$Value
    )

    $directory = Split-Path $Path -Parent
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 100 | Set-Content -Path $Path
}

function Save-Text {
    param(
        [string]$Path,
        [string]$Value
    )

    $directory = Split-Path $Path -Parent
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -Path $Path -Value $Value
}

function Get-AuthHeaders {
    param([string]$NormalizedBaseUri)

    Write-Status "Logging into $NormalizedBaseUri"

    $loginResponse = Invoke-ApiJson `
        -Method POST `
        -Uri "$NormalizedBaseUri/Api/Account/Login" `
        -Headers @{} `
        -Body @{
            User = $Username
            Pass = $Password
        }

    if ([string]::IsNullOrWhiteSpace($loginResponse.id)) {
        throw "Login did not return a bearer token."
    }

    return @{
        Authorization = "bearer $($loginResponse.id)"
    }
}

function Get-BaselinePackageDefinitions {
    $path = Join-Path $baselineRoot "BaselinePackages.json"
    return Get-Content -Path $path -Raw | ConvertFrom-Json
}

function Get-BaselinePackage {
    param(
        [string]$Name,
        [object[]]$Definitions,
        [string]$NormalizedBaseUri
    )

    $definition = $Definitions | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($null -eq $definition) {
        throw "Baseline package definition '$Name' was not found."
    }

    $items = foreach ($item in $definition.Items) {
        $itemPath = Join-Path $baselineRoot $item.FileName
        [ordered]@{
            id = ([Guid]::Empty).ToString()
            packageId = ([Guid]::Empty).ToString()
            type = [string]$item.Type
            data = [string](Get-Content -Path $itemPath -Raw)
            package = $null
        }
    }

    return [ordered]@{
        id = ([Guid]::Empty).ToString()
        name = [string]$definition.Name
        description = [string]$definition.Description
        category = [string]$definition.Category
        sourceApi = "$NormalizedBaseUri/Api/"
        items = @($items)
    }
}

function New-CommonObject {
    param(
        [object]$Source,
        [string]$Type
    )

    $createdOn = if ($Source.PSObject.Properties.Name -contains "CreatedOn" -and $null -ne $Source.CreatedOn) {
        $Source.CreatedOn
    } elseif ($Source.PSObject.Properties.Name -contains "LastUpdated" -and $null -ne $Source.LastUpdated) {
        $Source.LastUpdated
    } else {
        (Get-Date).ToUniversalTime().ToString("o")
    }

    $lastUpdated = if ($Source.PSObject.Properties.Name -contains "LastUpdated" -and $null -ne $Source.LastUpdated) {
        $Source.LastUpdated
    } else {
        $createdOn
    }

    $createdBy = if ($Source.PSObject.Properties.Name -contains "CreatedBy" -and -not [string]::IsNullOrWhiteSpace($Source.CreatedBy)) {
        $Source.CreatedBy
    } else {
        "setup"
    }

    $lastUpdatedBy = if ($Source.PSObject.Properties.Name -contains "LastUpdatedBy" -and -not [string]::IsNullOrWhiteSpace($Source.LastUpdatedBy)) {
        $Source.LastUpdatedBy
    } else {
        $createdBy
    }

    $culture = if ($Source.PSObject.Properties.Name -contains "Culture" -and $null -ne $Source.Culture) {
        $Source.Culture
    } else {
        ""
    }

    return [pscustomobject]@{
        id = 0
        name = $Source.Name
        description = $Source.Description
        lastUpdated = $lastUpdated
        lastUpdatedBy = $lastUpdatedBy
        createdOn = $createdOn
        createdBy = $createdBy
        version = 1
        key = $Source.Key
        type = $Type
        json = ($Source | ConvertTo-Json -Compress -Depth 100)
        culture = $culture
    }
}

function Get-BaselineCommonObjects {
    $objects = New-Object System.Collections.Generic.List[object]

    foreach ($definition in $commonObjectFiles) {
        $items = Get-Content -Path $definition.Path -Raw | ConvertFrom-Json
        foreach ($item in $items) {
            $objects.Add((New-CommonObject -Source $item -Type $definition.Type))
        }
    }

    return $objects.ToArray()
}

function Get-LiveSnapshot {
    param(
        [string]$NormalizedBaseUri,
        [hashtable]$Headers
    )

    Write-Status "Capturing production snapshot into $snapshotRoot"

    New-Item -ItemType Directory -Path $snapshotRoot -Force | Out-Null

    Write-Status "Exporting app packages"
    $packages = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/Package/Export?appId=$AppId" `
        -Headers $Headers

    Write-Status "Fetching root page"
    $rootPage = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/Page?`$filter=AppId eq $AppId and Path eq ''&`$expand=PageInfo,Contents" `
        -Headers $Headers

    Write-Status "Fetching Default layout"
    $defaultLayout = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/Layout?`$filter=AppId eq $AppId and Name eq 'Default'" `
        -Headers $Headers

    Write-Status "Fetching StaticContent layout"
    $staticLayout = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/Layout?`$filter=AppId eq $AppId and Name eq 'StaticContent'" `
        -Headers $Headers

    Write-Status "Fetching latest Core/Resource common objects"
    $resourceCommonObjects = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/CommonObject/Latest()?type=$([Uri]::EscapeDataString('Core/Resource'))" `
        -Headers $Headers

    Write-Status "Fetching latest Core/Script common objects"
    $scriptCommonObjects = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/CommonObject/Latest()?type=$([Uri]::EscapeDataString('Core/Script'))" `
        -Headers $Headers

    Write-Status "Fetching app metadata"
    $app = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/App($AppId)" `
        -Headers $Headers

    Write-Status "Saving production snapshot artifacts"
    Save-Json -Path (Join-Path $snapshotRoot "packages.export.json") -Value $packages
    Save-Json -Path (Join-Path $snapshotRoot "app.json") -Value $app
    Save-Json -Path (Join-Path $snapshotRoot "page.root.json") -Value $rootPage
    Save-Json -Path (Join-Path $snapshotRoot "layout.default.json") -Value $defaultLayout
    Save-Json -Path (Join-Path $snapshotRoot "layout.staticcontent.json") -Value $staticLayout
    Save-Json -Path (Join-Path $snapshotRoot "commonobject.resources.latest.json") -Value $resourceCommonObjects
    Save-Json -Path (Join-Path $snapshotRoot "commonobject.scripts.latest.json") -Value $scriptCommonObjects

    Write-Status "Extracting DMS asset references"
    $dmsAssetMatches = New-Object System.Collections.Generic.HashSet[string]
    $htmlPayloads = @(
        ($rootPage.value | Select-Object -First 1 | ForEach-Object { $_.Contents } | ForEach-Object { $_.Html }),
        ($defaultLayout.value | Select-Object -First 1 | ForEach-Object { $_.HeaderHtml }),
        ($defaultLayout.value | Select-Object -First 1 | ForEach-Object { $_.Html }),
        ($staticLayout.value | Select-Object -First 1 | ForEach-Object { $_.HeaderHtml }),
        ($staticLayout.value | Select-Object -First 1 | ForEach-Object { $_.Html })
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    $regex = [regex]'https?://[^"''\s>]+/Api/DMS/Content/[^"''\s>]+|/Api/DMS/Content/[^"''\s>]+'
    foreach ($html in $htmlPayloads) {
        foreach ($match in $regex.Matches($html)) {
            [void]$dmsAssetMatches.Add($match.Value)
        }
    }

    Save-Json -Path (Join-Path $snapshotRoot "dms-assets.json") -Value (@($dmsAssetMatches) | Sort-Object)
    Write-Status "Production snapshot complete"

    if ($CapturePageSources -and (Get-Command curl.exe -ErrorAction SilentlyContinue)) {
        Write-Status "Capturing live page sources"
        $pages = @(
            @{ Name = "home.html"; Uri = "$NormalizedBaseUri/" },
            @{ Name = "admin.html"; Uri = "$NormalizedBaseUri/Admin" },
            @{ Name = "documentation.html"; Uri = "$NormalizedBaseUri/Documentation" }
        )

        foreach ($page in $pages) {
            $content = & curl.exe -L -s --max-time 30 $page.Uri
            Save-Text -Path (Join-Path $snapshotRoot $page.Name) -Value $content
        }
    }
}

function Save-BaselineSnapshot {
    param(
        [object[]]$Definitions,
        [string]$NormalizedBaseUri
    )

    Write-Status "Capturing local baseline inputs into $baselineSnapshotRoot"
    New-Item -ItemType Directory -Path $baselineSnapshotRoot -Force | Out-Null

    foreach ($packageName in $appPackageNames) {
        Write-Status "Preparing baseline package snapshot for $packageName"
        $package = Get-BaselinePackage -Name $packageName -Definitions $Definitions -NormalizedBaseUri $NormalizedBaseUri
        Save-Json -Path (Join-Path $baselineSnapshotRoot "$packageName.package.json") -Value $package
    }

    Write-Status "Capturing baseline folder role source for manual/reference use"
    Save-Json -Path (Join-Path $baselineSnapshotRoot "FolderRoles.source.json") -Value (Get-Content -Path (Join-Path $baselineRoot "Core.FolderRole.json") -Raw | ConvertFrom-Json)

    Write-Status "Preparing baseline common object snapshot"
    $commonObjects = Get-BaselineCommonObjects
    Save-Json -Path (Join-Path $baselineSnapshotRoot "commonobjects.resources-and-scripts.json") -Value $commonObjects
    Write-Status "Baseline snapshot complete"
}

function Import-BaselineCommonObjects {
    param(
        [string]$NormalizedBaseUri,
        [hashtable]$Headers
    )

    Write-Status "Importing shared common objects"
    $items = Get-BaselineCommonObjects

    $result = Invoke-ApiJson `
        -Method POST `
        -Uri "$NormalizedBaseUri/Api/Core/CommonObject/Import" `
        -Headers $Headers `
        -Body @{
            value = $items
        }

    Save-Json -Path (Join-Path $runRoot "commonobjects.import.result.json") -Value $result

    $resourceLatest = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/CommonObject/Latest()?type=$([Uri]::EscapeDataString('Core/Resource'))" `
        -Headers $Headers

    $scriptLatest = Invoke-ApiJson `
        -Method GET `
        -Uri "$NormalizedBaseUri/Api/Core/CommonObject/Latest()?type=$([Uri]::EscapeDataString('Core/Script'))" `
        -Headers $Headers

    Save-Json -Path (Join-Path $runRoot "commonobjects.resources.latest.after.json") -Value $resourceLatest
    Save-Json -Path (Join-Path $runRoot "commonobjects.scripts.latest.after.json") -Value $scriptLatest
}

function Import-BaselinePackages {
    param(
        [string]$NormalizedBaseUri,
        [hashtable]$Headers,
        [object[]]$Definitions
    )

    Write-Status "Importing app-scoped baseline packages"
    Write-Status "FolderRoles are snapshot only here because the current package import surface does not support Core/FolderRole."

    $results = @()
    foreach ($packageName in $appPackageNames) {
        Write-Status "Importing package $packageName"
        $package = Get-BaselinePackage -Name $packageName -Definitions $Definitions -NormalizedBaseUri $NormalizedBaseUri
        $response = Invoke-ApiJson `
            -Method POST `
            -Uri "$NormalizedBaseUri/Api/Core/Package/Import?appId=$AppId" `
            -Headers $Headers `
            -Body $package

        $results += [pscustomobject]@{
            Name = $packageName
            Response = $response
        }
    }

    Save-Json -Path (Join-Path $runRoot "packages.import.result.json") -Value $results
}

$normalizedBaseUri = Get-NormalizedBaseUri -Uri $BaseUri
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null

$headers = Get-AuthHeaders -NormalizedBaseUri $normalizedBaseUri
$definitions = Get-BaselinePackageDefinitions

Get-LiveSnapshot -NormalizedBaseUri $normalizedBaseUri -Headers $headers
Save-BaselineSnapshot -Definitions $definitions -NormalizedBaseUri $normalizedBaseUri

if ($ApplyCommonObjects) {
    Import-BaselineCommonObjects -NormalizedBaseUri $normalizedBaseUri -Headers $headers
}

if ($ApplyPackages) {
    Import-BaselinePackages -NormalizedBaseUri $normalizedBaseUri -Headers $headers -Definitions $definitions
}

Write-Status "Completed. Artifacts written to $runRoot"
