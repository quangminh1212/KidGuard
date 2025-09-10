# ChildGuard Deployment Guide

This guide provides comprehensive instructions for deploying ChildGuard in various environments.

## 📋 Prerequisites

### System Requirements
- **Operating System**: Windows 10/11 (64-bit)
- **RAM**: Minimum 4GB, Recommended 8GB
- **Storage**: 500MB free space
- **Permissions**: Administrator privileges required
- **Network**: Internet connection for initial setup (optional)

### Development Requirements
- **Node.js**: Version 18.0 or higher
- **npm**: Version 8.0 or higher
- **Python**: Version 3.8+ (for native modules)
- **Visual Studio Build Tools**: 2019 or later
- **Git**: For source code management

## 🚀 Production Deployment

### 1. Building the Application

```bash
# Clone the repository
git clone https://github.com/your-org/childguard.git
cd childguard

# Install dependencies
npm install

# Build the application
npm run build

# Create installer
npm run dist:win
```

### 2. Installer Creation

The build process creates several installer formats:

```
release/
├── ChildGuard Setup 1.0.0.exe          # NSIS installer
├── ChildGuard-1.0.0-win.zip            # Portable version
└── ChildGuard-1.0.0.msi                # MSI installer
```

### 3. Code Signing (Recommended)

```bash
# Install electron-builder with code signing
npm install --save-dev electron-builder

# Configure code signing in package.json
{
  "build": {
    "win": {
      "certificateFile": "path/to/certificate.p12",
      "certificatePassword": "certificate_password",
      "publisherName": "Your Organization Name"
    }
  }
}

# Build with code signing
npm run dist:win
```

### 4. Distribution Methods

#### 4.1 Direct Download
- Host installer files on secure web server
- Provide checksums for integrity verification
- Use HTTPS for all downloads

#### 4.2 Microsoft Store (Optional)
```bash
# Create MSIX package for Microsoft Store
npm run dist:win -- --publish=never
electron-builder --win --x64 --publish=never
```

#### 4.3 Enterprise Distribution
- Use Group Policy for mass deployment
- Create MSI packages for enterprise environments
- Configure silent installation parameters

## 🏢 Enterprise Deployment

### 1. Silent Installation

```cmd
# Silent installation with default settings
ChildGuard-Setup.exe /S

# Silent installation with custom directory
ChildGuard-Setup.exe /S /D=C:\Program Files\ChildGuard

# MSI silent installation
msiexec /i ChildGuard-1.0.0.msi /quiet /norestart
```

### 2. Group Policy Deployment

#### 2.1 Create GPO
1. Open Group Policy Management Console
2. Create new GPO: "ChildGuard Deployment"
3. Navigate to Computer Configuration > Policies > Software Settings

#### 2.2 Configure Software Installation
1. Right-click "Software installation" > New > Package
2. Select ChildGuard MSI file
3. Choose "Assigned" deployment method
4. Configure installation options

#### 2.3 Security Filtering
- Apply to specific OUs or security groups
- Test deployment on pilot group first
- Monitor deployment status

### 3. Configuration Management

#### 3.1 Default Configuration
Create `config.json` template:

```json
{
  "version": "1.0.0",
  "environment": "production",
  "autoStart": true,
  "minimizeToTray": true,
  "database": {
    "backupEnabled": true,
    "retentionDays": 90
  },
  "security": {
    "sessionTimeout": 30,
    "maxLoginAttempts": 5,
    "passwordMinLength": 8
  },
  "monitoring": {
    "updateInterval": 1000,
    "bufferSize": 1000,
    "maxLogFileSize": 100
  }
}
```

#### 3.2 Registry Configuration
```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\ChildGuard]
"InstallPath"="C:\\Program Files\\ChildGuard"
"AutoStart"=dword:00000001
"MinimizeToTray"=dword:00000001
"LogLevel"="info"
```

## 🔧 Configuration Options

### 1. Installation Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `/S` | Silent installation | N/A |
| `/D=path` | Installation directory | `%ProgramFiles%\ChildGuard` |
| `/AUTOSTART` | Enable auto-start | Enabled |
| `/NOTRAY` | Disable system tray | Disabled |

### 2. Runtime Configuration

#### 2.1 Environment Variables
```cmd
# Set log level
set CHILDGUARD_LOG_LEVEL=debug

# Set data directory
set CHILDGUARD_DATA_DIR=C:\ChildGuardData

# Disable auto-updates
set CHILDGUARD_AUTO_UPDATE=false
```

#### 2.2 Command Line Arguments
```cmd
# Start with specific configuration
ChildGuard.exe --config="C:\custom-config.json"

# Start in debug mode
ChildGuard.exe --debug

# Start minimized
ChildGuard.exe --minimized
```

## 🛡️ Security Considerations

### 1. File Permissions
```cmd
# Set appropriate permissions on installation directory
icacls "C:\Program Files\ChildGuard" /grant "Administrators:(OI)(CI)F"
icacls "C:\Program Files\ChildGuard" /grant "Users:(OI)(CI)RX"

# Secure data directory
icacls "%APPDATA%\ChildGuard" /grant "%USERNAME%:(OI)(CI)F"
icacls "%APPDATA%\ChildGuard" /remove "Users"
```

### 2. Windows Defender Exclusions
```powershell
# Add Windows Defender exclusions
Add-MpPreference -ExclusionPath "C:\Program Files\ChildGuard"
Add-MpPreference -ExclusionProcess "ChildGuard.exe"
Add-MpPreference -ExclusionExtension ".cgdb"
```

### 3. Firewall Configuration
```cmd
# Allow ChildGuard through Windows Firewall
netsh advfirewall firewall add rule name="ChildGuard" dir=in action=allow program="C:\Program Files\ChildGuard\ChildGuard.exe"
```

## 📊 Monitoring and Maintenance

### 1. Health Checks
```powershell
# Check if ChildGuard service is running
Get-Process -Name "ChildGuard" -ErrorAction SilentlyContinue

# Check application logs
Get-EventLog -LogName Application -Source "ChildGuard" -Newest 10
```

### 2. Automated Monitoring
```powershell
# PowerShell script for monitoring
$process = Get-Process -Name "ChildGuard" -ErrorAction SilentlyContinue
if (-not $process) {
    # Restart ChildGuard
    Start-Process "C:\Program Files\ChildGuard\ChildGuard.exe"
    Write-EventLog -LogName Application -Source "ChildGuard Monitor" -EventId 1001 -Message "ChildGuard restarted"
}
```

### 3. Log Management
```cmd
# Rotate logs (run daily)
forfiles /p "%APPDATA%\ChildGuard\logs" /s /m *.log /d -30 /c "cmd /c del @path"

# Compress old logs
powershell Compress-Archive -Path "%APPDATA%\ChildGuard\logs\*.log" -DestinationPath "%APPDATA%\ChildGuard\logs\archive\logs-%date%.zip"
```

## 🔄 Updates and Maintenance

### 1. Automatic Updates
```json
{
  "autoUpdater": {
    "enabled": true,
    "checkInterval": "24h",
    "downloadInBackground": true,
    "installOnQuit": true
  }
}
```

### 2. Manual Updates
```cmd
# Download new version
curl -L -o ChildGuard-Update.exe https://releases.childguard.com/latest/ChildGuard-Setup.exe

# Verify checksum
certutil -hashfile ChildGuard-Update.exe SHA256

# Install update
ChildGuard-Update.exe /S /UPDATE
```

### 3. Rollback Procedures
```cmd
# Backup current installation
xcopy "C:\Program Files\ChildGuard" "C:\Backup\ChildGuard" /E /I /H

# Restore from backup if needed
xcopy "C:\Backup\ChildGuard" "C:\Program Files\ChildGuard" /E /I /H /Y
```

## 🚨 Troubleshooting

### 1. Common Issues

#### Installation Fails
```cmd
# Check Windows Installer service
sc query msiserver

# Repair Windows Installer
msiexec /unregister
msiexec /regserver
```

#### Application Won't Start
```cmd
# Check dependencies
dumpbin /dependents "C:\Program Files\ChildGuard\ChildGuard.exe"

# Run dependency walker
depends.exe "C:\Program Files\ChildGuard\ChildGuard.exe"
```

#### Database Corruption
```cmd
# Backup corrupted database
copy "%APPDATA%\ChildGuard\childguard.db" "%APPDATA%\ChildGuard\childguard.db.backup"

# Reset database (will lose data)
del "%APPDATA%\ChildGuard\childguard.db"
```

### 2. Diagnostic Tools
```powershell
# Generate diagnostic report
& "C:\Program Files\ChildGuard\ChildGuard.exe" --diagnostic --output="C:\Temp\childguard-diagnostic.zip"

# Check system compatibility
& "C:\Program Files\ChildGuard\ChildGuard.exe" --check-system
```

## 📞 Support and Resources

### 1. Log Collection
```cmd
# Collect all logs for support
powershell Compress-Archive -Path "%APPDATA%\ChildGuard\logs\*" -DestinationPath "C:\Temp\childguard-logs.zip"
```

### 2. System Information
```cmd
# Generate system info report
msinfo32 /report "C:\Temp\system-info.txt"

# Export event logs
wevtutil epl Application "C:\Temp\application-events.evtx"
```

### 3. Contact Information
- **Technical Support**: support@childguard.com
- **Documentation**: https://docs.childguard.com
- **Community Forum**: https://community.childguard.com
- **Emergency Support**: +1-800-CHILDGUARD

---

**Note**: This deployment guide should be customized based on your specific organizational requirements and security policies.
