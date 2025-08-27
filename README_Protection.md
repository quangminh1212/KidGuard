# ChildGuard Protection Suite - Complete Documentation

## 🛡️ Overview
ChildGuard Protection Suite is a comprehensive .NET 8 WinForms application designed to monitor and protect computer usage, particularly for child safety. It combines multiple protection mechanisms to create a safe digital environment.

## 🚀 Key Features

### 1. **Input Monitoring**
- **Keyboard Tracking**: Real-time monitoring of keyboard input
- **Mouse Activity**: Tracks mouse clicks and movements
- **Statistics**: Provides detailed usage statistics

### 2. **Content Protection**
- **Bad Words Detection**: Analyzes typed text for inappropriate content
- **Multi-language Support**: Vietnamese and English bad word detection
- **Custom Word Lists**: Ability to add custom words to filter

### 3. **URL Safety**
- **Real-time URL Checking**: Validates URLs against safety databases
- **Phishing Protection**: Detects malicious and phishing websites
- **Clipboard Monitoring**: Checks URLs copied to clipboard

### 4. **Audio Monitoring** (Experimental)
- **Speech Detection**: Monitors microphone for inappropriate speech
- **Loud Noise Detection**: Alerts on unusually loud sounds
- **FFmpeg Integration**: Uses FFmpeg for audio capture

### 5. **Advanced Protection**
- **Screenshot Detection**: Detects and optionally blocks screenshot attempts
- **Pattern Analysis**: Identifies patterns of inappropriate behavior
- **Window Switching Detection**: Monitors application switching

## 📁 Project Structure

```
ChildGuard/
├── ChildGuard.Core/           # Core business logic
│   ├── Audio/                 # Audio monitoring components
│   │   ├── AudioMonitor.cs
│   │   └── AudioEvents.cs
│   ├── Configuration/         # App configuration
│   │   └── AppConfig.cs
│   ├── Detection/            # Content detection engines
│   │   ├── BadWordsDetector.cs
│   │   └── UrlSafetyChecker.cs
│   └── Models/               # Data models
│
├── ChildGuard.Hooking/        # Windows hooks and monitoring
│   ├── HookManager.cs        # Basic hook management
│   ├── EnhancedHookManager.cs # Enhanced with detection
│   └── AdvancedProtectionManager.cs # Full protection suite
│
├── ChildGuard.UI/            # User interface
│   ├── Form1.cs             # Main application window
│   ├── EnhancedForm1.cs     # Advanced UI with threat display
│   └── SimpleModernLayout.cs # Modern UI layout system
│
└── ChildGuard.Tests/         # Unit tests

```

## 🔧 Configuration

### AppConfig Properties
```csharp
{
    EnableInputMonitoring: true,        // Enable keyboard/mouse monitoring
    EnableAudioMonitoring: false,       // Enable microphone monitoring
    BlockScreenshots: false,            // Block screenshot attempts
    CheckUrls: true,                   // Check URL safety
    BlockInappropriateContent: true,   // Block inappropriate content
    UILanguage: "vi",                  // UI language (vi/en)
    Theme: "System"                    // UI theme (System/Light/Dark)
}
```

## 💻 Technical Implementation

### Protection Manager Hierarchy

1. **HookManager** (Basic)
   - Simple keyboard/mouse hook implementation
   - Basic activity counting

2. **EnhancedHookManager** (Intermediate)
   - Adds content detection
   - URL safety checking
   - Clipboard monitoring

3. **AdvancedProtectionManager** (Full)
   - All enhanced features
   - Audio monitoring integration
   - Pattern analysis
   - Threat level assessment
   - Real-time statistics

### Event System

```csharp
// Threat detection event
protectionManager.OnThreatDetected += (sender, e) => {
    Console.WriteLine($"Threat: {e.Type} - {e.Description}");
    // Severity levels: Low, Medium, High, Critical
};

// Activity monitoring
protectionManager.OnActivity += (sender, e) => {
    // Log all activities
};

// Statistics updates
protectionManager.OnStatisticsUpdated += (sender, e) => {
    UpdateUI(e.TotalKeysPressed, e.TotalMouseClicks, e.ThreatsDetected);
};
```

## 🎨 User Interface

### Main UI Components

1. **Statistics Panel**
   - Real-time keyboard/mouse activity counters
   - Threat detection counter
   - Protection status indicator

2. **Threat Detection Panel**
   - Live threat notifications
   - Color-coded by severity
   - Detailed threat information

3. **Activity Log**
   - Chronological event log
   - Filterable by event type
   - Export capability

4. **Control Panel**
   - Start/Stop protection
   - Enable/disable specific features
   - Quick settings access

## 🚦 Threat Levels

- **Low**: Minor concerns (e.g., mild inappropriate language)
- **Medium**: Moderate threats (e.g., multiple bad words)
- **High**: Serious threats (e.g., dangerous URLs, explicit content)
- **Critical**: Immediate action required (e.g., pattern of dangerous behavior)

## 🔒 Privacy & Security

- **Local Processing**: All detection happens locally
- **No Cloud Storage**: No data sent to external servers
- **Configurable**: Users control what is monitored
- **Transparent**: Open source, auditable code

## 📝 Testing Scenarios

### 1. Bad Word Detection Test
```text
Type: "This is a test with violence and drugs"
Expected: Threat detected - 2 bad words found
```

### 2. URL Safety Test
```text
Paste: "http://phishing-site.example.com"
Expected: Unsafe URL detected
```

### 3. Pattern Detection Test
```text
Type multiple inappropriate messages over time
Expected: Pattern of inappropriate content detected
```

## 🔨 Building & Running

### Prerequisites
- .NET 8 SDK
- Windows 10/11
- Visual Studio 2022 or VS Code
- FFmpeg (optional, for audio monitoring)

### Build Commands
```bash
# Build the solution
dotnet build --configuration Release

# Run the UI application
dotnet run --project ChildGuard.UI

# Run tests
dotnet test
```

## ⚠️ Known Limitations

1. **Audio Monitoring**: Requires FFmpeg and microphone permissions
2. **URL Checking**: Limited to basic pattern matching (no real-time database)
3. **Screenshot Blocking**: Detection only, cannot fully prevent screenshots
4. **Browser URL Detection**: Limited to window title analysis

## 🚀 Future Enhancements

- [ ] Machine Learning-based content detection
- [ ] Cloud-based threat database integration
- [ ] Parent dashboard web interface
- [ ] Mobile app for remote monitoring
- [ ] Screen time management
- [ ] Application usage statistics
- [ ] Content filtering for web browsers
- [ ] Integration with Windows Family Safety

## 📄 License

This project is open source and available under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please read the contributing guidelines before submitting PRs.

## 📞 Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

**Note**: This application is designed for legitimate parental control and child safety purposes. Users are responsible for complying with local laws and regulations regarding monitoring software.
