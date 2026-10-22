param([Parameter(Mandatory = $true)][string]$Version)
$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Publish = Join-Path $Root "artifacts/publish/$Version/win-x64"
$Release = Join-Path $Root "artifacts/release/$Version/windows"
if (-not (Test-Path (Join-Path $Publish 'WiiStudio.exe'))) { throw "Run publish.sh $Version win-x64 first." }
New-Item -ItemType Directory -Force $Release | Out-Null
$Zip = Join-Path $Release "WiiStudio-$Version-win-x64.zip"
if (Test-Path $Zip) { Remove-Item $Zip -Force }
Compress-Archive -Path (Join-Path $Publish '*') -DestinationPath $Zip -CompressionLevel Optimal
$Iscc = if ($env:ISCC_PATH) { $env:ISCC_PATH } else { Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe' }
if (-not (Test-Path $Iscc)) { throw 'Inno Setup 6 was not found. Set ISCC_PATH.' }
$env:WIISTUDIO_VERSION = $Version
$env:WIISTUDIO_PUBLISH_DIR = $Publish
$env:WIISTUDIO_OUTPUT_DIR = $Release
$env:WIISTUDIO_ICON_FILE = Join-Path $Root 'src/WiiStudio.App/Assets/wiistudio.ico'
& $Iscc (Join-Path $Root 'packaging/windows/WiiStudio.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed with exit code $LASTEXITCODE." }
