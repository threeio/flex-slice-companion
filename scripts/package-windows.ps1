param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "publish/FlexSliceCompanion-$Runtime"
$zipPath = Join-Path $root "publish/FlexSliceCompanion-$Runtime.zip"

Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir | Out-Null

dotnet test (Join-Path $root "FlexSliceCompanion.sln") --configuration $Configuration

dotnet publish (Join-Path $root "src/FlexSliceCompanion.Windows/FlexSliceCompanion.Windows.csproj") `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

Copy-Item (Join-Path $root "README.md") $publishDir
Copy-Item (Join-Path $root "LICENSE") $publishDir
Copy-Item (Join-Path $root "appsettings.example.json") $publishDir
Copy-Item (Join-Path $root "docs") (Join-Path $publishDir "docs") -Recurse

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath
Write-Host "Created $zipPath"
