using System;
using System.ComponentModel.DataAnnotations;
using KidGuard.Core.Services;

namespace KidGuard.Core.Models
{
    /// <summary>
    /// Activity log entity model
    /// </summary>
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public ActivityType Type { get; set; }

        [Required]
        public string Description { get; set; }

        public string ApplicationName { get; set; }

        public string ProcessName { get; set; }

        public string WindowTitle { get; set; }

        public string Url { get; set; }

        public DateTime Timestamp { get; set; }

        public TimeSpan? Duration { get; set; }

        public string AdditionalData { get; set; }

        public virtual User User { get; set; }
    }
}