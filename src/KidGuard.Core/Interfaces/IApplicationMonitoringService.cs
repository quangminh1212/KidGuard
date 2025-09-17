using KidGuard.Core.Models;

namespace KidGuard.Core.Interfaces;

/// <summary>
/// Interface for monitoring and controlling applications
/// </summary>
public interface IApplicationMonitoringService
{
    /// <summary>
    /// Gets all currently running applications
    /// </summary>
    Task<IEnumerable<RunningApplication>> GetRunningApplicationsAsync();
    
    /// <summary>
    /// Blocks an application from running
    /// </summary>
    Task<bool> BlockApplicationAsync(string processName, string reason = "Blocked by parent");
    
    /// <summary>
    /// Unblocks a previously blocked application
    /// </summary>
    Task<bool> UnblockApplicationAsync(string processName);
    
    /// <summary>
    /// Gets all monitored applications
    /// </summary>
    Task<IEnumerable<MonitoredApplication>> GetMonitoredApplicationsAsync();
    
    /// <summary>
    /// Adds an application to monitoring list
    /// </summary>
    Task<bool> AddToMonitoringAsync(MonitoredApplication application);
    
    /// <summary>
    /// Removes an application from monitoring
    /// </summary>
    Task<bool> RemoveFromMonitoringAsync(Guid applicationId);
    
    /// <summary>
    /// Sets time limit for an application
    /// </summary>
    Task<bool> SetTimeLimitAsync(string processName, TimeSpan dailyLimit);
    
    /// <summary>
    /// Gets usage statistics for an application
    /// </summary>
    Task<ApplicationUsageStats> GetUsageStatsAsync(string processName, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Terminates a running application
    /// </summary>
    Task<bool> TerminateApplicationAsync(string processName);
    
    /// <summary>
    /// Checks if an application is currently blocked
    /// </summary>
    Task<bool> IsApplicationBlockedAsync(string processName);
    
    /// <summary>
    /// Starts monitoring applications continuously
    /// </summary>
    Task StartMonitoringAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Stops monitoring applications
    /// </summary>
    Task StopMonitoringAsync();
}