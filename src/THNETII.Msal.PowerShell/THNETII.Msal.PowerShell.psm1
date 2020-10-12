Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.Identity.Client.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "THNETII.Msal.PowerShell.dll")

[Microsoft.Identity.Client.MsalHttpFactory]`
$Script:MsalHttpClient = New-Object "Microsoft.Identity.Client.MsalHttpFactory" `
    -ArgumentList @((New-Object "System.Net.Http.HttpClient"))

[Microsoft.Identity.Client.LogCallback]$Script:MsalLogCallback = {
    param(
        [Microsoft.Identity.Client.LogLevel]$LogLevel,
        [string]$Message,
        [switch]$ContainsPii
    )

    if ($PSCmdlet) {
        switch ($LogLevel) {
            ([Microsoft.Identity.Client.LogLevel]::Error) {
                $PSCmdlet.WriteWarning($Message)
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Warning) {
                $PSCmdlet.WriteWarning($Message)
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Info) {
                Write-Host $Message
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Verbose) {
                $PSCmdlet.WriteVerbose($Message)
                break
            }
        }
    } else {
        switch ($LogLevel) {
            ([Microsoft.Identity.Client.LogLevel]::Error) {
                if ($ErrorActionPreference -ne [System.Management.Automation.ActionPreference]::SilentlyContinue) {
                    Write-Warning $Message
                }
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Warning) {
                if ($WarningPreference -ne [System.Management.Automation.ActionPreference]::SilentlyContinue) {
                    Write-Warning $Message
                }
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Info) {
                Write-Host $Message
                break
            }
            ([Microsoft.Identity.Client.LogLevel]::Verbose) {
                if ($VerbosePreference -ne [System.Management.Automation.ActionPreference]::SilentlyContinue) {
                    Write-Verbose $Message
                }
                break
            }
        }
    }
}
$Script:MsalClientName = "PowerShell $($PSVersionTable["PSEdition"])"
$Script:MsalClientVersion = $PSVersionTable["PSVersion"]

[System.Collections.Generic.IDictionary[string, Microsoft.Identity.Client.IClientApplicationBase]]`
$Script:MsalClients = New-Object "System.Collections.Generic.Dictionary[string, Microsoft.Identity.Client.IClientApplicationBase]" `
    -ArgumentList @([System.StringComparer]::OrdinalIgnoreCase)

function New-MsalPublicClientApplication {
    [CmdletBinding()]
    [OutputType([Microsoft.Identity.Client.IPublicClientApplication])]
    param (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNull()]
        [Microsoft.Identity.Client.PublicClientApplicationOptions]
        $InputObject,
        [switch]$SuppressCache
    )

    $Builder = [Microsoft.Identity.Client.PublicClientApplicationBuilder]::CreateWithApplicationOptions($InputObject)
    $Builder.WithClientName($Script:MsalClientName)
    $Builder.WithClientVersion($Script:MsalClientVersion)
    $Builder.WithHttpClientFactory([Microsoft.Identity.Client.IMsalHttpClientFactory]$Script:MsalHttpClient)
    $Builder.WithLogging($Script:MsalLogCallback)
    $App = $Builder.Build()
    if (-not $SuppressCache) {
        $Script:MsalClients[$App.AppConfig.ClientId] = $App
    }
    return $App
}

function New-MsalPublicClientOptions {
    [CmdletBinding()]
    param()
    DynamicParam {
        $Props = ([Microsoft.Identity.Client.PublicClientApplicationOptions]).GetProperties(
            [System.Reflection.BindingFlags]::Instance -bor
            [System.Reflection.BindingFlags]::Public) `
            | Where-Object -Property CanWrite
        $DynamicParams = New-Object "System.Management.Automation.RuntimeDefinedParameterDictionary"
        foreach ($PropInfo in $Props) {
            $ParameterAttribute = New-Object System.Management.Automation.ParameterAttribute
            $ParameterAttribute.Mandatory = $false
            $AttributeCollection = New-Object "System.Collections.ObjectModel.Collection[System.Attribute]"
            $AttributeCollection.Add($ParameterAttribute)
            $DynamicParam = New-Object System.Management.Automation.RuntimeDefinedParameter `
                -ArgumentList @($PropInfo.Name, $PropInfo.PropertyType, $AttributeCollection)
            $DynamicParams.Add($PropInfo.Name, $DynamicParam)
        }
        return $DynamicParams
    }
}

function New-MsalConfidentialClientApplication {
    [CmdletBinding()]
    [OutputType([Microsoft.Identity.Client.IConfidentialClientApplication])]
    param (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNull()]
        [Microsoft.Identity.Client.ConfidentialClientApplicationOptions]
        $InputObject,
        [switch]$SuppressCache
    )

    $Builder = [Microsoft.Identity.Client.ConfidentialClientApplicationBuilder]::CreateWithApplicationOptions($InputObject)
    $Builder.WithClientName($Script:MsalClientName)
    $Builder.WithClientVersion($Script:MsalClientVersion)
    $Builder.WithHttpClientFactory([Microsoft.Identity.Client.IMsalHttpClientFactory]$Script:MsalHttpClient)
    $Builder.WithLogging($Script:MsalLogCallback)
    $App = $Builder.Build()
    if (-not $SuppressCache) {
        $Script:MsalClients[$App.AppConfig.ClientId] = $App
    }
    return $App
}

function Get-MsalPublicClientApplication {
    [CmdletBinding()]
    [OutputType([Microsoft.Identity.Client.IPublicClientApplication])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]$ClientId
    )

    $App = $Script:MsalClients[$ClientId]
    $PublicApp = [Microsoft.Identity.Client.IPublicClientApplication]$App
    $PublicApp
}

function Get-MsalConfidentialClientApplication {
    [CmdletBinding()]
    [OutputType([Microsoft.Identity.Client.IConfidentialClientApplication])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]$ClientId
    )

    $App = $Script:MsalClients[$ClientId]
    $ConfidentialApp = [Microsoft.Identity.Client.IConfidentialClientApplication]$App
    $ConfidentialApp
}
