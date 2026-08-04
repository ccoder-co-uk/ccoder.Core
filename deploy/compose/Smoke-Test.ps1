[CmdletBinding()]
param(
    [int] $WebPort = 80,
    [int] $Attempts = 30,
    [int] $DelaySeconds = 2
)

function Wait-ForHealth {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [uri] $Uri
    )

    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 5

            if ($response.StatusCode -eq 200 -and $response.Content.Trim() -eq "OK") {
                Write-Host "$Name is healthy at $Uri"
                return
            }
        }
        catch {
            if ($attempt -eq $Attempts) {
                throw "$Name did not become healthy at $Uri. $($_.Exception.Message)"
            }
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    throw "$Name did not become healthy at $Uri."
}

Wait-ForHealth -Name "Web" -Uri "http://localhost:$WebPort/Health"
