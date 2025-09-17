using System;
using System.ComponentModel.DataAnnotations;

namespace KidGuard.Core.Models
{
    /// <summary>
    /// Time restriction entity model
    /// </summary>
    public class TimeRestriction
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public TimeSpan? MaxDuration { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual User User { get; set; }
    }

    /// <summary>
    /// Website filter entity model
    /// </summary>
    public class WebsiteFilter
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Domain { get; set; }

        public string Category { get; set; }

        public FilterAction Action { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Notes { get; set; }

        public virtual User User { get; set; }
    }

    public enum FilterAction
    {
        Block,
        Allow,
        Warn
    }

    /// <summary>
    /// Interface for web filtering service
    /// </summary>
    public interface IWebFilteringService
    {
        Task<bool> IsWebsiteBlockedAsync(string url);
        Task<bool> BlockWebsiteAsync(string domain);
        Task<bool> UnblockWebsiteAsync(string domain);
        Task<List<WebsiteFilter>> GetBlockedWebsitesAsync();
        Task<bool> AddCategoryFilterAsync(string category, FilterAction action);
    }
}