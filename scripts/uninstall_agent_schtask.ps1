param(
  [string]$TaskName = "ChildGuardAgent"
)
$ErrorActionPreference = 'Continue'

Write-Host "Deleting Scheduled Task '$TaskName' (if exists) ..."
schtasks.exe /Delete /F /TN $TaskName | Write-Host
Write-Host "Done."

