param(
  [string]$TaskName = "ChildGuardAgent",
  [string]$ExePath
)
$ErrorActionPreference = 'Stop'

if (-not $ExePath -or -not (Test-Path $ExePath)) {
  $base = Split-Path -Parent $PSScriptRoot
  $ExePath = Join-Path $base 'Agent\ChildGuard.Agent.exe'
}
if (!(Test-Path $ExePath)) { throw "Agent exe not found at $ExePath" }

$ru = "$env:USERNAME"
Write-Host "Creating Scheduled Task '$TaskName' for user $ru -> $ExePath"
$exists = schtasks.exe /Query /TN $TaskName 2>$null
if ($LASTEXITCODE -eq 0) {
  schtasks.exe /Change /TN $TaskName /TR ('"' + $ExePath + '"') | Out-Null
} else {
  schtasks.exe /Create /F /SC ONLOGON /RL LIMITED /RU $ru /TN $TaskName /TR ('"' + $ExePath + '"') | Out-Null
}
Write-Host "Scheduled Task installed/updated."

