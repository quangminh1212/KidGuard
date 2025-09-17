using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KidGuard.Core.DTOs;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for application monitoring service
    /// </summary>
    public interface IApplicationMonitoringService
    {
        Task<List<ApplicationInfoDto>> GetRunningApplicationsAsync();
        Task<bool> BlockApplicationAsync(string processName);
        Task<bool> UnblockApplicationAsync(string processName);
        Task<ApplicationUsageDto> GetApplicationUsageAsync(string processName, DateTime startDate, DateTime endDate);
        Task<bool> SetApplicationTimeLimitAsync(string processName, TimeSpan dailyLimit);
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        event EventHandler<ApplicationEventArgs> ApplicationStarted;
        event EventHandler<ApplicationEventArgs> ApplicationStopped;
    }

    public class ApplicationEventArgs : EventArgs
    {
        public string ProcessName { get; set; }
        public string ApplicationName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}