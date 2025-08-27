using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Data
{
    /// <summary>
    /// Interface định nghĩa các phương thức truy xuất dữ liệu sự kiện
    /// </summary>
    public interface IEventRepository
    {
        // Event operations
        Task<long> AddEventAsync(EventLog eventLog);
        Task<EventLog?> GetEventByIdAsync(long id);
        Task<IEnumerable<EventLog>> GetEventsAsync(DateTime? fromDate = null, DateTime? toDate = null, EventType? type = null, EventSeverity? severity = null);
        Task<IEnumerable<EventLog>> GetRecentEventsAsync(int count = 100);
        Task<IEnumerable<EventLog>> GetUnreadEventsAsync();
        Task<bool> MarkEventAsReadAsync(long id);
        Task<bool> MarkAllEventsAsReadAsync();
        Task<bool> DeleteEventAsync(long id);
        Task<int> DeleteOldEventsAsync(DateTime olderThan);
        
        // Attachment operations
        Task<long> AddAttachmentAsync(EventAttachment attachment);
        Task<IEnumerable<EventAttachment>> GetAttachmentsByEventIdAsync(long eventId);
        Task<bool> DeleteAttachmentAsync(long id);
        
        // Keystroke operations
        Task<long> AddKeystrokeAsync(KeystrokeLog keystroke);
        Task<IEnumerable<KeystrokeLog>> GetKeystrokesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> DeleteOldKeystrokesAsync(DateTime olderThan);
        
        // Process operations
        Task<long> AddProcessLogAsync(ProcessLog processLog);
        Task<IEnumerable<ProcessLog>> GetProcessLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null);
        Task<IEnumerable<ProcessLog>> GetBlockedProcessesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        
        // Statistics operations
        Task<Dictionary<EventType, int>> GetEventCountsByTypeAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<EventSeverity, int>> GetEventCountsBySeverityAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalEventCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetThreatCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        
        // Search operations
        Task<IEnumerable<EventLog>> SearchEventsAsync(string searchTerm, int maxResults = 100);
        
        // Cleanup operations
        Task<long> GetDatabaseSizeAsync();
        Task<int> CleanupOldDataAsync(int daysToKeep);
    }
}
