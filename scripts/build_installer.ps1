param(
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64",
  [string]$Version = "1.0.0"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$agentProj = Join-Path $root 'ChildGuard.Agent\ChildGuard.Agent.csproj'
$serviceProj = Join-Path $root 'ChildGuard.Service\ChildGuard.Service.csproj'
$outAgent = Join-Path $root 'out\Agent'
$outService = Join-Path $root 'out\Service'
$iss = Join-Path $root 'installer\ChildGuard.iss'
$dist = Join-Path $root 'dist'

if (!(Test-Path $dist)) { New-Item -ItemType Directory -Path $dist | Out-Null }

Write-Host "Publishing Agent -> $outAgent"
dotnet publish $agentProj -c $Configuration -r $Rid -o $outAgent --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
Write-Host "Publishing Service -> $outService"
dotnet publish $serviceProj -c $Configuration -r $Rid -o $outService --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Update version in ISS dynamically (optional)
$issContent = Get-Content -Raw $iss
$issContent = $issContent -replace '#define AppVersion ".*?"', ('#define AppVersion "' + $Version + '"')
Set-Content -Path $iss -Value $issContent -Encoding UTF8

# Find ISCC
$possible = @(
  (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
  (Join-Path ${env:ProgramFiles} 'Inno Setup 6\ISCC.exe'),
  'iscc.exe'
)
$ISCC = $null
foreach ($p in $possible) {
  if (Get-Command $p -ErrorAction SilentlyContinue) { $ISCC = $p; break }
}
if (-not $ISCC) {
  Write-Warning "Inno Setup Compiler (ISCC.exe) not found. Please install Inno Setup 6 and ensure ISCC.exe is in PATH."
  Write-Host "You can then compile: ISCC.exe `"$iss`""
  exit 1
}

Write-Host "Compiling installer with $ISCC ..."
& $ISCC /Qp $iss | Write-Host

# Move output to dist
$pattern = Join-Path $root 'installer\Output\ChildGuardSetup_*.exe'
$files = Get-ChildItem $pattern -ErrorAction SilentlyContinue
if ($files) {
  foreach ($f in $files) { Copy-Item $f.FullName -Destination $dist -Force }
  Write-Host "Installer(s) copied to $dist"
} else {
  Write-Warning "No installer output found at $pattern"
}

