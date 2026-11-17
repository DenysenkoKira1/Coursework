$ErrorActionPreference = 'Stop'
$localSdk = Join-Path $PSScriptRoot '..\.tools\dotnet\dotnet.exe'
if (Test-Path -LiteralPath $localSdk) { $dotnetPath = $localSdk }
else {
    $found = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $found) { throw 'Потрібен .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0' }
    $dotnetPath = $found.Source
}
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '..\.tools\cli-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ROOT = Split-Path -Parent $dotnetPath
& $dotnetPath @args
exit $LASTEXITCODE
