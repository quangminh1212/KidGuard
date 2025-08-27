param(
  [string]$OutPath = "C:\VF\ChildGuard\ChildGuard.UI\Assets\app.ico"
)
$ErrorActionPreference = 'Stop'

# Ensure directory
$dir = [System.IO.Path]::GetDirectoryName($OutPath)
if (!(Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }

Add-Type -AssemblyName System.Drawing

# Create a simple round icon with CG letters
$size = 256
$bmp = New-Object System.Drawing.Bitmap($size, $size)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::White)
$accent = [System.Drawing.Color]::DodgerBlue
$brush = New-Object System.Drawing.SolidBrush($accent)
$g.FillEllipse($brush, 8, 8, $size-16, $size-16)

# Draw letters
$font = New-Object System.Drawing.Font('Segoe UI', 120, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$sf = New-Object System.Drawing.StringFormat
$sf.Alignment = [System.Drawing.StringAlignment]::Center
$sf.LineAlignment = [System.Drawing.StringAlignment]::Center
$rect = New-Object System.Drawing.RectangleF(0,0,$size,$size)
$wbrush = [System.Drawing.Brushes]::White
$g.DrawString('CG', $font, $wbrush, $rect, $sf)
$g.Dispose()

# Save as .ico
$hicon = $bmp.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($hicon)
$fs = [System.IO.File]::Open($OutPath, [System.IO.FileMode]::Create)
$icon.Save($fs)
$fs.Close()
$bmp.Dispose()
Write-Host "Icon generated: $OutPath"

