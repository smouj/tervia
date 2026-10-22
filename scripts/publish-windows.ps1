param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts\publish\win-x64'),
    [string]$Version = '0.2.0-alpha',
    [ValidateSet('debug', 'release')]
    [string]$HomeBuildVariant = 'debug'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'src\Tervia.App\Tervia.App.csproj'
$androidProjectPath = Join-Path $repositoryRoot 'src\Tervia.Home.Android'
$gradleWrapper = Join-Path $androidProjectPath 'gradlew.bat'

$androidSdkCandidates = @($env:ANDROID_SDK_ROOT, $env:ANDROID_HOME, (Join-Path $env:LOCALAPPDATA 'Android\Sdk')) |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path -LiteralPath $_ -PathType Container) }
$androidSdk = $androidSdkCandidates | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($androidSdk)) {
    throw 'Android SDK was not found. Set ANDROID_SDK_ROOT or ANDROID_HOME, or install it in the standard local SDK folder.'
}
$env:ANDROID_SDK_ROOT = $androidSdk
$env:ANDROID_HOME = $androidSdk

if (-not (Test-Path -LiteralPath $gradleWrapper -PathType Leaf)) {
    throw "Gradle Wrapper is missing: $gradleWrapper"
}

if ($HomeBuildVariant -eq 'release') {
    $requiredSigningVariables = @(
        'TERVIA_HOME_KEYSTORE_PATH',
        'TERVIA_HOME_KEYSTORE_PASSWORD',
        'TERVIA_HOME_KEY_ALIAS',
        'TERVIA_HOME_KEY_PASSWORD'
    )
    foreach ($variableName in $requiredSigningVariables) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($variableName))) {
            throw "A signed Android Home release needs $variableName. Keep signing credentials in local environment variables or GitHub Actions secrets."
        }
    }
    if (-not (Test-Path -LiteralPath $env:TERVIA_HOME_KEYSTORE_PATH -PathType Leaf)) {
        throw 'TERVIA_HOME_KEYSTORE_PATH does not point to a keystore file.'
    }
}

$gradleTask = ":app:assemble$($HomeBuildVariant.Substring(0,1).ToUpperInvariant())$($HomeBuildVariant.Substring(1))"
Push-Location $androidProjectPath
try {
    & $gradleWrapper --no-daemon $gradleTask
    if ($LASTEXITCODE -ne 0) {
        throw "Android Home build failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

$androidOutput = Join-Path $androidProjectPath "app\build\outputs\apk\$HomeBuildVariant\app-$HomeBuildVariant.apk"
if (-not (Test-Path -LiteralPath $androidOutput -PathType Leaf)) {
    throw "Expected signed Android Home package was not produced: $androidOutput"
}

$apksigner = Get-ChildItem -Path (Join-Path $androidSdk 'build-tools') -Filter 'apksigner.bat' -Recurse -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending |
    Select-Object -First 1 -ExpandProperty FullName
if ([string]::IsNullOrWhiteSpace($apksigner)) {
    throw "Android SDK apksigner was not found under $androidSdk."
}
& $apksigner verify --verbose $androidOutput
if ($LASTEXITCODE -ne 0) {
    throw 'Android Home APK signature validation failed.'
}

dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:Platform=x64 `
    "-p:TerviaHomeAndroidVariant=$HomeBuildVariant" `
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

$builtHomePackage = Join-Path $OutputDirectory 'Tervia.Home.Android.apk'
if (-not (Test-Path -LiteralPath $builtHomePackage -PathType Leaf)) {
    throw "Expected Android Home package was not copied into the Windows release: $builtHomePackage"
}

Write-Host "Published Tervia for Windows x64: $builtExecutable"
Write-Host "Published Tervia Home companion: $builtHomePackage"
