using System;

namespace KidGuard.Core.DTOs
{
    public class ApplicationInfoDto
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string ApplicationName { get; set; }
        public string WindowTitle { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan RunningTime { get; set; }
        public bool IsBlocked { get; set; }
        public TimeSpan? TimeLimit { get; set; }
        public string Category { get; set; }
        public long MemoryUsageMB { get; set; }
    }

    public class ApplicationUsageDto
    {
        public string ApplicationName { get; set; }
        public TimeSpan TotalUsageTime { get; set; }
        public int LaunchCount { get; set; }
        public DateTime FirstLaunch { get; set; }
        public DateTime LastLaunch { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
    }

    public class ActivityLogDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string ApplicationName { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

    public class ActivitySummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalActivities { get; set; }
        public TimeSpan TotalScreenTime { get; set; }
        public int ApplicationsUsed { get; set; }
        public int WebsitesVisited { get; set; }
        public int WarningsCount { get; set; }
        public string MostUsedApp { get; set; }
    }

    public class TimeRestrictionDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string MaxDuration { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }

    public class TimeUsageDto
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalUsage { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsOverLimit { get; set; }
    }

    public class ReportDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan TotalScreenTime { get; set; }
        public int TotalApplications { get; set; }
        public int TotalWebsites { get; set; }
        public int TotalWarnings { get; set; }
        public List<ApplicationUsageDto> TopApplications { get; set; }
        public List<string> BlockedWebsites { get; set; }
        public Dictionary<string, double> CategoryDistribution { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}