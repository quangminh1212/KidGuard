# KidGuard - Parental Control Software

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![License](https://img.shields.io/badge/License-MIT-green)

KidGuard is a comprehensive parental control application for Windows that helps parents protect their children from inappropriate content on the internet.

## Features

### ✅ Implemented
- **Website Blocking**: Block access to specific websites or entire categories
- **Category-based Filtering**: Organize blocked sites by categories (Social Media, Gaming, Adult, etc.)
- **Import/Export Block Lists**: Share and backup your blocked website lists
- **Real-time Protection**: Changes take effect immediately
- **User-friendly Dashboard**: Easy-to-use interface with statistics and quick actions

### 🚧 Coming Soon
- **Application Monitoring**: Control which applications can be used
- **Time Management**: Set time limits for internet and application usage
- **Activity Logging**: Monitor and review online activities
- **Scheduled Access**: Configure allowed times for internet access
- **Remote Management**: Control settings from a parent's device

## Requirements

- Windows 10/11
- .NET 9.0 Runtime
- Administrator privileges (required for modifying system hosts file)

## Installation

1. Download the latest release from the [Releases](https://github.com/quangminh1212/KidGuard/releases) page
2. Extract the ZIP file to your preferred location
3. Run `KidGuard.exe` as Administrator

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK

### Build Steps
```bash
# Clone the repository
git clone https://github.com/quangminh1212/KidGuard.git
cd KidGuard

# Build the solution
dotnet build

# Run the application
dotnet run --project src/KidGuard
```

## Usage

1. **Launch KidGuard** with Administrator privileges
2. **Navigate to Website Blocking** tab
3. **Enter a website URL** you want to block (e.g., facebook.com)
4. **Select a category** for organization
5. **Click "Block Website"** to add it to the block list
6. The website will be immediately inaccessible from any browser

### Managing Blocked Websites
- View all blocked websites in the data grid
- Select a website and click "Unblock Selected" to remove restrictions
- Use "Import List" to bulk import websites from a text file
- Use "Export List" to backup your current blocked websites

## Architecture

The project follows clean architecture principles:

```
KidGuard/
├── src/
│   ├── KidGuard/              # WinForms UI Application
│   ├── KidGuard.Core/         # Domain models and interfaces
│   └── KidGuard.Services/     # Business logic implementation
└── tests/                     # Unit and integration tests
```

## Technical Details

- **Framework**: .NET 9.0 with Windows Forms
- **Blocking Method**: Windows hosts file modification
- **Logging**: Serilog with daily rolling file logs
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Architecture**: Clean Architecture with dependency injection

## Security

- Requires Administrator privileges to function
- All modifications are logged for transparency
- Hosts file changes are clearly marked with KidGuard tags
- No data is sent to external servers

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions, please [open an issue](https://github.com/quangminh1212/KidGuard/issues) on GitHub.

## Acknowledgments

- Built with .NET 9.0 and Windows Forms
- Icons from System.Drawing.SystemIcons
- Logging powered by Serilog

---

**Note**: This software is intended for parental control purposes only. Please use responsibly and in accordance with local laws and regulations.