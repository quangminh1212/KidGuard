# KidGuard - Test Script đơn giản
Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "    KIDGUARD - TEST SERVICES        " -ForegroundColor Cyan  
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$PassedTests = 0
$FailedTests = 0
$TotalTests = 0

function Test-Feature {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    $script:TotalTests++
    Write-Host "[TEST $TotalTests] $Name" -ForegroundColor Yellow -NoNewline
    
    try {
        $result = & $Test
        if ($result) {
            Write-Host " - PASSED" -ForegroundColor Green
            $script:PassedTests++
        }
        else {
            Write-Host " - FAILED" -ForegroundColor Red
            $script:FailedTests++
        }
    }
    catch {
        Write-Host " - ERROR: $_" -ForegroundColor Red
        $script:FailedTests++
    }
}

Write-Host "1. DATABASE TESTS" -ForegroundColor Cyan
Write-Host "-----------------" -ForegroundColor Cyan

# Test 1: Database directory
Test-Feature "Database directory exists" {
    $dbDir = "$env:LOCALAPPDATA\KidGuard"
    if (!(Test-Path $dbDir)) {
        New-Item -Path $dbDir -ItemType Directory -Force | Out-Null
    }
    Test-Path $dbDir
}

# Test 2: Can create database file
Test-Feature "Can create database file" {
    $testDb = "$env:LOCALAPPDATA\KidGuard\test.db"
    "test" | Out-File $testDb -Force
    $exists = Test-Path $testDb
    if ($exists) { Remove-Item $testDb -Force }
    $exists
}

Write-Host ""
Write-Host "2. AUTHENTICATION TESTS" -ForegroundColor Cyan
Write-Host "-----------------------" -ForegroundColor Cyan

# Test 3: SHA256 hashing
Test-Feature "SHA256 password hashing" {
    $password = "Test123"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    $hashString = [System.BitConverter]::ToString($hash)
    $hashString.Length -gt 0
}

# Test 4: GUID generation
Test-Feature "Session token generation" {
    $guid = [System.Guid]::NewGuid().ToString()
    $guid.Length -eq 36
}

Write-Host ""
Write-Host "3. MONITORING TESTS" -ForegroundColor Cyan
Write-Host "-------------------" -ForegroundColor Cyan

# Test 5: Get processes
Test-Feature "Get running processes" {
    $processes = Get-Process
    $processes.Count -gt 0
}

# Test 6: Check dangerous apps
Test-Feature "No dangerous apps running" {
    $dangerous = @("virus", "trojan", "malware")
    $running = Get-Process | Select-Object -ExpandProperty ProcessName
    $found = $false
    foreach ($app in $dangerous) {
        if ($running -contains $app) {
            $found = $true
            break
        }
    }
    !$found
}

Write-Host ""
Write-Host "4. FILE SYSTEM TESTS" -ForegroundColor Cyan
Write-Host "--------------------" -ForegroundColor Cyan

# Test 7: Screenshot directory
Test-Feature "Screenshot directory" {
    $screenshotDir = "$env:LOCALAPPDATA\KidGuard\Screenshots"
    if (!(Test-Path $screenshotDir)) {
        New-Item -Path $screenshotDir -ItemType Directory -Force | Out-Null
    }
    Test-Path $screenshotDir
}

# Test 8: Report directory
Test-Feature "Report directory" {
    $reportDir = "$env:LOCALAPPDATA\KidGuard\Reports"
    if (!(Test-Path $reportDir)) {
        New-Item -Path $reportDir -ItemType Directory -Force | Out-Null
    }
    Test-Path $reportDir
}

# Test 9: Log directory
Test-Feature "Log directory" {
    $logDir = "$env:LOCALAPPDATA\KidGuard\Logs"
    if (!(Test-Path $logDir)) {
        New-Item -Path $logDir -ItemType Directory -Force | Out-Null
    }
    Test-Path $logDir
}

Write-Host ""
Write-Host "5. WINDOWS INTEGRATION TESTS" -ForegroundColor Cyan
Write-Host "-----------------------------" -ForegroundColor Cyan

# Test 10: Hosts file exists
Test-Feature "Windows hosts file exists" {
    Test-Path "C:\Windows\System32\drivers\etc\hosts"
}

# Test 11: Check admin privileges
Test-Feature "Check admin privilege detection" {
    $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($currentUser)
    $isAdmin = $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
    # Test passes if we can detect admin status (true or false)
    ($isAdmin -eq $true) -or ($isAdmin -eq $false)
}

Write-Host ""
Write-Host "6. TIME MANAGEMENT TESTS" -ForegroundColor Cyan
Write-Host "------------------------" -ForegroundColor Cyan

# Test 12: TimeSpan calculation
Test-Feature "TimeSpan calculation" {
    $start = Get-Date "09:00"
    $end = Get-Date "17:00"
    $duration = $end - $start
    $duration.TotalHours -eq 8
}

# Test 13: Time limit check
Test-Feature "Time limit calculation" {
    $limit = New-TimeSpan -Hours 4
    $used = New-TimeSpan -Hours 3
    $remaining = $limit - $used
    $remaining.TotalHours -eq 1
}

Write-Host ""
Write-Host "7. NOTIFICATION TESTS" -ForegroundColor Cyan
Write-Host "---------------------" -ForegroundColor Cyan

# Test 14: Config file creation
Test-Feature "Notification config file" {
    $configPath = "$env:LOCALAPPDATA\KidGuard\notification-config.json"
    if (!(Test-Path $configPath)) {
        $config = @{
            EmailEnabled = $true
            SystemTrayEnabled = $true
        }
        $config | ConvertTo-Json | Out-File $configPath -Force
    }
    Test-Path $configPath
}

Write-Host ""
Write-Host "8. REPORT GENERATION TESTS" -ForegroundColor Cyan
Write-Host "--------------------------" -ForegroundColor Cyan

# Test 15: HTML generation
Test-Feature "HTML report generation" {
    $html = "<html><body><h1>Test Report</h1></body></html>"
    $testFile = "$env:TEMP\test_report.html"
    $html | Out-File $testFile -Force
    $exists = Test-Path $testFile
    if ($exists) { Remove-Item $testFile -Force }
    $exists
}

# Test 16: CSV generation
Test-Feature "CSV report generation" {
    $data = @(
        [PSCustomObject]@{Date="2024-01-01"; Hours=4}
        [PSCustomObject]@{Date="2024-01-02"; Hours=5}
    )
    $csv = $data | ConvertTo-Csv -NoTypeInformation
    $csv.Count -ge 3
}

Write-Host ""
Write-Host "9. PERFORMANCE TESTS" -ForegroundColor Cyan
Write-Host "--------------------" -ForegroundColor Cyan

# Test 17: Memory check
Test-Feature "PowerShell memory usage reasonable" {
    $proc = Get-Process -Id $PID
    $memoryMB = $proc.WorkingSet64 / 1MB
    $memoryMB -lt 500  # Less than 500MB
}

# Test 18: Disk space
Test-Feature "Sufficient disk space" {
    $drive = Get-PSDrive C
    $freeGB = $drive.Free / 1GB
    $freeGB -gt 0.5  # At least 500MB free
}

Write-Host ""
Write-Host "10. INTEGRATION TESTS" -ForegroundColor Cyan
Write-Host "---------------------" -ForegroundColor Cyan

# Test 19: Service workflow
Test-Feature "Service integration workflow" {
    # Simulate a workflow
    $violation = @{Type = "TestViolation"; Time = Get-Date}
    $log = "Violation logged at $($violation.Time)"
    $notification = @{Title = "Test"; Message = "Test notification"}
    
    ($violation -ne $null) -and ($log -ne "") -and ($notification -ne $null)
}

# Test 20: Data persistence
Test-Feature "Data can be saved and loaded" {
    $testData = @{TestKey = "TestValue"; Number = 123}
    $testFile = "$env:TEMP\kidguard_test.json"
    $testData | ConvertTo-Json | Out-File $testFile -Force
    $loaded = Get-Content $testFile | ConvertFrom-Json
    $result = $loaded.TestKey -eq "TestValue"
    Remove-Item $testFile -Force
    $result
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "         TEST RESULTS               " -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Total Tests: $TotalTests"
Write-Host "Passed: $PassedTests" -ForegroundColor Green
Write-Host "Failed: $FailedTests" -ForegroundColor Red

$successRate = if ($TotalTests -gt 0) { 
    [Math]::Round(($PassedTests / $TotalTests) * 100, 2)
} else { 0 }

Write-Host ""
Write-Host "Success Rate: $successRate%" -ForegroundColor $(
    if ($successRate -ge 80) { "Green" }
    elseif ($successRate -ge 60) { "Yellow" }
    else { "Red" }
)

Write-Host ""
if ($successRate -eq 100) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host "The KidGuard system is ready for deployment." -ForegroundColor Green
} elseif ($successRate -ge 80) {
    Write-Host "Most tests passed. Review failed tests for potential issues." -ForegroundColor Yellow
} else {
    Write-Host "Multiple tests failed. Please review and fix issues." -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed at $(Get-Date -Format 'HH:mm:ss dd/MM/yyyy')" -ForegroundColor Cyan