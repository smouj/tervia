param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts\publish\win-x64'),
    [string]$Version = '0.1.0-alpha'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'src\Tervia.App\Tervia.App.csproj'

dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:Platform=x64 `
    -p:WindowsAppSDKSelfContained=true `
    -p:EnableMsixTooling=true `
    "-p:Version=$Version" `
    --output $OutputDirectory

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$builtExecutable = Join-Path $OutputDirectory 'Tervia.exe'
if (-not (Test-Path -LiteralPath $builtExecutable -PathType Leaf)) {
    throw "Expected self-contained executable was not produced: $builtExecutable"
}

Write-Host "Published Tervia for Windows x64: $builtExecutable"
