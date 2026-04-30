[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUri,

    [Parameter(Mandatory = $true)]
    [string]$Username,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [int]$AppId = 1,

    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\public-baseline-validation"
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runRoot = Join-Path $OutputRoot $timestamp
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null

$report = [ordered]@{
    baseUri = $BaseUri
    appId = $AppId
    generatedOn = (Get-Date).ToString("o")
    checks = New-Object System.Collections.Generic.List[object]
}

function Write-Status {
    param([string]$Message)
    Write-Host "==> $Message"
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

function Add-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Details,
        [object]$Data = $null
    )

    $item = [pscustomobject]@{
        Name = $Name
        Passed = $Passed
        Details = $Details
        Data = $Data
    }

    $report.checks.Add($item)

    if ($Passed) {
        Write-Host "PASS: $Name - $Details"
    } else {
        Write-Host "FAIL: $Name - $Details" -ForegroundColor Red
    }
}

function Get-FirstValue {
    param([object]$Response)

    if ($null -eq $Response) {
        return $null
    }

    if ($Response.PSObject.Properties.Name -contains "value") {
        return @($Response.value) | Select-Object -First 1
    }

    return $Response
}

function Get-Values {
    param([object]$Response)

    if ($null -eq $Response) {
        return @()
    }

    if ($Response.PSObject.Properties.Name -contains "value") {
        return @($Response.value)
    }

    return @($Response)
}

function Test-PublicPage {
    param(
        [string]$Name,
        [string]$Url,
        [string[]]$RequiredText = @(),
        [string[]]$ForbiddenText = @()
    )

    $response = Invoke-WebRequest -Uri $Url -UseBasicParsing
    $content = [string]$response.Content
    $result = [ordered]@{
        StatusCode = $response.StatusCode
        Url = $Url
        RequiredMatches = @{}
        ForbiddenMatches = @{}
    }

    $passed = $response.StatusCode -eq 200

    foreach ($text in $RequiredText) {
        $matched = $content.Contains($text)
        $result.RequiredMatches[$text] = $matched
        $passed = $passed -and $matched
    }

    foreach ($text in $ForbiddenText) {
        $matched = $content.Contains($text)
        $result.ForbiddenMatches[$text] = $matched
        $passed = $passed -and (-not $matched)
    }

    Add-Check -Name $Name -Passed $passed -Details "HTTP $($response.StatusCode)" -Data $result

    return $content
}

$normalizedBaseUri = Get-NormalizedBaseUri -Uri $BaseUri
$headers = Get-AuthHeaders -NormalizedBaseUri $normalizedBaseUri

Write-Status "Checking public routes"
$homeHtml = Test-PublicPage `
    -Name "Homepage renders refreshed baseline" `
    -Url "$normalizedBaseUri/" `
    -RequiredText @(
        'data-Layout="PublicSite"',
        'CompanyLogoTransparent',
        'everything.min.js',
        'Open Platform',
        'Notify me',
        'mailto:info@ccoder.co.uk?subject=Notify%20me%20about%20cCoder',
        'mailto:info@ccoder.co.uk?subject=Project%20enquiry%20for%20cCoder'
    ) `
    -ForbiddenText @(
        'OriginalLogoSymbolOnly.png',
        'why-us-1.jpg',
        'why-us-2.jpg',
        'why-us-3.jpg'
    )

[void](Test-PublicPage -Name "Admin route reachable" -Url "$normalizedBaseUri/Admin")
[void](Test-PublicPage -Name "Documentation route reachable" -Url "$normalizedBaseUri/Documentation")

Write-Status "Checking live CMS data"
$rootPageResponse = Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/Page?`$filter=AppId eq $AppId and Path eq ''&`$expand=PageInfo,Contents" `
    -Headers $headers
$rootPage = Get-FirstValue -Response $rootPageResponse

$rootBody = @($rootPage.Contents | Where-Object { $_.Name -eq "body" }) | Select-Object -First 1
$rootTitle = @($rootPage.PageInfo | Where-Object { [string]::IsNullOrWhiteSpace($_.CultureId) }) | Select-Object -First 1
Add-Check `
    -Name "Root page uses PublicSite layout" `
    -Passed ($null -ne $rootPage -and $rootPage.Layout -eq "PublicSite") `
    -Details "Layout = $($rootPage.Layout)" `
    -Data $rootPage
Add-Check `
    -Name "Root page title updated" `
    -Passed ($null -ne $rootTitle -and $rootTitle.Title -eq "cCoder | Standards-first bespoke cloud software") `
    -Details "Title = $($rootTitle.Title)" `
    -Data $rootTitle
Add-Check `
    -Name "Root page body keeps working CTA replacements" `
    -Passed ($null -ne $rootBody -and $rootBody.Html.Contains("mailto:info@ccoder.co.uk") -and $rootBody.Html.Contains("Open notify email") -and $rootBody.Html.Contains("Open contact email")) `
    -Details "Homepage body contains mailto CTA links" `
    -Data $rootBody

$publicLayoutResponse = Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/Layout?`$filter=AppId eq $AppId and Name eq 'PublicSite'" `
    -Headers $headers
$publicLayout = Get-FirstValue -Response $publicLayoutResponse
Add-Check `
    -Name "PublicSite layout exists" `
    -Passed ($null -ne $publicLayout) `
    -Details "Layout found for app $AppId" `
    -Data $publicLayout

$adminPageResponse = Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/Page?`$filter=AppId eq $AppId and Path eq 'Admin'" `
    -Headers $headers
$adminPage = Get-FirstValue -Response $adminPageResponse
Add-Check `
    -Name "Admin landing page still exists" `
    -Passed ($null -ne $adminPage -and $adminPage.Layout -eq "Default") `
    -Details "Admin layout = $($adminPage.Layout)" `
    -Data $adminPage

$appManagementPageResponse = Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/Page?`$filter=AppId eq $AppId and Path eq 'Admin/AppManagement'&`$expand=Contents" `
    -Headers $headers
$appManagementPage = Get-FirstValue -Response $appManagementPageResponse
$appManagementBody = @($appManagementPage.Contents | Where-Object { $_.Name -eq "body" }) | Select-Object -First 1
Add-Check `
    -Name "Admin AppManagement page still references appmanagement component" `
    -Passed ($null -ne $appManagementBody -and $appManagementBody.Html.Contains("[component[appmanagement]]")) `
    -Details "Admin/AppManagement body still references appmanagement" `
    -Data $appManagementBody

Write-Status "Checking key CMS components"
foreach ($componentName in @("AppManagement", "LayoutManagement", "ComponentManagement", "ResourceManagement")) {
    $componentResponse = Invoke-ApiJson `
        -Method GET `
        -Uri "$normalizedBaseUri/Api/Core/Component?`$filter=AppId eq $AppId and Name eq '$componentName'" `
        -Headers $headers

    $component = Get-FirstValue -Response $componentResponse
    Add-Check `
        -Name "Component '$componentName' exists" `
        -Passed ($null -ne $component) `
        -Details "Component lookup for $componentName" `
        -Data $component
}

Write-Status "Checking common cache objects"
$scriptObjects = Get-Values -Response (Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/CommonObject/Latest()?type=Core/Script" `
    -Headers $headers)
$resourceObjects = Get-Values -Response (Invoke-ApiJson `
    -Method GET `
    -Uri "$normalizedBaseUri/Api/Core/CommonObject/Latest()?type=Core/Resource" `
    -Headers $headers)

Add-Check `
    -Name "Common cache scripts exist" `
    -Passed ($scriptObjects.Count -gt 0) `
    -Details "$($scriptObjects.Count) script common objects returned" `
    -Data @{ Count = $scriptObjects.Count }
Add-Check `
    -Name "Common cache resources exist" `
    -Passed ($resourceObjects.Count -gt 0) `
    -Details "$($resourceObjects.Count) resource common objects returned" `
    -Data @{ Count = $resourceObjects.Count }

$defaultResourcing = @($scriptObjects | Where-Object { $_.Name -eq "DefaultResourcing" }) | Select-Object -First 1
$kendoCultures = @($scriptObjects | Where-Object { $_.Name -eq "KendoCultures" }) | Select-Object -First 1

Add-Check `
    -Name "DefaultResourcing script present in common cache" `
    -Passed ($null -ne $defaultResourcing) `
    -Details "DefaultResourcing lookup" `
    -Data $defaultResourcing
Add-Check `
    -Name "KendoCultures script present in common cache" `
    -Passed ($null -ne $kendoCultures) `
    -Details "KendoCultures lookup" `
    -Data $kendoCultures

$failureCount = @($report.checks | Where-Object { -not $_.Passed }).Count
$report.summary = [ordered]@{
    TotalChecks = $report.checks.Count
    FailedChecks = $failureCount
    PassedChecks = $report.checks.Count - $failureCount
}

$reportPath = Join-Path $runRoot "validation-report.json"
Save-Json -Path $reportPath -Value $report

Write-Status "Validation report saved to $reportPath"

if ($failureCount -gt 0) {
    throw "$failureCount validation checks failed."
}

Write-Host "Validation completed successfully with $($report.summary.PassedChecks) passing checks."
