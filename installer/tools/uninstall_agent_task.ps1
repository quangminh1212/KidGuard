param(
  [string]$TaskName = "ChildGuardAgent"
)
$ErrorActionPreference = 'Continue'

function Write-Info($msg) { Write-Host "[ChildGuard Installer] $msg" }

try {
  Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction Stop | Out-Null
  Write-Info "Unregistered scheduled task: $TaskName"
} catch {
  Write-Info "Unregister via API failed: $($_.Exception.Message). Trying schtasks.exe"
  schtasks.exe /Delete /F /TN $TaskName | Out-Null
}

