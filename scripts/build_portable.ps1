param(
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64",
  [string]$Version = "1.0.0"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$uiProj = Join-Path $root 'ChildGuard.UI\ChildGuard.UI.csproj'
$agentProj = Join-Path $root 'ChildGuard.Agent\ChildGuard.Agent.csproj'
$outUi = Join-Path $root 'out\UI'
$outAgent = Join-Path $root 'out\Agent'
$dist = Join-Path $root 'dist'
if (!(Test-Path $dist)) { New-Item -ItemType Directory -Path $dist | Out-Null }

Write-Host "Publish UI -> $outUi"
dotnet publish $uiProj -c $Configuration -r $Rid -o $outUi --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
Write-Host "Publish Agent -> $outAgent"
dotnet publish $agentProj -c $Configuration -r $Rid -o $outAgent --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Compose portable layout
$portableRoot = Join-Path $root 'portable'
if (Test-Path $portableRoot) { Remove-Item $portableRoot -Recurse -Force }
New-Item -ItemType Directory -Path $portableRoot | Out-Null
Copy-Item $outUi -Destination (Join-Path $portableRoot 'UI') -Recurse
Copy-Item $outAgent -Destination (Join-Path $portableRoot 'Agent') -Recurse
# Optional tools helpful for autostart
$toolsSrc = Join-Path $root 'installer\tools'
if (Test-Path $toolsSrc) {
  Copy-Item (Join-Path $toolsSrc '*') -Destination (Join-Path $portableRoot 'tools') -Recurse -ErrorAction SilentlyContinue
}

# Zip
$zipPath = Join-Path $dist ("ChildGuard_Portable_" + $Version + ".zip")
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $portableRoot '*') -DestinationPath $zipPath
Write-Host "Portable ZIP: $zipPath"

