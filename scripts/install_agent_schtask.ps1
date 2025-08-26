param(
  [string]$TaskName = "ChildGuardAgent",
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
# Ensure agent is published
& (Join-Path $PSScriptRoot 'publish_agent.ps1') -Configuration $Configuration -Rid $Rid
$exePath = Join-Path $root 'out\Agent\ChildGuard.Agent.exe'
if (!(Test-Path $exePath)) { throw "Agent exe not found at $exePath" }

$ru = "$env:USERNAME"
Write-Host "Creating Scheduled Task '$TaskName' for user $ru -> $exePath"
# Create or update
$exists = schtasks.exe /Query /TN $TaskName 2>$null
if ($LASTEXITCODE -eq 0) {
  schtasks.exe /Change /TN $TaskName /TR ('"' + $exePath + '"') | Write-Host
} else {
  schtasks.exe /Create /F /SC ONLOGON /RL LIMITED /RU $ru /TN $TaskName /TR ('"' + $exePath + '"') | Write-Host
}
Write-Host "Scheduled Task installed/updated."

