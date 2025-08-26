param(
  [string]$ExePath = "C:\\Program Files\\ChildGuard\\Agent\\ChildGuard.Agent.exe",
  [string]$RunName = "ChildGuardAgent"
)
$ErrorActionPreference = 'Stop'

$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
if (-not (Test-Path $runKey)) { New-Item -Path $runKey -Force | Out-Null }
New-ItemProperty -Path $runKey -Name $RunName -PropertyType String -Value ('"' + $ExePath + '"') -Force | Out-Null
Write-Host "Set HKCU Run entry: $RunName -> $ExePath"

