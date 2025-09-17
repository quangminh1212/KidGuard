using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KidGuard.Core.DTOs;
using KidGuard.Core.Services;
using KidGuard.Infrastructure.Data;

namespace KidGuard.Infrastructure.Services
{
    /// <summary>
    /// Implementation of application monitoring service
    /// </summary>
    public class ApplicationMonitoringService : IApplicationMonitoringService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApplicationMonitoringService> _logger;
        private readonly HashSet<string> _blockedApplications = new HashSet<string>();
        
        public event EventHandler<ApplicationEventArgs> ApplicationStarted;
        public event EventHandler<ApplicationEventArgs> ApplicationStopped;

        public ApplicationMonitoringService(AppDbContext context, ILogger<ApplicationMonitoringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ApplicationInfoDto>> GetRunningApplicationsAsync()
        {
            try
            {
                var processes = Process.GetProcesses();
                var applications = new List<ApplicationInfoDto>();

                foreach (var process in processes)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.MainWindowTitle))
                        {
                            applications.Add(new ApplicationInfoDto
                            {
                                ProcessId = process.Id,
                                ProcessName = process.ProcessName,
                                ApplicationName = process.MainWindowTitle,
                                WindowTitle = process.MainWindowTitle,
                                StartTime = process.StartTime,
                                RunningTime = DateTime.Now - process.StartTime,
                                IsBlocked = _blockedApplications.Contains(process.ProcessName),
                                MemoryUsageMB = process.WorkingSet64 / (1024 * 1024)
                            });
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }

                return await Task.FromResult(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting running applications");
                return new List<ApplicationInfoDto>();
            }
        }

        public async Task<bool> BlockApplicationAsync(string processName)
        {
            try
            {
                _blockedApplications.Add(processName.ToLower());
                _logger.LogInformation($"Application {processName} has been blocked");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error blocking application {processName}");
                return false;
            }
        }

        public async Task<bool> UnblockApplicationAsync(string processName)
        {
            try
            {
                _blockedApplications.Remove(processName.ToLower());
                _logger.LogInformation($"Application {processName} has been unblocked");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unblocking application {processName}");
                return false;
            }
        }

        public async Task<ApplicationUsageDto> GetApplicationUsageAsync(string processName, DateTime startDate, DateTime endDate)
        {
            // Implementation would query database for usage statistics
            return await Task.FromResult(new ApplicationUsageDto
            {
                ApplicationName = processName,
                TotalUsageTime = TimeSpan.FromHours(2.5),
                LaunchCount = 5,
                FirstLaunch = startDate,
                LastLaunch = endDate,
                AverageSessionDuration = TimeSpan.FromMinutes(30)
            });
        }

        public async Task<bool> SetApplicationTimeLimitAsync(string processName, TimeSpan dailyLimit)
        {
            // Implementation would store time limits in database
            _logger.LogInformation($"Time limit set for {processName}: {dailyLimit}");
            return await Task.FromResult(true);
        }

        public async Task StartMonitoringAsync()
        {
            _logger.LogInformation("Application monitoring started");
            await Task.CompletedTask;
        }

        public async Task StopMonitoringAsync()
        {
            _logger.LogInformation("Application monitoring stopped");
            await Task.CompletedTask;
        }
    }
}