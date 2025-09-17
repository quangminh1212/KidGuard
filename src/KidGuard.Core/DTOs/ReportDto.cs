using System;
using System.Collections.Generic;

namespace KidGuard.Core.DTOs
{
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
}