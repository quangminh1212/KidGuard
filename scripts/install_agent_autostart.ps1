param(
  [string]$RunName = "ChildGuardAgent",
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

# Ensure agent is published
& (Join-Path $PSScriptRoot 'publish_agent.ps1') -Configuration $Configuration -Rid $Rid

$exePath = Join-Path $root 'out\Agent\ChildGuard.Agent.exe'
if (!(Test-Path $exePath)) { throw "Agent exe not found at $exePath" }

$runKey = 'HKCU:Software\Microsoft\Windows\CurrentVersion\Run'
Write-Host "Setting Run key $RunName -> $exePath"
New-ItemProperty -Path $runKey -Name $RunName -PropertyType String -Value ('"' + $exePath + '"') -Force | Out-Null
Write-Host "Agent will auto-start at user logon."

