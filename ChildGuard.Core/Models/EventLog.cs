using System;
using System.Text.Json.Serialization;

namespace ChildGuard.Core.Models
{
    /// <summary>
    /// Enum định nghĩa các loại sự kiện trong hệ thống
    /// </summary>
    public enum EventType
    {
        Keystroke,           // Sự kiện gõ phím
        BadWordDetected,     // Phát hiện từ nhạy cảm
        UrlVisited,          // Truy cập URL
        UrlThreat,           // Phát hiện URL nguy hiểm
        AudioCaptured,       // Ghi âm
        ScreenshotCaptured,  // Chụp màn hình
        ProcessStarted,      // Process bắt đầu
        ProcessBlocked,      // Process bị chặn
        System,              // Sự kiện hệ thống
        ApplicationStarted,  // Ứng dụng khởi động
        ApplicationStopped,  // Ứng dụng dừng
        ConfigurationChanged // Thay đổi cấu hình
    }

    /// <summary>
    /// Enum định nghĩa mức độ nghiêm trọng
    /// </summary>
    public enum EventSeverity
    {
        Info,       // Thông tin
        Low,        // Thấp
        Medium,     // Trung bình
        High,       // Cao
        Critical    // Nghiêm trọng
    }

    /// <summary>
    /// Model cho một bản ghi sự kiện
    /// </summary>
    public class EventLog
    {
        public long Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public EventType Type { get; set; }
        public EventSeverity Severity { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? MetaJson { get; set; }
        public string? AttachmentPath { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties cho các attachment
        [JsonIgnore]
        public List<EventAttachment>? Attachments { get; set; }
        
        public EventLog()
        {
            TimestampUtc = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
        
        public EventLog(EventType type, EventSeverity severity, string title, string? content = null)
            : this()
        {
            Type = type;
            Severity = severity;
            Title = title;
            Content = content;
        }
    }

    /// <summary>
    /// Model cho file đính kèm
    /// </summary>
    public class EventAttachment
    {
        public long Id { get; set; }
        public long EventLogId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [JsonIgnore]
        public EventLog? EventLog { get; set; }
        
        public EventAttachment()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Model cho keystroke log
    /// </summary>
    public class KeystrokeLog
    {
        public long Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string KeyData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        public KeystrokeLog()
        {
            TimestampUtc = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Model cho process monitoring
    /// </summary>
    public class ProcessLog
    {
        public long Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessPath { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string Action { get; set; } = string.Empty; // Started, Stopped, Blocked
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public ProcessLog()
        {
            TimestampUtc = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
