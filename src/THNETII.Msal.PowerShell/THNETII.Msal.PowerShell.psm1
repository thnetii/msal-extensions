Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.Identity.Client.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "THNETII.Msal.PowerShell.dll")

[Microsoft.Identity.Client.MsalHttpClient]`
$Script:MsalHttpClient = New-Object "Microsoft.Identity.Client.MsalHttpClient" `
    -ArgumentList @((New-Object "System.Net.Http.HttpClient"))

$Script:MsalClientName = "PowerShell $($PSVersionTable["PSEdition"])"
$Script:MsalClientVersion = $PSVersionTable["PSVersion"]

function New-MsalPublicClientApplicationBuilder {
    [CmdletBinding(DefaultParameterSetName="ByClientId")]
    [OutputType([Microsoft.Identity.Client.PublicClientApplicationBuilder])]
    param (
        [Parameter(ParameterSetName="ByClientId", Mandatory=$true, Position=0)]
        [Parameter(ParameterSetName="ByOptions", Mandatory=$false, Position=1)]
        [string]$ClientId,
        [Parameter(ParameterSetName="ByOptions", Mandatory=$true, ValueFromPipeline=$true)]
        [Microsoft.Identity.Client.PublicClientApplicationOptions]$Options
    )

    [Microsoft.Identity.Client.PublicClientApplicationBuilder]$Builder = $null
    if ($Options) {
        $Builder = [Microsoft.Identity.Client.PublicClientApplicationBuilder]::CreateWithApplicationOptions($Options)
        if ($ClientId) {
            $Builder.WithClientId($ClientId) | Out-Null
        }
    } else {
        $Builder = [Microsoft.Identity.Client.PublicClientApplicationBuilder]::Create($ClientId)
    }

    $Builder.WithClientName($Script:MsalClientName) | Out-Null
    $Builder.WithClientVersion($Script:MsalClientVersion) | Out-Null
    $Builder.WithHttpClientFactory($Script:MsalHttpClient) | Out-Null

    return $Builder
}

function New-MsalConfidentialClientApplicationBuilder {
    [CmdletBinding(DefaultParameterSetName="ByClientId")]
    [OutputType([Microsoft.Identity.Client.ConfidentialClientApplicationBuilder])]
    param (
        [Parameter(ParameterSetName="ByClientId", Mandatory=$true, Position=0)]
        [Parameter(ParameterSetName="ByOptions", Mandatory=$false, Position=1)]
        [ValidateNotNullOrEmpty()]
        [string]$ClientId,
        [Parameter(ParameterSetName="ByOptions", Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        [ValidateNotNull()]
        [Microsoft.Identity.Client.ConfidentialClientApplicationOptions]$Options
    )

    [Microsoft.Identity.Client.ConfidentialClientApplicationBuilder]$Builder = $null
    if ($Options) {
        $Builder = [Microsoft.Identity.Client.ConfidentialClientApplicationBuilder]::CreateWithApplicationOptions($Options)
        if ($ClientId) {
            $Builder.WithClientId($ClientId) | Out-Null
        }
    } else {
        $Builder = [Microsoft.Identity.Client.ConfidentialClientApplicationBuilder]::Create($ClientId)
    }

    $Builder.WithClientName($Script:MsalClientName) | Out-Null
    $Builder.WithClientVersion($Script:MsalClientVersion) | Out-Null
    $Builder.WithHttpClientFactory($Script:MsalHttpClient) | Out-Null

    return $Builder
}
