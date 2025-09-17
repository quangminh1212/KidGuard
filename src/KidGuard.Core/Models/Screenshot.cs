using System;
using System.ComponentModel.DataAnnotations;

namespace KidGuard.Core.Models
{
    /// <summary>
    /// Screenshot entity model
    /// </summary>
    public class Screenshot
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string ThumbnailPath { get; set; }

        public DateTime CapturedAt { get; set; }

        public string ActiveApplicationName { get; set; }

        public string ActiveWindowTitle { get; set; }

        public long FileSizeBytes { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool IsDeleted { get; set; }

        public virtual User User { get; set; }
    }
}