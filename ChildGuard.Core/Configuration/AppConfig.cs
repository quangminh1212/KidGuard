namespace ChildGuard.Core.Configuration;

public class AppConfig
{
    public bool EnableInputMonitoring { get; set; } = false; // Disabled by default for privacy; enable explicitly
    public bool EnableActiveWindowTracking { get; set; } = true;
    public string DataDirectory { get; set; } = "C:/ProgramData/ChildGuard";

    // Simple policy: list of process names to block (case-insensitive, without path, e.g., "game", "chrome")
    public string[] BlockedProcesses { get; set; } = Array.Empty<string>();

    // Quiet hours (local time). If both set (HH:mm), treat as a time range where stricter policy applies.
    public string? QuietHoursStart { get; set; } = null; // e.g., "21:30"
    public string? QuietHoursEnd { get; set; } = null;   // e.g., "06:30"

    // Log retention in days for JSONL files.
    public int LogRetentionDays { get; set; } = 14;

    // Warning countdown before enforcing blocked process (seconds). 0 = close immediately.
    public int BlockCloseWarningSeconds { get; set; } = 10;

    // Maximum total size of logs directory in MB (best-effort cleanup). 0 or negative = unlimited.
    public int LogMaxSizeMB { get; set; } = 200;
}
