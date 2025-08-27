# Audio Monitoring Feature - ChildGuard

## Overview
The Audio Monitoring feature in ChildGuard provides real-time audio analysis and detection of inappropriate content through speech recognition and FFmpeg integration.

## Features

### 1. Real-time Speech Recognition
- **Windows Speech Recognition Integration**: Uses built-in Windows speech recognition for real-time audio transcription
- **Confidence Filtering**: Only processes speech with confidence > 50% to reduce false positives
- **Continuous Monitoring**: Supports continuous recognition mode for uninterrupted monitoring

### 2. Bad Words Detection
- **Custom Word Lists**: Supports adding custom bad words in multiple languages
- **Real-time Analysis**: Immediately analyzes transcribed text for inappropriate content
- **Alert System**: Triggers alerts when inappropriate content is detected

### 3. Audio Level Monitoring
- **NAudio Integration**: Real-time audio level monitoring using NAudio library
- **Volume Threshold Detection**: Configurable volume threshold for detecting loud noises
- **Visual Feedback**: Progress bar showing current audio levels

### 4. FFmpeg Support (Optional)
- **Audio Capture**: Can capture system audio or microphone input
- **File Recording**: Saves audio clips for later analysis
- **Flexible Configuration**: Supports various audio capture methods

## Components

### EnhancedAudioMonitor
Main class that orchestrates all audio monitoring features:
- Speech recognition management
- Bad words detection
- Audio level monitoring
- FFmpeg integration

### TestAudioMonitor Application
A Windows Forms application for testing and demonstrating audio monitoring features:
- Start/Stop monitoring controls
- Real-time activity log
- Bad words management interface
- Audio level visualization
- FFmpeg testing capability

## Usage

### Basic Setup
```csharp
// Create monitor instance
var monitor = new EnhancedAudioMonitor();

// Subscribe to events
monitor.OnBadWordDetected += (sender, e) =>
{
    Console.WriteLine($"Bad word detected: {e.TranscribedText}");
    Console.WriteLine($"Words found: {string.Join(", ", e.DetectionResult.DetectedWords)}");
};

monitor.OnSpeechDetected += (sender, e) =>
{
    Console.WriteLine($"Speech: {e.Text} (Confidence: {e.Confidence})");
};

// Add custom bad words
monitor.SetCustomBadWords(new[] { "inappropriate", "word", "list" });

// Start monitoring
await monitor.StartMonitoringAsync();
```

### Running the Test Application
1. Build the project: `dotnet build TestAudioMonitor/TestAudioMonitor.csproj`
2. Run the application: `dotnet run --project TestAudioMonitor/TestAudioMonitor.csproj`

### Test Application Features
- **Start Monitoring**: Begins audio capture and analysis
- **Stop Monitoring**: Stops all audio monitoring
- **Test FFmpeg**: Verifies FFmpeg installation
- **Add Bad Words**: Add custom words to detection list
- **Activity Log**: Shows all detected events
- **Audio Level Bar**: Visual representation of current audio levels
- **Detected List**: Shows all detected inappropriate content

## Configuration Options

### UseWindowsSpeechRecognition
- **Default**: `true`
- **Description**: Enable/disable Windows Speech Recognition

### UseFFmpegCapture
- **Default**: `false`
- **Description**: Enable/disable FFmpeg audio capture

### AudioBufferSeconds
- **Default**: `30`
- **Description**: Duration of audio buffer for FFmpeg capture

### VolumeThreshold
- **Default**: `0.3f`
- **Description**: Volume threshold for detecting significant audio (0.0 - 1.0)

## Requirements

### System Requirements
- Windows 10/11 (for Speech Recognition)
- .NET 8.0 or higher
- Microphone or audio input device

### Dependencies
- **NAudio**: Audio capture and processing
- **System.Speech**: Windows Speech Recognition
- **FFmpeg** (optional): Advanced audio capture

### Installing FFmpeg (Optional)
1. Download FFmpeg from https://ffmpeg.org/download.html
2. Extract to `C:\ffmpeg` or add to system PATH
3. Test installation: `ffmpeg -version`

## Detection Examples

### English Bad Words (Default)
- damn, hell, stupid, idiot, shut up, hate

### Vietnamese Bad Words (For Testing)
- đồ ngu, im đi, chết tiệt, khốn nạn

### Custom Words
You can add any words specific to your monitoring needs through the UI or programmatically.

## Performance Considerations

### CPU Usage
- Speech recognition: ~5-10% CPU
- Audio processing: ~2-5% CPU
- FFmpeg capture: ~3-7% CPU

### Memory Usage
- Base: ~50MB
- With speech recognition: ~100-150MB
- With FFmpeg: Additional ~20-30MB

### Optimization Tips
1. Disable FFmpeg if not needed
2. Adjust volume threshold to reduce processing
3. Use confidence filtering for speech recognition
4. Clear detected phrases periodically

## Troubleshooting

### Speech Recognition Not Working
1. Ensure Windows Speech Recognition is installed
2. Check microphone permissions
3. Verify audio input device is working
4. Try running as administrator

### FFmpeg Issues
1. Verify FFmpeg is installed and in PATH
2. Check audio device names in Windows
3. Try different capture methods (DirectShow vs WASAPI)
4. Ensure proper permissions for audio capture

### No Audio Detection
1. Check volume levels
2. Verify microphone is not muted
3. Test with the Windows Sound Recorder
4. Adjust volume threshold settings

## Security Considerations

### Privacy
- Audio is processed locally, no cloud services required
- Temporary audio files are automatically deleted
- No audio is stored permanently unless explicitly configured

### Permissions
- Requires microphone access permission
- May require administrator rights for system audio capture
- Ensure proper consent before monitoring

## Future Enhancements

### Planned Features
1. Cloud-based speech recognition integration
2. Multi-language support expansion
3. Machine learning for improved detection
4. Audio fingerprinting for content identification
5. Network streaming support
6. Mobile device audio monitoring

### API Integrations
- Google Cloud Speech-to-Text
- Azure Cognitive Services
- Amazon Transcribe
- IBM Watson Speech to Text

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review the test application for examples
3. Submit issues to the GitHub repository
4. Contact support team

## License
Part of the ChildGuard protection suite. See main LICENSE file for details.
