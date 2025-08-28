param(
  [string]$OutputDir = "docs/screenshots",
  [int]$Width,
  [int]$Height
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root $OutputDir
if (!(Test-Path $out)) { New-Item -ItemType Directory -Path $out -Force | Out-Null }

$uiExe = Join-Path $root 'ChildGuard.UI\\bin\\Debug\\net8.0-windows\\ChildGuard.UI.exe'
if (!(Test-Path $uiExe)) { dotnet build (Join-Path $root 'ChildGuard.sln') -c Debug | Out-Null }

# Helper to capture by window title substring and process id (multi-language)
Add-Type -ReferencedAssemblies "System.Drawing" -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
public static class WinSnap {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
  [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  public static IntPtr FindWindowOfProcess(int pid, string titleSubstrs) {
    string[] keys = (titleSubstrs ?? "").Split(new[]{'|',';'}, StringSplitOptions.RemoveEmptyEntries);
    IntPtr found = IntPtr.Zero;
    EnumWindows((h, l) => {
      if (!IsWindowVisible(h)) return true;
      uint p; GetWindowThreadProcessId(h, out p);
      if (p != pid) return true;
      int len = GetWindowTextLength(h);
      var sb = new StringBuilder(len + 1);
      GetWindowText(h, sb, sb.Capacity);
      string t = sb.ToString();
      foreach (var k in keys) {
        if (!string.IsNullOrEmpty(t) && t.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) { found = h; return false; }
      }
      return true;
    }, IntPtr.Zero);
    return found;
  }
  public static void SaveWindowToFile(IntPtr hWnd, string path) {
    RECT r; if (!GetWindowRect(hWnd, out r)) throw new Exception("GetWindowRect failed");
    int w = r.Right - r.Left; int h = r.Bottom - r.Top;
    using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
    using (var g = Graphics.FromImage(bmp))
    {
      IntPtr hdc = g.GetHdc();
      try { PrintWindow(hWnd, hdc, 0); }
      finally { g.ReleaseHdc(hdc); }
      bmp.Save(path, ImageFormat.Png);
    }
  }
}
'@

function Capture-WindowFor($proc, $name, $titleKeys) {
  $timeout = [DateTime]::UtcNow.AddSeconds(10)
  $h = [IntPtr]::Zero
  while($h -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $timeout) {
    $h = [WinSnap]::FindWindowOfProcess($proc.Id, $titleKeys)
    Start-Sleep -Milliseconds 200
  }
  if ($h -eq [IntPtr]::Zero) { throw "Could not find window: $name" }
  # resize if requested
  if ($PSBoundParameters.ContainsKey('Width') -and $PSBoundParameters.ContainsKey('Height')) {
    [WinSnap]::MoveWindow($h, 50, 50, $Width, $Height, $true) | Out-Null
    Start-Sleep -Milliseconds 200
  }
  $suffix = if ($PSBoundParameters.ContainsKey('Width') -and $PSBoundParameters.ContainsKey('Height')) { "_{0}x{1}" -f $Width, $Height } else { "" }
  $path = Join-Path $out ($name.Replace('.png',"$suffix.png"))
  [WinSnap]::SaveWindowToFile($h, $path)
  Write-Host "Saved: $path"
}

# Capture Settings (multi-language title)
$proc = Start-Process -FilePath $uiExe -ArgumentList '--open settings' -PassThru
Start-Sleep -Milliseconds 400
Capture-WindowFor $proc 'childguard_settings.png' 'Settings|Cài đặt'
Get-Process -Id $proc.Id -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Capture Reports (multi-language title)
$proc = Start-Process -FilePath $uiExe -ArgumentList '--open reports' -PassThru
Start-Sleep -Milliseconds 400
Capture-WindowFor $proc 'childguard_reports.png' 'Reports|Báo cáo'
Get-Process -Id $proc.Id -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

