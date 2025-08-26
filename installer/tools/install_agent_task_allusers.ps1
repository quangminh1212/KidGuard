param(
  [string]$TaskName = "ChildGuardAgent",
  [string]$ExePath
)
$ErrorActionPreference = 'Stop'

function Write-Info($msg) { Write-Host "[ChildGuard Installer] $msg" }

if (-not $ExePath -or -not (Test-Path $ExePath)) {
  $base = Split-Path -Parent $PSScriptRoot
  $ExePath = Join-Path $base 'Agent\ChildGuard.Agent.exe'
}
if (!(Test-Path $ExePath)) { throw "Agent exe not found at $ExePath" }

$wd = Split-Path $ExePath -Parent
$act = New-ScheduledTaskAction -Execute $ExePath -WorkingDirectory $wd
$trg = New-ScheduledTaskTrigger -AtLogOn

try {
  # Principal for any interactive user (Users group)
  $principal = New-ScheduledTaskPrincipal -GroupId 'Users' -RunLevel LeastPrivilege
  Register-ScheduledTask -TaskName $TaskName -Action $act -Trigger $trg -Principal $principal -Force | Out-Null
  Write-Info "Registered scheduled task for all users: $TaskName -> $ExePath"
}
catch {
  Write-Info "Register for group failed: $($_.Exception.Message). Falling back to current user."
  $ru = "$env:USERNAME"
  try {
    $existing = schtasks.exe /Query /TN $TaskName 2>$null
    if ($LASTEXITCODE -eq 0) {
      schtasks.exe /Change /TN $TaskName /TR ('"' + $ExePath + '"') | Out-Null
    } else {
      schtasks.exe /Create /F /SC ONLOGON /RL LIMITED /RU $ru /TN $TaskName /TR ('"' + $ExePath + '"') | Out-Null
    }
    Write-Info "Registered scheduled task for current user $ru: $TaskName -> $ExePath"
  } catch {
    throw $_
  }
}

