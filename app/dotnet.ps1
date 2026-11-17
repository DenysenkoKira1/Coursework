$ErrorActionPreference = 'Stop'
$found = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $found) { throw 'Потрібен .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0' }
$dotnetPath = $found.Source
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '..\.tools\cli-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ROOT = Split-Path -Parent $dotnetPath
Push-Location $PSScriptRoot
try {
    & $dotnetPath @args
    $result = $LASTEXITCODE
}
finally { Pop-Location }
exit $result
