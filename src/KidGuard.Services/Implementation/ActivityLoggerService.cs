using System.Text;
using System.Text.Json;
using KidGuard.Core.Interfaces;
using KidGuard.Core.Models;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Service for logging and tracking user activities
/// </summary>
public class ActivityLoggerService : IActivityLoggerService
{
    private readonly ILogger<ActivityLoggerService> _logger;
    private readonly List<ActivityLogEntry> _activityLogs = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _logFilePath;
    
    public ActivityLoggerService(ILogger<ActivityLoggerService> logger)
    {
        _logger = logger;
        _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KidGuard", "Logs", "activities.log"
        );
        
        EnsureDirectoryExists();
        LoadActivityLogs();
    }
    
    public async Task LogActivityAsync(ActivityLogEntry entry)
    {
        await _semaphore.WaitAsync();
        try
        {
            entry.UserName = Environment.UserName;
            _activityLogs.Add(entry);
            await AppendToFileAsync(entry);
            
            _logger.LogInformation("Activity logged: {Type} - {Description}", 
                entry.Type, entry.Description);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task LogApplicationLaunchAsync(string processName, string? details = null)
    {
        await LogActivityAsync(new ActivityLogEntry
        {
            Type = ActivityType.ApplicationLaunched,
            Description = $"Application launched: {processName}",
            Details = details,
            ProcessName = processName,
            Severity = LogSeverity.Information
        });
    }
    
    public async Task LogApplicationBlockedAsync(string processName, string reason)
    {
        await LogActivityAsync(new ActivityLogEntry
        {
            Type = ActivityType.ApplicationBlocked,
            Description = $"Application blocked: {processName}",
            Details = $"Reason: {reason}",
            ProcessName = processName,
            Severity = LogSeverity.Warning
        });
    }
    
    public async Task LogWebsiteBlockedAsync(string domain, string category)
    {
        await LogActivityAsync(new ActivityLogEntry
        {
            Type = ActivityType.WebsiteBlocked,
            Description = $"Website blocked: {domain}",
            Details = $"Category: {category}",
            WebsiteUrl = domain,
            Severity = LogSeverity.Information
        });
    }
    
    public async Task LogWebsiteUnblockedAsync(string domain)
    {
        await LogActivityAsync(new ActivityLogEntry
        {
            Type = ActivityType.WebsiteUnblocked,
            Description = $"Website unblocked: {domain}",
            WebsiteUrl = domain,
            Severity = LogSeverity.Information
        });
    }
    
    public async Task LogTimeLimitExceededAsync(string processName, TimeSpan usageTime, TimeSpan limit)
    {
        await LogActivityAsync(new ActivityLogEntry
        {
            Type = ActivityType.TimeLimitExceeded,
            Description = $"Time limit exceeded for {processName}",
            Details = $"Usage: {usageTime:hh\\:mm\\:ss}, Limit: {limit:hh\\:mm\\:ss}",
            ProcessName = processName,
            Severity = LogSeverity.Alert
        });
    }
    
    public async Task<IEnumerable<ActivityLogEntry>> GetActivityLogsAsync(DateTime startDate, DateTime endDate)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _activityLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .OrderByDescending(log => log.Timestamp)
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<IEnumerable<ActivityLogEntry>> GetActivityLogsByTypeAsync(ActivityType type, int maxRecords = 100)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _activityLogs
                .Where(log => log.Type == type)
                .OrderByDescending(log => log.Timestamp)
                .Take(maxRecords)
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<IEnumerable<ActivityLogEntry>> GetRecentActivitiesAsync(int hours = 24)
    {
        var cutoffTime = DateTime.Now.AddHours(-hours);
        await _semaphore.WaitAsync();
        try
        {
            return _activityLogs
                .Where(log => log.Timestamp >= cutoffTime)
                .OrderByDescending(log => log.Timestamp)
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<int> ClearOldLogsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        await _semaphore.WaitAsync();
        try
        {
            var oldLogs = _activityLogs.Where(log => log.Timestamp < cutoffDate).ToList();
            var removedCount = oldLogs.Count;
            
            foreach (var log in oldLogs)
            {
                _activityLogs.Remove(log);
            }
            
            if (removedCount > 0)
            {
                await SaveActivityLogsAsync();
                _logger.LogInformation("Cleared {Count} old activity logs", removedCount);
            }
            
            return removedCount;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> ExportLogsAsync(string filePath, DateTime startDate, DateTime endDate)
    {
        try
        {
            var logs = await GetActivityLogsAsync(startDate, endDate);
            var exportData = new StringBuilder();
            
            exportData.AppendLine("KidGuard Activity Report");
            exportData.AppendLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            exportData.AppendLine(new string('=', 80));
            exportData.AppendLine();
            
            foreach (var log in logs)
            {
                exportData.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Severity}] {log.Type}");
                exportData.AppendLine($"  {log.Description}");
                if (!string.IsNullOrEmpty(log.Details))
                {
                    exportData.AppendLine($"  Details: {log.Details}");
                }
                if (!string.IsNullOrEmpty(log.ProcessName))
                {
                    exportData.AppendLine($"  Process: {log.ProcessName}");
                }
                if (!string.IsNullOrEmpty(log.WebsiteUrl))
                {
                    exportData.AppendLine($"  Website: {log.WebsiteUrl}");
                }
                exportData.AppendLine();
            }
            
            await File.WriteAllTextAsync(filePath, exportData.ToString());
            _logger.LogInformation("Exported {Count} activity logs to {Path}", logs.Count(), filePath);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export activity logs to {Path}", filePath);
            return false;
        }
    }
    
    public async Task<ActivitySummary> GetActivitySummaryAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await GetActivityLogsAsync(startDate, endDate);
        var summary = new ActivitySummary
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalActivities = logs.Count()
        };
        
        foreach (var log in logs)
        {
            switch (log.Type)
            {
                case ActivityType.WebsiteBlocked:
                    summary.BlockedWebsites++;
                    if (!string.IsNullOrEmpty(log.WebsiteUrl))
                    {
                        if (!summary.TopBlockedWebsites.ContainsKey(log.WebsiteUrl))
                            summary.TopBlockedWebsites[log.WebsiteUrl] = 0;
                        summary.TopBlockedWebsites[log.WebsiteUrl]++;
                    }
                    break;
                    
                case ActivityType.ApplicationBlocked:
                    summary.BlockedApplications++;
                    if (!string.IsNullOrEmpty(log.ProcessName))
                    {
                        if (!summary.TopBlockedApplications.ContainsKey(log.ProcessName))
                            summary.TopBlockedApplications[log.ProcessName] = 0;
                        summary.TopBlockedApplications[log.ProcessName]++;
                    }
                    break;
                    
                case ActivityType.TimeLimitExceeded:
                    summary.TimeLimitViolations++;
                    break;
            }
            
            if (log.Severity >= LogSeverity.Alert)
            {
                summary.CriticalEvents.Add(log);
            }
        }
        
        // Sort and limit top blocked items
        summary.TopBlockedWebsites = summary.TopBlockedWebsites
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
        summary.TopBlockedApplications = summary.TopBlockedApplications
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        return summary;
    }
    
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
    
    private void LoadActivityLogs()
    {
        try
        {
            if (File.Exists(_logFilePath))
            {
                var lines = File.ReadAllLines(_logFilePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            var entry = JsonSerializer.Deserialize<ActivityLogEntry>(line);
                            if (entry != null)
                            {
                                _activityLogs.Add(entry);
                            }
                        }
                        catch
                        {
                            // Skip invalid entries
                        }
                    }
                }
                
                _logger.LogInformation("Loaded {Count} activity logs", _activityLogs.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load activity logs");
        }
    }
    
    private async Task SaveActivityLogsAsync()
    {
        try
        {
            var lines = _activityLogs.Select(log => JsonSerializer.Serialize(log));
            await File.WriteAllLinesAsync(_logFilePath, lines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save activity logs");
        }
    }
    
    private async Task AppendToFileAsync(ActivityLogEntry entry)
    {
        try
        {
            var json = JsonSerializer.Serialize(entry);
            await File.AppendAllTextAsync(_logFilePath, json + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append activity log to file");
        }
    }
}