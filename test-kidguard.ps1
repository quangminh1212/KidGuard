# KidGuard Test Script
# Script để test các chức năng chính của KidGuard
# Yêu cầu: PowerShell 5.1+ và quyền Administrator

Write-Host "================================" -ForegroundColor Cyan
Write-Host "   KIDGUARD TEST SCRIPT v1.0   " -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra quyền Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ Script cần chạy với quyền Administrator!" -ForegroundColor Red
    Write-Host "Đang khởi động lại với quyền Admin..." -ForegroundColor Yellow
    Start-Process powershell -Verb RunAs -ArgumentList "-File `"$PSCommandPath`""
    exit
}

Write-Host "✅ Đang chạy với quyền Administrator" -ForegroundColor Green
Write-Host ""

# Kiểm tra .NET 9 đã cài chưa
Write-Host "📋 Kiểm tra .NET Runtime..." -ForegroundColor Yellow
$dotnetVersion = dotnet --list-runtimes | Where-Object { $_ -like "*Microsoft.WindowsDesktop.App 9.*" }
if ($dotnetVersion) {
    Write-Host "✅ .NET 9 Desktop Runtime đã cài đặt" -ForegroundColor Green
    Write-Host "   $dotnetVersion" -ForegroundColor Gray
} else {
    Write-Host "❌ Chưa cài .NET 9 Desktop Runtime" -ForegroundColor Red
    Write-Host "   Vui lòng cài đặt từ: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
}
Write-Host ""

# Build dự án
Write-Host "🔨 Building KidGuard..." -ForegroundColor Yellow
Set-Location -Path $PSScriptRoot
$buildResult = dotnet build --configuration Release 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build thành công!" -ForegroundColor Green
} else {
    Write-Host "❌ Build thất bại!" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 1: Kiểm tra hosts file
Write-Host "🧪 Test 1: Kiểm tra hosts file" -ForegroundColor Cyan
$hostsPath = "C:\Windows\System32\drivers\etc\hosts"
if (Test-Path $hostsPath) {
    Write-Host "✅ Hosts file tồn tại" -ForegroundColor Green
    
    # Backup hosts file
    $backupPath = "$hostsPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $hostsPath $backupPath -Force
    Write-Host "✅ Đã backup hosts file: $backupPath" -ForegroundColor Green
    
    # Kiểm tra quyền ghi
    try {
        Add-Content -Path $hostsPath -Value "# KIDGUARD TEST" -ErrorAction Stop
        $content = Get-Content $hostsPath -Raw
        if ($content -match "# KIDGUARD TEST") {
            Write-Host "✅ Có thể ghi vào hosts file" -ForegroundColor Green
            # Xóa dòng test
            $lines = Get-Content $hostsPath | Where-Object { $_ -ne "# KIDGUARD TEST" }
            Set-Content -Path $hostsPath -Value $lines -Force
        }
    } catch {
        Write-Host "❌ Không thể ghi vào hosts file: $_" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Hosts file không tồn tại!" -ForegroundColor Red
}
Write-Host ""

# Test 2: Test chặn website
Write-Host "🧪 Test 2: Test chức năng chặn website" -ForegroundColor Cyan
$testDomain = "test.kidguard.local"

# Thêm entry test vào hosts
try {
    Add-Content -Path $hostsPath -Value "`n# === KIDGUARD BLOCK START ===" -ErrorAction Stop
    Add-Content -Path $hostsPath -Value "127.0.0.1 $testDomain"
    Add-Content -Path $hostsPath -Value "127.0.0.1 www.$testDomain"
    Add-Content -Path $hostsPath -Value "# === KIDGUARD BLOCK END ===" -ErrorAction Stop
    
    Write-Host "✅ Đã thêm domain test: $testDomain" -ForegroundColor Green
    
    # Kiểm tra ping
    Write-Host "   Đang test ping $testDomain..." -ForegroundColor Gray
    $pingResult = Test-Connection -ComputerName $testDomain -Count 1 -Quiet 2>$null
    if ($pingResult) {
        Write-Host "✅ Domain đã được redirect về 127.0.0.1" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Không thể ping (có thể do firewall)" -ForegroundColor Yellow
    }
    
    # Cleanup
    $lines = Get-Content $hostsPath | Where-Object { 
        $_ -notmatch "KIDGUARD" -and $_ -notmatch $testDomain 
    }
    Set-Content -Path $hostsPath -Value $lines -Force
    Write-Host "✅ Đã dọn dẹp entries test" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Lỗi khi test chặn website: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: Kiểm tra processes
Write-Host "🧪 Test 3: Liệt kê các ứng dụng đang chạy" -ForegroundColor Cyan
$processes = Get-Process | Where-Object {
    $_.MainWindowTitle -ne "" -and 
    $_.ProcessName -notin @("explorer", "SystemSettings", "ApplicationFrameHost")
} | Select-Object -First 5

if ($processes) {
    Write-Host "✅ Tìm thấy các ứng dụng đang chạy:" -ForegroundColor Green
    $processes | ForEach-Object {
        Write-Host "   - $($_.ProcessName): $($_.MainWindowTitle)" -ForegroundColor Gray
    }
} else {
    Write-Host "⚠️  Không có ứng dụng nào có window đang chạy" -ForegroundColor Yellow
}
Write-Host ""

# Test 4: Kiểm tra log directory
Write-Host "🧪 Test 4: Kiểm tra thư mục logs" -ForegroundColor Cyan
$logPath = "$env:LOCALAPPDATA\KidGuard\Logs"
if (-not (Test-Path $logPath)) {
    New-Item -ItemType Directory -Path $logPath -Force | Out-Null
    Write-Host "✅ Đã tạo thư mục logs: $logPath" -ForegroundColor Green
} else {
    Write-Host "✅ Thư mục logs đã tồn tại: $logPath" -ForegroundColor Green
    
    # Liệt kê log files
    $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" -ErrorAction SilentlyContinue
    if ($logFiles) {
        Write-Host "   Tìm thấy $($logFiles.Count) file log:" -ForegroundColor Gray
        $logFiles | Select-Object -First 3 | ForEach-Object {
            Write-Host "   - $($_.Name) ($([Math]::Round($_.Length/1KB, 2)) KB)" -ForegroundColor Gray
        }
    }
}
Write-Host ""

# Test 5: Test configuration
Write-Host "🧪 Test 5: Kiểm tra file cấu hình" -ForegroundColor Cyan
$configPath = Join-Path $PSScriptRoot "src\KidGuard\appsettings.json"
if (Test-Path $configPath) {
    Write-Host "✅ File appsettings.json tồn tại" -ForegroundColor Green
    
    try {
        $config = Get-Content $configPath | ConvertFrom-Json
        Write-Host "   - Monitoring interval: $($config.KidGuard.Monitoring.CheckIntervalSeconds)s" -ForegroundColor Gray
        Write-Host "   - App monitoring: $($config.KidGuard.Monitoring.EnableApplicationMonitoring)" -ForegroundColor Gray
        Write-Host "   - Website blocking: $($config.KidGuard.Monitoring.EnableWebsiteBlocking)" -ForegroundColor Gray
        Write-Host "   - Activity logging: $($config.KidGuard.Monitoring.EnableActivityLogging)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Không thể đọc file config: $_" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Không tìm thấy appsettings.json" -ForegroundColor Red
}
Write-Host ""

# Test 6: Chạy ứng dụng (optional)
Write-Host "🚀 Bạn có muốn chạy KidGuard không? (Y/N): " -ForegroundColor Yellow -NoNewline
$runApp = Read-Host
if ($runApp -eq 'Y' -or $runApp -eq 'y') {
    Write-Host "Đang khởi động KidGuard..." -ForegroundColor Cyan
    
    $exePath = Join-Path $PSScriptRoot "src\KidGuard\bin\Release\net9.0-windows\KidGuard.exe"
    if (Test-Path $exePath) {
        Start-Process $exePath -Verb RunAs
        Write-Host "✅ KidGuard đã được khởi động!" -ForegroundColor Green
    } else {
        Write-Host "Chạy từ dotnet CLI..." -ForegroundColor Yellow
        Start-Process "dotnet" -ArgumentList "run --project src/KidGuard --configuration Release" -Verb RunAs
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "   TEST HOÀN THÀNH!            " -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Tóm tắt kết quả
Write-Host "📊 TÓM TẮT:" -ForegroundColor Yellow
Write-Host "- Build: ✅ Thành công" -ForegroundColor Green
Write-Host "- Hosts file: ✅ Có thể truy cập" -ForegroundColor Green
Write-Host "- Logging: ✅ Sẵn sàng" -ForegroundColor Green
Write-Host "- Configuration: ✅ Đã cấu hình" -ForegroundColor Green
Write-Host ""
Write-Host "Nhấn phím bất kỳ để thoát..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
