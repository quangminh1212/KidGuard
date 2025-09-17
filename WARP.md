# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

KidGuard is a Windows Forms application built with .NET 9 that provides website blocking functionality through Windows hosts file manipulation. The application helps protect children from inappropriate content by blocking access to specified domains at the system level.

## Common Development Commands

### Build Commands
```bash
# Build the project
dotnet build KidGuardWin

# Run the application (requires Administrator privileges for full functionality)
dotnet run --project KidGuardWin

# Clean build artifacts
dotnet clean KidGuardWin

# Publish for deployment (self-contained for Windows x64)
dotnet publish KidGuardWin -c Release -r win-x64 --self-contained
```

### Testing Commands
```bash
# Run as Administrator (PowerShell)
Start-Process dotnet -ArgumentList "run --project KidGuardWin" -Verb RunAs

# Debug build with detailed output
dotnet build KidGuardWin -v detailed
```

## Architecture

### Project Structure
```
KidGuard/
├── KidGuardWin/              # Main WinForms application
│   ├── Form1.cs              # Main form logic with blocking/unblocking functionality
│   ├── Form1.Designer.cs     # Form UI designer code
│   ├── Program.cs            # Application entry point
│   └── KidGuardWin.csproj    # Project configuration (.NET 9, Windows Forms)
```

### Core Components

1. **Form1.cs**: Main application logic
   - Manages the hosts file through marker-based sections
   - Handles domain blocking/unblocking operations
   - Validates Administrator privileges
   - Maintains blocked domains list

2. **Hosts File Management**:
   - Uses markers `# KIDGUARD_START` and `# KIDGUARD_END` to identify managed sections
   - Blocks domains by adding entries mapping to 127.0.0.1
   - Automatically handles both bare domain and www subdomain
   - Preserves existing hosts file content outside managed section

### Key Implementation Details

- **Administrator Requirement**: The application requires Administrator privileges to modify `C:\Windows\System32\drivers\etc\hosts`
- **Domain Normalization**: Strips protocols and trailing slashes, converts to lowercase
- **Duplicate Prevention**: Automatically removes duplicate entries when blocking
- **UI Language**: Interface uses Vietnamese for user messages
- **Target Framework**: .NET 9.0 Windows Forms application

## Important Considerations

1. **Administrator Privileges**: The application must run as Administrator to modify the hosts file. Without elevation, it operates in read-only mode with a warning.

2. **Hosts File Location**: Hardcoded to `C:\Windows\System32\drivers\etc\hosts` - Windows standard location.

3. **Blocking Method**: Uses localhost redirection (127.0.0.1) to block domains, affecting all browsers and applications system-wide.

4. **Marker System**: The application manages only the content between its markers, preserving any existing hosts file entries.

## Development Workflow

1. Make code changes in the KidGuardWin folder
2. Build with `dotnet build KidGuardWin`
3. Test with Administrator privileges using `Start-Process dotnet -ArgumentList "run --project KidGuardWin" -Verb RunAs`
4. Commit changes directly to main branch (as per user preference)
5. Push to GitHub repository

## Repository Information

- **Remote**: https://github.com/quangminh1212/KidGuard.git
- **Branch**: main (direct development on main branch)
- **Language**: C# with .NET 9.0
- **UI Framework**: Windows Forms