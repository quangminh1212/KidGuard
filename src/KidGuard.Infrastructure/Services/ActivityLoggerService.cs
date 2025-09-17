using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KidGuard.Core.DTOs;
using KidGuard.Core.Models;
using KidGuard.Core.Services;
using KidGuard.Infrastructure.Data;

namespace KidGuard.Infrastructure.Services
{
    /// <summary>
    /// Implementation of activity logger service
    /// </summary>
    public class ActivityLoggerService : IActivityLoggerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ActivityLoggerService> _logger;

        public ActivityLoggerService(AppDbContext context, ILogger<ActivityLoggerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActivityAsync(ActivityLog activity)
        {
            try
            {
                activity.Timestamp = DateTime.UtcNow;
                _context.ActivityLogs.Add(activity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Activity logged: {activity.Description}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging activity");
            }
        }

        public async Task<List<ActivityLogDto>> GetActivitiesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var activities = await _context.ActivityLogs
                    .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        Type = a.Type.ToString(),
                        Description = a.Description,
                        ApplicationName = a.ApplicationName,
                        Timestamp = a.Timestamp,
                        Duration = a.Duration
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities");
                return new List<ActivityLogDto>();
            }
        }

        public async Task<List<ActivityLogDto>> GetActivitiesByTypeAsync(ActivityType type, DateTime startDate, DateTime endDate)
        {
            try
            {
                var activities = await _context.ActivityLogs
                    .Where(a => a.Type == type && a.Timestamp >= startDate && a.Timestamp <= endDate)
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        Type = a.Type.ToString(),
                        Description = a.Description,
                        ApplicationName = a.ApplicationName,
                        Timestamp = a.Timestamp,
                        Duration = a.Duration
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activities by type {type}");
                return new List<ActivityLogDto>();
            }
        }

        public async Task<List<ActivityLogDto>> GetActivitiesByApplicationAsync(string applicationName, DateTime startDate, DateTime endDate)
        {
            try
            {
                var activities = await _context.ActivityLogs
                    .Where(a => a.ApplicationName == applicationName && 
                               a.Timestamp >= startDate && 
                               a.Timestamp <= endDate)
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => new ActivityLogDto
                    {
                        Id = a.Id,
                        Type = a.Type.ToString(),
                        Description = a.Description,
                        ApplicationName = a.ApplicationName,
                        Timestamp = a.Timestamp,
                        Duration = a.Duration
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activities for application {applicationName}");
                return new List<ActivityLogDto>();
            }
        }

        public async Task<bool> DeleteActivityLogsAsync(DateTime olderThan)
        {
            try
            {
                var logsToDelete = await _context.ActivityLogs
                    .Where(a => a.Timestamp < olderThan)
                    .ToListAsync();

                _context.ActivityLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted {logsToDelete.Count} activity logs older than {olderThan}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity logs");
                return false;
            }
        }

        public async Task<ActivitySummaryDto> GetActivitySummaryAsync(DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var activities = await _context.ActivityLogs
                    .Where(a => a.Timestamp >= startOfDay && a.Timestamp < endOfDay)
                    .ToListAsync();

                var summary = new ActivitySummaryDto
                {
                    Date = date,
                    TotalActivities = activities.Count,
                    TotalScreenTime = TimeSpan.FromHours(activities.Where(a => a.Duration.HasValue)
                        .Sum(a => a.Duration.Value.TotalHours)),
                    ApplicationsUsed = activities.Select(a => a.ApplicationName).Distinct().Count(),
                    WebsitesVisited = activities.Where(a => a.Type == ActivityType.WebsiteVisit).Count(),
                    WarningsCount = activities.Where(a => a.Type == ActivityType.Warning).Count(),
                    MostUsedApp = activities.GroupBy(a => a.ApplicationName)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "None"
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activity summary for {date}");
                return new ActivitySummaryDto { Date = date };
            }
        }
    }
}