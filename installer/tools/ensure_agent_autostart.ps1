param(
  [ValidateSet('allusers','current')]
  [string]$Mode = 'current',
  [string]$TaskName = 'ChildGuardAgent',
  [string]$ExePath
)
$ErrorActionPreference = 'Stop'

function Set-RunKey([string]$exe, [string]$name) {
  $runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
  if (-not (Test-Path $runKey)) { New-Item -Path $runKey -Force | Out-Null }
  New-ItemProperty -Path $runKey -Name $name -PropertyType String -Value ('"' + $exe + '"') -Force | Out-Null
  Write-Host "[Autostart] Set HKCU Run: $name -> $exe"
}

if (-not $ExePath -or -not (Test-Path $ExePath)) {
  $base = Split-Path -Parent $PSScriptRoot
  $ExePath = Join-Path $base '..\Agent\ChildGuard.Agent.exe' | Resolve-Path | Select-Object -ExpandProperty Path
}
if (!(Test-Path $ExePath)) { throw "Agent exe not found at $ExePath" }

if ($Mode -eq 'allusers') {
  try {
    $wd = Split-Path $ExePath -Parent
    $act = New-ScheduledTaskAction -Execute $ExePath -WorkingDirectory $wd
    $trg = New-ScheduledTaskTrigger -AtLogOn
    $principal = New-ScheduledTaskPrincipal -GroupId 'Users' -RunLevel LeastPrivilege
    Register-ScheduledTask -TaskName $TaskName -Action $act -Trigger $trg -Principal $principal -Force | Out-Null
    Write-Host "[Autostart] Registered Scheduled Task for all users: $TaskName"
    return
  } catch {
    Write-Host "[Autostart] All-users task failed: $($_.Exception.Message). Falling back to current user."
    $Mode = 'current'
  }
}
if ($Mode -eq 'current') {
  try {
    $ru = "$env:USERNAME"
    $quoted = '"' + $ExePath + '"'
    $null = schtasks.exe /Query /TN $TaskName 2>$null
    if ($LASTEXITCODE -eq 0) {
      schtasks.exe /Change /TN $TaskName /TR $quoted | Out-Null
    } else {
      schtasks.exe /Create /F /SC ONLOGON /RL LIMITED /RU $ru /TN $TaskName /TR $quoted | Out-Null
    }
    Write-Host "[Autostart] Registered Scheduled Task for current user ${ru}: $TaskName"
    return
  } catch {
    Write-Host "[Autostart] Current-user task failed: $($_.Exception.Message). Falling back to HKCU Run."
  }
}
# Fallback
Set-RunKey -exe $ExePath -name $TaskName
