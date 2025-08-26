param(
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$agentProj = Join-Path $root 'ChildGuard.Agent\ChildGuard.Agent.csproj'
$outDir = Join-Path $root 'out\Agent'

Write-Host "Publishing Agent to $outDir ..."
dotnet publish $agentProj -c $Configuration -r $Rid -o $outDir --no-self-contained

$exePath = Join-Path $outDir 'ChildGuard.Agent.exe'
if (!(Test-Path $exePath)) { throw "Agent executable not found at $exePath" }

Write-Host "Publish completed: $exePath"

