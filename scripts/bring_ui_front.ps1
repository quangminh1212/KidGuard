$ErrorActionPreference = 'Stop'

$p = Get-Process -Name 'ChildGuard.UI' -ErrorAction SilentlyContinue
if ($p) {
  $hwnd = $p.MainWindowHandle
  if ($hwnd -ne 0) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@
    [Win32]::ShowWindowAsync([System.IntPtr]$hwnd, 9) | Out-Null # SW_RESTORE
    [Win32]::SetForegroundWindow([System.IntPtr]$hwnd) | Out-Null
    Write-Host 'BroughtToFront'
  } else {
    Write-Host 'NoWindowHandle'
  }
} else {
  Write-Host 'NotRunning'
}

