$EnvJson = Get-Content (Join-Path $PSScriptRoot "env.json") | ConvertFrom-Json
$PublicJson = $EnvJson["public"]

$TfmFolder = "netcoreapp3.1"
if ($PSVersionTable.PSEdition -ine "Core") {
    $TfmFolder = "net45"
}
Import-Module -Verbose -ErrorAction "Stop" ([System.IO.Path]::Combine($PSScriptRoot, "..", "publish", "THNETII.Msal.PowerShell", "Debug", $TfmFolder, "THNETII.Msal.PowerShell.psm1"))

Get-Command New-MsalPublicClientOptions | fl *
