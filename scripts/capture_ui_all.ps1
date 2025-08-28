param(
  [string]$OutputDir = "docs/screenshots"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

# Kích thước cần chụp
$sizes = @(
  @{ W = 1200; H = 700 },
  @{ W = 1000; H = 600 },
  @{ W = 1400; H = 900 }
)

foreach ($s in $sizes) {
  Write-Host "-- Capture Dashboard ${($s.W)}x${($s.H)} --"
  powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root 'scripts/capture_ui_screenshot.ps1') -OutputDir $OutputDir -Width $($s.W) -Height $($s.H))
  if ($LASTEXITCODE -ne 0) { throw "Dashboard capture failed at ${($s.W)}x${($s.H)}" }

  Write-Host "-- Capture Settings/Reports ${($s.W)}x${($s.H)} --"
  powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root 'scripts/capture_ui_screens.ps1') -OutputDir $OutputDir -Width $($s.W) -Height $($s.H)
  if ($LASTEXITCODE -ne 0) { throw "Screens capture failed at ${($s.W)}x${($s.H)}" }
}

Write-Host "All screenshots saved to" (Join-Path $root $OutputDir)

