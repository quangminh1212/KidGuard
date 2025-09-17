using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KidGuard.Core.DTOs;
using KidGuard.Core.Models;

namespace KidGuard.Core.Services
{
    /// <summary>
    /// Interface for activity logging service
    /// </summary>
    public interface IActivityLoggerService
    {
        Task LogActivityAsync(ActivityLog activity);
        Task<List<ActivityLogDto>> GetActivitiesAsync(DateTime startDate, DateTime endDate);
        Task<List<ActivityLogDto>> GetActivitiesByTypeAsync(ActivityType type, DateTime startDate, DateTime endDate);
        Task<List<ActivityLogDto>> GetActivitiesByApplicationAsync(string applicationName, DateTime startDate, DateTime endDate);
        Task<bool> DeleteActivityLogsAsync(DateTime olderThan);
        Task<ActivitySummaryDto> GetActivitySummaryAsync(DateTime date);
    }

    public enum ActivityType
    {
        ApplicationStart,
        ApplicationStop,
        WebsiteVisit,
        WebsiteBlocked,
        ScreenshotTaken,
        TimeRestrictionActivated,
        SystemStart,
        SystemShutdown,
        Warning
    }
}