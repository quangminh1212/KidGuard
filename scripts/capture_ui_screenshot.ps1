param(
  [string]$OutputDir = "docs/screenshots",
  [int]$Width,
  [int]$Height
)
$ErrorActionPreference = 'Stop'

# Ensure output directory exists
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root $OutputDir
if (!(Test-Path $out)) { New-Item -ItemType Directory -Path $out -Force | Out-Null }

# Build and start UI if needed
$uiExe = Join-Path $root 'ChildGuard.UI\\bin\\Debug\\net8.0-windows\\ChildGuard.UI.exe'
if (!(Test-Path $uiExe)) {
  dotnet build (Join-Path $root 'ChildGuard.sln') -c Debug | Out-Null
}
$proc = Get-Process -Name 'ChildGuard.UI' -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) {
  if (Test-Path $uiExe) { $proc = Start-Process -FilePath $uiExe -PassThru }
}
if (-not $proc) { throw 'Could not start UI process.' }

# Wait for window
$timeout = [DateTime]::UtcNow.AddSeconds(10)
while (($proc.MainWindowHandle -eq 0) -and ([DateTime]::UtcNow -lt $timeout)) { Start-Sleep -Milliseconds 200; $proc.Refresh() }
if ($proc.MainWindowHandle -eq 0) { throw 'UI window not ready.' }

Add-Type -ReferencedAssemblies "System.Drawing" -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
public static class ScreenShotHelper {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
  [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
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

# Resize if requested
if ($PSBoundParameters.ContainsKey('Width') -and $PSBoundParameters.ContainsKey('Height')) {
  [ScreenShotHelper]::SetForegroundWindow([System.IntPtr]$proc.MainWindowHandle) | Out-Null
  [ScreenShotHelper]::MoveWindow([System.IntPtr]$proc.MainWindowHandle, 50, 50, $Width, $Height, $true) | Out-Null
  Start-Sleep -Milliseconds 200
}

$mainPath = if ($PSBoundParameters.ContainsKey('Width') -and $PSBoundParameters.ContainsKey('Height')) {
  Join-Path $out ("childguard_dashboard_{0}x{1}.png" -f $Width, $Height)
} else {
  Join-Path $out 'childguard_ui_main.png'
}
[ScreenShotHelper]::SaveWindowToFile($proc.MainWindowHandle, $mainPath)
Write-Host "Saved: $mainPath"

