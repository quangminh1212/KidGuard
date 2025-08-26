param(
  [string]$Exe = "C:\\Program Files\\ChildGuard\\Agent\\ChildGuard.Agent.exe"
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Exe)) { throw "Agent exe not found at $Exe" }

if (-not (Get-Process -Name 'ChildGuard.Agent' -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath $Exe -WorkingDirectory (Split-Path $Exe -Parent)
  Write-Host "AgentStarted"
} else {
  Write-Host "AgentAlreadyRunning"
}

