param(
  [string]$ShortcutName = "ChildGuard Agent.lnk",
  [string]$Target = "C:\\Program Files\\ChildGuard\\Agent\\ChildGuard.Agent.exe",
  [string]$WorkingDir = "C:\\Program Files\\ChildGuard\\Agent"
)
$ErrorActionPreference = 'Stop'

$desktop=[Environment]::GetFolderPath('Desktop')
$spath = Join-Path $desktop $ShortcutName
$w=New-Object -ComObject WScript.Shell
$lnk=$w.CreateShortcut($spath)
$lnk.TargetPath=$Target
$lnk.WorkingDirectory=$WorkingDir
$lnk.IconLocation=$Target+',0'
$lnk.Save()
Write-Host "ShortcutCreated: $spath"

