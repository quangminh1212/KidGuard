namespace KidGuard.Core.Models;

/// <summary>
/// Represents a currently running application
/// </summary>
public class RunningApplication
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? WindowTitle { get; set; }
    public string? ExecutablePath { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan RunningTime { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public string? CompanyName { get; set; }
    public string? ProductName { get; set; }
    public bool IsMonitored { get; set; }
    public ApplicationStatus Status { get; set; }
}

/// <summary>
/// Application usage statistics
/// </summary>
public class ApplicationUsageStats
{
    public string ProcessName { get; set; } = string.Empty;
    public TimeSpan TotalUsageTime { get; set; }
    public int LaunchCount { get; set; }
    public DateTime FirstLaunch { get; set; }
    public DateTime LastLaunch { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
    public TimeSpan LongestSession { get; set; }
    public Dictionary<DateTime, TimeSpan> DailyUsage { get; set; } = new();
    public List<UsageSession> Sessions { get; set; } = new();
}

/// <summary>
/// Represents a single usage session of an application
/// </summary>
public class UsageSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProcessName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
    public bool WasTerminated { get; set; }
    public string? TerminationReason { get; set; }
}

/// <summary>
/// Activity log entry
/// </summary>
public class ActivityLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? UserName { get; set; }
    public string? ProcessName { get; set; }
    public string? WebsiteUrl { get; set; }
    public LogSeverity Severity { get; set; } = LogSeverity.Information;
}

/// <summary>
/// Type of activity being logged
/// </summary>
public enum ActivityType
{
    ApplicationLaunched,
    ApplicationBlocked,
    ApplicationTerminated,
    WebsiteBlocked,
    WebsiteUnblocked,
    TimeLimitExceeded,
    SystemStartup,
    SystemShutdown,
    SettingsChanged,
    ParentalOverride
}

/// <summary>
/// Severity level for log entries
/// </summary>
public enum LogSeverity
{
    Information,
    Warning,
    Alert,
    Critical
}