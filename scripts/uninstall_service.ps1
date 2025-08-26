param(
  [string]$ServiceName = "ChildGuardService"
)
$ErrorActionPreference = 'Continue'

sc.exe stop $ServiceName | Write-Host
Start-Sleep -Seconds 2
sc.exe delete $ServiceName | Write-Host
Write-Host "Service $ServiceName stopped and deleted."

