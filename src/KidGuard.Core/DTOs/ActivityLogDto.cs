using System;

namespace KidGuard.Core.DTOs
{
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
}