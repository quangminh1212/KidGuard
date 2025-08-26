param(
  [string]$RunName = "ChildGuardAgent"
)
$ErrorActionPreference = 'Continue'

$runKey = 'HKCU:Software\Microsoft\Windows\CurrentVersion\Run'
if (Get-ItemProperty -Path $runKey -Name $RunName -ErrorAction SilentlyContinue) {
  Remove-ItemProperty -Path $runKey -Name $RunName -Force
  Write-Host "Removed Run key: $RunName"
} else {
  Write-Host "Run key not found: $RunName"
}

