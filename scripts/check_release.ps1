param(
  [string]$Owner = 'quangminh1212',
  [string]$Repo = 'ChildGuard',
  [string]$Tag = 'v1.0.3',
  [int]$Attempts = 12,
  [int]$DelaySeconds = 30
)
$ErrorActionPreference = 'Continue'

$headers = @{ 'User-Agent'='agent-mode'; 'Accept'='application/vnd.github+json' }
$found = $false
for($i=0; $i -lt $Attempts; $i++){
  try {
    $url = "https://api.github.com/repos/$Owner/$Repo/releases/tags/$Tag"
    $r = Invoke-RestMethod -Uri $url -Headers $headers -ErrorAction Stop
    if ($r.assets -and $r.assets.Count -gt 0) {
      Write-Output 'RELEASE_READY'
      foreach ($a in $r.assets) { Write-Output ($a.name + '|' + $a.browser_download_url) }
      $found = $true
      break
    }
  } catch {
    # ignore and retry
  }
  Start-Sleep -Seconds $DelaySeconds
}
if (-not $found) { Write-Output 'NOT_READY' }

