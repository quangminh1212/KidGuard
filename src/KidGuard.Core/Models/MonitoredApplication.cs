namespace KidGuard.Core.Models;

/// <summary>
/// Represents an application that is being monitored
/// </summary>
public class MonitoredApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Allowed;
    public TimeSpan? DailyTimeLimit { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public TimeSpan TodayUsageTime { get; set; } = TimeSpan.Zero;
    public List<TimeRestriction> TimeRestrictions { get; set; } = new();
}

public enum ApplicationStatus
{
    Allowed,
    Blocked,
    TimeLimited,
    Monitoring
}

/// <summary>
/// Represents a time restriction for an application
/// </summary>
public class TimeRestriction
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAllowed { get; set; }
}