param(
  [string]$TaskName = "ChildGuardAgent",
  [string]$AgentExe = "C:\\Program Files\\ChildGuard\\Agent\\ChildGuard.Agent.exe"
)
$ErrorActionPreference = 'Stop'

if (!(Test-Path $AgentExe)) { throw "Agent exe not found at $AgentExe" }

$ru = $env:USERNAME
$quoted = '"' + $AgentExe + '"'
Write-Host "Registering scheduled task for current user ${ru}: $TaskName -> $AgentExe"
schtasks.exe /Create /F /SC ONLOGON /RL LIMITED /RU $ru /TN $TaskName /TR $quoted | Out-Null
Write-Host "Registered scheduled task for current user: $TaskName"

