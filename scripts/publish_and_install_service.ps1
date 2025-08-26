param(
  [string]$Configuration = "Release",
  [string]$Rid = "win-x64",
  [string]$ServiceName = "ChildGuardService"
)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$serviceProj = Join-Path $root 'ChildGuard.Service\ChildGuard.Service.csproj'
$outDir = Join-Path $root 'out\Service'

Write-Host "Publishing service to $outDir ..."
dotnet publish $serviceProj -c $Configuration -r $Rid -o $outDir --no-self-contained

$exePath = Join-Path $outDir 'ChildGuard.Service.exe'
if (!(Test-Path $exePath)) { throw "Service executable not found at $exePath" }

Write-Host "Creating/Updating Windows service $ServiceName ..."
# Create or update service
$svc = sc.exe query $ServiceName | Out-String
if ($LASTEXITCODE -ne 0 -or -not ($svc -match "SERVICE_NAME")) {
  sc.exe create $ServiceName binPath= '"' + $exePath + '"' start= auto DisplayName= "ChildGuard Service" | Write-Host
  sc.exe description $ServiceName "Child activity monitoring service" | Write-Host
} else {
  sc.exe config $ServiceName binPath= '"' + $exePath + '"' start= auto | Write-Host
}

# Set recovery actions: restart up to 3 times
sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/5000/restart/5000 | Write-Host

Write-Host "Starting service..."
sc.exe start $ServiceName | Write-Host

Write-Host "Done.
Service installed and started."

