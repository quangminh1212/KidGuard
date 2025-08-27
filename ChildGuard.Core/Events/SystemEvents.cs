using System;
using System.Collections.Generic;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Events
{
    /// <summary>
    /// Base class cho tất cả events trong hệ thống
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public Guid EventId { get; }
        public DateTime Timestamp { get; }
        public string Source { get; }
        
        protected BaseEvent(string source)
        {
            EventId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Source = source ?? "System";
        }
    }
    
    /// <summary>
    /// Event khi phát hiện từ nguy hiểm
    /// </summary>
    public class BadWordDetectedEvent : BaseEvent
    {
        public string Word { get; }
        public string Context { get; }
        public string WindowTitle { get; }
        public string ProcessName { get; }
        public EventSeverity Severity { get; }
        
        public BadWordDetectedEvent(string word, string context, string windowTitle, string processName, EventSeverity severity = EventSeverity.High) 
            : base("BadWordDetector")
        {
            Word = word;
            Context = context;
            WindowTitle = windowTitle;
            ProcessName = processName;
            Severity = severity;
        }
    }
    
    /// <summary>
    /// Event khi phát hiện URL
    /// </summary>
    public class UrlDetectedEvent : BaseEvent
    {
        public string Url { get; }
        public string WindowTitle { get; }
        public string ProcessName { get; }
        public bool IsSafe { get; }
        public string? ThreatType { get; }
        
        public UrlDetectedEvent(string url, string windowTitle, string processName, bool isSafe = true, string? threatType = null)
            : base("UrlDetector")
        {
            Url = url;
            WindowTitle = windowTitle;
            ProcessName = processName;
            IsSafe = isSafe;
            ThreatType = threatType;
        }
    }
    
    /// <summary>
    /// Event khi có keystroke
    /// </summary>
    public class KeystrokeEvent : BaseEvent
    {
        public string Key { get; }
        public string WindowTitle { get; }
        public string ProcessName { get; }
        public bool IsSpecialKey { get; }
        
        public KeystrokeEvent(string key, string windowTitle, string processName, bool isSpecialKey = false)
            : base("KeyboardMonitor")
        {
            Key = key;
            WindowTitle = windowTitle;
            ProcessName = processName;
            IsSpecialKey = isSpecialKey;
        }
    }
    
    /// <summary>
    /// Event khi chụp màn hình
    /// </summary>
    public class ScreenshotCapturedEvent : BaseEvent
    {
        public string FilePath { get; }
        public string TriggerReason { get; }
        public long FileSize { get; }
        public int Width { get; }
        public int Height { get; }
        
        public ScreenshotCapturedEvent(string filePath, string triggerReason, long fileSize, int width, int height)
            : base("ScreenshotService")
        {
            FilePath = filePath;
            TriggerReason = triggerReason;
            FileSize = fileSize;
            Width = width;
            Height = height;
        }
    }
    
    /// <summary>
    /// Event khi ghi âm
    /// </summary>
    public class AudioCapturedEvent : BaseEvent
    {
        public string FilePath { get; }
        public string TriggerReason { get; }
        public long FileSize { get; }
        public int DurationSeconds { get; }
        
        public AudioCapturedEvent(string filePath, string triggerReason, long fileSize, int durationSeconds)
            : base("AudioService")
        {
            FilePath = filePath;
            TriggerReason = triggerReason;
            FileSize = fileSize;
            DurationSeconds = durationSeconds;
        }
    }
    
    /// <summary>
    /// Event khi process bắt đầu
    /// </summary>
    public class ProcessStartedEvent : BaseEvent
    {
        public string ProcessName { get; }
        public string ProcessPath { get; }
        public int ProcessId { get; }
        public bool IsBlocked { get; }
        
        public ProcessStartedEvent(string processName, string processPath, int processId, bool isBlocked = false)
            : base("ProcessMonitor")
        {
            ProcessName = processName;
            ProcessPath = processPath;
            ProcessId = processId;
            IsBlocked = isBlocked;
        }
    }
    
    /// <summary>
    /// Event khi process bị chặn
    /// </summary>
    public class ProcessBlockedEvent : BaseEvent
    {
        public string ProcessName { get; }
        public string ProcessPath { get; }
        public int ProcessId { get; }
        public string Reason { get; }
        
        public ProcessBlockedEvent(string processName, string processPath, int processId, string reason)
            : base("ProcessMonitor")
        {
            ProcessName = processName;
            ProcessPath = processPath;
            ProcessId = processId;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event thông báo chung
    /// </summary>
    public class NotificationEvent : BaseEvent
    {
        public string Title { get; }
        public string Message { get; }
        public EventSeverity Severity { get; }
        public Dictionary<string, object>? Metadata { get; }
        
        public NotificationEvent(string title, string message, EventSeverity severity, Dictionary<string, object>? metadata = null)
            : base("NotificationService")
        {
            Title = title;
            Message = message;
            Severity = severity;
            Metadata = metadata;
        }
    }
    
    /// <summary>
    /// Event khi có thay đổi cấu hình
    /// </summary>
    public class ConfigurationChangedEvent : BaseEvent
    {
        public string ConfigKey { get; }
        public object? OldValue { get; }
        public object? NewValue { get; }
        
        public ConfigurationChangedEvent(string configKey, object? oldValue, object? newValue)
            : base("Configuration")
        {
            ConfigKey = configKey;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
    
    /// <summary>
    /// Event khi ứng dụng khởi động
    /// </summary>
    public class ApplicationStartedEvent : BaseEvent
    {
        public string Version { get; }
        public DateTime StartTime { get; }
        
        public ApplicationStartedEvent(string version)
            : base("Application")
        {
            Version = version;
            StartTime = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Event khi ứng dụng dừng
    /// </summary>
    public class ApplicationStoppedEvent : BaseEvent
    {
        public string Reason { get; }
        public TimeSpan RunDuration { get; }
        
        public ApplicationStoppedEvent(string reason, TimeSpan runDuration)
            : base("Application")
        {
            Reason = reason;
            RunDuration = runDuration;
        }
    }
}
