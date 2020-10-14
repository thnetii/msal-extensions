$EnvJson = Get-Content (Join-Path $PSScriptRoot "env.json") | ConvertFrom-Json
$PublicJson = $EnvJson.public

$TfmFolder = "netcoreapp3.1"
if ($PSVersionTable.PSEdition -ine "Core") {
    $TfmFolder = "net45"
}

Import-Module -Verbose -ErrorAction "Stop" ([System.IO.Path]::Combine($PSScriptRoot, "..", "publish", "THNETII.Msal.PowerShell", "Debug", $TfmFolder, "THNETII.Msal.PowerShell.psm1"))

[Microsoft.Identity.Client.PublicClientApplicationBuilder]$Builder = New-MsalPublicClientApplicationBuilder -ClientId $PublicJson.ClientId
$Builder.WithTenantId($PublicJson.TenantId) | Out-Null
$Builder.WithRedirectUri("http://localhost") | Out-Null
$MsalApp = $Builder.Build()
$AuthFlow = $MsalApp.AcquireTokenInteractive([string[]]@())
try {
    $AuthResult = $AuthFlow.ExecuteAsync().GetAwaiter().GetResult()
    $AuthResult | Format-List *
}
catch {
    Write-Host $_.Exception
}


