namespace ChildGuard.Core.Configuration;

public class AppConfig
{
    public bool EnableInputMonitoring { get; set; } = false; // Disabled by default for privacy; enable explicitly
    public bool EnableActiveWindowTracking { get; set; } = true;
    public string DataDirectory { get; set; } = "C:/ProgramData/ChildGuard";

    // Simple policy: list of process names to block (case-insensitive, without path, e.g., "game", "chrome")
    public string[] BlockedProcesses { get; set; } = Array.Empty<string>();
}
