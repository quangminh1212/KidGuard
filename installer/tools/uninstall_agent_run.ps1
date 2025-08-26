param(
  [string]$RunName = 'ChildGuardAgent'
)
$ErrorActionPreference = 'Continue'
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
if (Get-ItemProperty -Path $runKey -Name $RunName -ErrorAction SilentlyContinue) {
  Remove-ItemProperty -Path $runKey -Name $RunName -Force
  Write-Host "Removed HKCU Run: $RunName"
} else {
  Write-Host "HKCU Run not found: $RunName"
}
