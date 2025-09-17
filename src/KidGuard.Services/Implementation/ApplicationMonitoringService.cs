using System.Diagnostics;
using System.Management;
using KidGuard.Core.Interfaces;
using KidGuard.Core.Models;
using Microsoft.Extensions.Logging;

namespace KidGuard.Services.Implementation;

/// <summary>
/// Service for monitoring and controlling applications
/// </summary>
public class ApplicationMonitoringService : IApplicationMonitoringService
{
    private readonly ILogger<ApplicationMonitoringService> _logger;
    private readonly List<MonitoredApplication> _monitoredApplications = new();
    private readonly Dictionary<string, UsageSession> _activeSessions = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public ApplicationMonitoringService(ILogger<ApplicationMonitoringService> logger)
    {
        _logger = logger;
        LoadMonitoredApplications();
    }

    public async Task<IEnumerable<RunningApplication>> GetRunningApplicationsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var runningApps = new List<RunningApplication>();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // Skip system processes and processes without main window
                    if (process.Id == 0 || string.IsNullOrEmpty(process.ProcessName))
                        continue;

                    var app = new RunningApplication
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        StartTime = GetProcessStartTime(process),
                        RunningTime = DateTime.Now - GetProcessStartTime(process),
                        MemoryUsageMB = process.WorkingSet64 / (1024 * 1024)
                    };

                    // Try to get window title for GUI applications
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            app.WindowTitle = process.MainWindowTitle;
                        }
                    }
                    catch { }

                    // Try to get executable path
                    try
                    {
                        app.ExecutablePath = process.MainModule?.FileName;
                        if (!string.IsNullOrEmpty(app.ExecutablePath))
                        {
                            var fileInfo = FileVersionInfo.GetVersionInfo(app.ExecutablePath);
                            app.CompanyName = fileInfo.CompanyName;
                            app.ProductName = fileInfo.ProductName;
                        }
                    }
                    catch { }

                    // Check if monitored
                    var monitored = _monitoredApplications.FirstOrDefault(
                        m => m.ProcessName.Equals(app.ProcessName, StringComparison.OrdinalIgnoreCase));
                    
                    if (monitored != null)
                    {
                        app.IsMonitored = true;
                        app.Status = monitored.Status;
                    }

                    runningApps.Add(app);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error processing {ProcessName}", process.ProcessName);
                }
                finally
                {
                    process.Dispose();
                }
            }

            return runningApps.OrderBy(a => a.ProcessName);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> BlockApplicationAsync(string processName, string reason = "Blocked by parent")
    {
        await _semaphore.WaitAsync();
        try
        {
            var app = _monitoredApplications.FirstOrDefault(
                a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (app == null)
            {
                app = new MonitoredApplication
                {
                    Name = processName,
                    ProcessName = processName,
                    Status = ApplicationStatus.Blocked
                };
                _monitoredApplications.Add(app);
            }
            else
            {
                app.Status = ApplicationStatus.Blocked;
            }

            // Terminate any running instances
            await TerminateApplicationAsync(processName);

            _logger.LogInformation("Blocked application: {ProcessName} - Reason: {Reason}", processName, reason);
            SaveMonitoredApplications();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking application: {ProcessName}", processName);
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UnblockApplicationAsync(string processName)
    {
        await _semaphore.WaitAsync();
        try
        {
            var app = _monitoredApplications.FirstOrDefault(
                a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (app != null)
            {
                app.Status = ApplicationStatus.Allowed;
                SaveMonitoredApplications();
                _logger.LogInformation("Unblocked application: {ProcessName}", processName);
                return true;
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<MonitoredApplication>> GetMonitoredApplicationsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _monitoredApplications.ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddToMonitoringAsync(MonitoredApplication application)
    {
        await _semaphore.WaitAsync();
        try
        {
            var existing = _monitoredApplications.FirstOrDefault(
                a => a.ProcessName.Equals(application.ProcessName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                _monitoredApplications.Add(application);
                SaveMonitoredApplications();
                _logger.LogInformation("Added application to monitoring: {ProcessName}", application.ProcessName);
                return true;
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveFromMonitoringAsync(Guid applicationId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var app = _monitoredApplications.FirstOrDefault(a => a.Id == applicationId);
            if (app != null)
            {
                _monitoredApplications.Remove(app);
                SaveMonitoredApplications();
                _logger.LogInformation("Removed application from monitoring: {ProcessName}", app.ProcessName);
                return true;
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> SetTimeLimitAsync(string processName, TimeSpan dailyLimit)
    {
        await _semaphore.WaitAsync();
        try
        {
            var app = _monitoredApplications.FirstOrDefault(
                a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (app == null)
            {
                app = new MonitoredApplication
                {
                    Name = processName,
                    ProcessName = processName,
                    Status = ApplicationStatus.TimeLimited,
                    DailyTimeLimit = dailyLimit
                };
                _monitoredApplications.Add(app);
            }
            else
            {
                app.Status = ApplicationStatus.TimeLimited;
                app.DailyTimeLimit = dailyLimit;
            }

            SaveMonitoredApplications();
            _logger.LogInformation("Set time limit for {ProcessName}: {Limit}", processName, dailyLimit);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ApplicationUsageStats> GetUsageStatsAsync(string processName, DateTime startDate, DateTime endDate)
    {
        await _semaphore.WaitAsync();
        try
        {
            // TODO: Implement database query for historical data
            var stats = new ApplicationUsageStats
            {
                ProcessName = processName,
                FirstLaunch = startDate,
                LastLaunch = DateTime.Now,
                LaunchCount = 0,
                TotalUsageTime = TimeSpan.Zero
            };

            return stats;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TerminateApplicationAsync(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            var terminated = false;

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    await process.WaitForExitAsync();
                    terminated = true;
                    _logger.LogInformation("Terminated process: {ProcessName} (PID: {ProcessId})", 
                        processName, process.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to terminate process: {ProcessName} (PID: {ProcessId})", 
                        processName, process.Id);
                }
                finally
                {
                    process.Dispose();
                }
            }

            return terminated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating application: {ProcessName}", processName);
            return false;
        }
    }

    public async Task<bool> IsApplicationBlockedAsync(string processName)
    {
        await _semaphore.WaitAsync();
        try
        {
            var app = _monitoredApplications.FirstOrDefault(
                a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
            
            return app?.Status == ApplicationStatus.Blocked;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        if (_monitoringTask != null && !_monitoringTask.IsCompleted)
        {
            _logger.LogWarning("Monitoring is already running");
            return;
        }

        _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _monitoringTask = MonitorApplicationsAsync(_monitoringCts.Token);
        
        _logger.LogInformation("Application monitoring started");
        await Task.CompletedTask;
    }

    public async Task StopMonitoringAsync()
    {
        if (_monitoringCts != null)
        {
            _monitoringCts.Cancel();
            
            if (_monitoringTask != null)
            {
                try
                {
                    await _monitoringTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }

            _monitoringCts.Dispose();
            _monitoringCts = null;
            _monitoringTask = null;
            
            _logger.LogInformation("Application monitoring stopped");
        }
    }

    private async Task MonitorApplicationsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckBlockedApplications();
                await CheckTimeLimits();
                await UpdateUsageSessions();
                
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitoring loop");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }

    private async Task CheckBlockedApplications()
    {
        var blockedApps = _monitoredApplications.Where(a => a.Status == ApplicationStatus.Blocked);
        
        foreach (var app in blockedApps)
        {
            var processes = Process.GetProcessesByName(app.ProcessName);
            foreach (var process in processes)
            {
                try
                {
                    _logger.LogWarning("Terminating blocked application: {ProcessName}", app.ProcessName);
                    process.Kill();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to terminate blocked application: {ProcessName}", app.ProcessName);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        
        await Task.CompletedTask;
    }

    private async Task CheckTimeLimits()
    {
        var timeLimitedApps = _monitoredApplications.Where(a => 
            a.Status == ApplicationStatus.TimeLimited && 
            a.DailyTimeLimit.HasValue);
        
        foreach (var app in timeLimitedApps)
        {
            if (app.TodayUsageTime >= app.DailyTimeLimit.Value)
            {
                await TerminateApplicationAsync(app.ProcessName);
                _logger.LogWarning("Time limit exceeded for {ProcessName}. Today's usage: {Usage}, Limit: {Limit}",
                    app.ProcessName, app.TodayUsageTime, app.DailyTimeLimit.Value);
            }
        }
    }

    private async Task UpdateUsageSessions()
    {
        var runningProcesses = Process.GetProcesses()
            .Where(p => _monitoredApplications.Any(m => 
                m.ProcessName.Equals(p.ProcessName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Start new sessions
        foreach (var process in runningProcesses)
        {
            if (!_activeSessions.ContainsKey(process.ProcessName))
            {
                _activeSessions[process.ProcessName] = new UsageSession
                {
                    ProcessName = process.ProcessName,
                    StartTime = GetProcessStartTime(process)
                };
            }
            process.Dispose();
        }

        // End finished sessions
        var runningNames = runningProcesses.Select(p => p.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var endedSessions = _activeSessions.Where(s => !runningNames.Contains(s.Key)).ToList();
        
        foreach (var session in endedSessions)
        {
            session.Value.EndTime = DateTime.Now;
            _activeSessions.Remove(session.Key);
            
            // Update daily usage time
            var app = _monitoredApplications.FirstOrDefault(a => 
                a.ProcessName.Equals(session.Key, StringComparison.OrdinalIgnoreCase));
            
            if (app != null)
            {
                app.TodayUsageTime = app.TodayUsageTime.Add(session.Value.Duration);
                app.LastAccessTime = session.Value.EndTime;
            }
        }
        
        await Task.CompletedTask;
    }

    private DateTime GetProcessStartTime(Process process)
    {
        try
        {
            return process.StartTime;
        }
        catch
        {
            return DateTime.Now;
        }
    }

    private void LoadMonitoredApplications()
    {
        // TODO: Load from database/file
        _logger.LogInformation("Loaded {Count} monitored applications", _monitoredApplications.Count);
    }

    private void SaveMonitoredApplications()
    {
        // TODO: Save to database/file
        _logger.LogDebug("Saved {Count} monitored applications", _monitoredApplications.Count);
    }
}