using System;
using ChildGuard.Core.Detection;

namespace ChildGuard.Core.Audio;

public class SpeechDetectedEventArgs : EventArgs
{
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Confidence { get; set; }
}

public class AudioLevelEventArgs : EventArgs
{
    public double Level { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsLoud => Level > 0.8;
}

public class AudioDetectionEventArgs : EventArgs
{
    public string TranscribedText { get; set; } = string.Empty;
    public DetectionResult DetectionResult { get; set; } = new();
    public string AudioFilePath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
