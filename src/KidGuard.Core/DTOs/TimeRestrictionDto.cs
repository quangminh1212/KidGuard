using System;

namespace KidGuard.Core.DTOs
{
    public class TimeRestrictionDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string MaxDuration { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}