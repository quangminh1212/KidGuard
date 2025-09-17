using KidGuard.Core.Models;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Interface for logging and tracking user activities
/// </summary>
public interface IActivityLoggerService
{
    /// <summary>
    /// Logs an activity entry
    /// </summary>
    Task LogActivityAsync(ActivityLogEntry entry);
    
    /// <summary>
    /// Logs an application launch
    /// </summary>
    Task LogApplicationLaunchAsync(string processName, string? details = null);
    
    /// <summary>
    /// Logs an application block event
    /// </summary>
    Task LogApplicationBlockedAsync(string processName, string reason);
    
    /// <summary>
    /// Logs a website block event
    /// </summary>
    Task LogWebsiteBlockedAsync(string domain, string category);
    
    /// <summary>
    /// Logs a website unblock event
    /// </summary>
    Task LogWebsiteUnblockedAsync(string domain);
    
    /// <summary>
    /// Logs when time limit is exceeded
    /// </summary>
    Task LogTimeLimitExceededAsync(string processName, TimeSpan usageTime, TimeSpan limit);
    
    /// <summary>
    /// Gets activity logs for a date range
    /// </summary>
    Task<IEnumerable<ActivityLogEntry>> GetActivityLogsAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets activity logs filtered by type
    /// </summary>
    Task<IEnumerable<ActivityLogEntry>> GetActivityLogsByTypeAsync(ActivityType type, int maxRecords = 100);
    
    /// <summary>
    /// Gets recent activities
    /// </summary>
    Task<IEnumerable<ActivityLogEntry>> GetRecentActivitiesAsync(int hours = 24);
    
    /// <summary>
    /// Clears old activity logs
    /// </summary>
    Task<int> ClearOldLogsAsync(int daysToKeep = 30);
    
    /// <summary>
    /// Exports activity logs to a file
    /// </summary>
    Task<bool> ExportLogsAsync(string filePath, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets activity summary for reporting
    /// </summary>
    Task<ActivitySummary> GetActivitySummaryAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Summary of activities for reporting
/// </summary>
public class ActivitySummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalActivities { get; set; }
    public int BlockedWebsites { get; set; }
    public int BlockedApplications { get; set; }
    public int TimeLimitViolations { get; set; }
    public Dictionary<string, int> TopBlockedWebsites { get; set; } = new();
    public Dictionary<string, int> TopBlockedApplications { get; set; } = new();
    public Dictionary<string, TimeSpan> ApplicationUsageTime { get; set; } = new();
    public List<ActivityLogEntry> CriticalEvents { get; set; } = new();
}