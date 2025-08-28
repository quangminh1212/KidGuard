using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChildGuard.Core.Audio;
using ChildGuard.Core.Detection;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Services
{
    /// <summary>
    /// Service xử lý response khi phát hiện threat
    /// </summary>
    public class ThreatResponseService : IEventSink, IDisposable
    {
        private readonly AudioCaptureService _audioCaptureService;
        private readonly object _lockObject = new object();
        private bool _isEnabled = true;
        
        // Configuration
        public bool EnableAudioCapture { get; set; } = true;
        public bool EnableScreenshot { get; set; } = true;
        public int AudioDurationOnThreat { get; set; } = 10; // seconds
        public int MinThreatSeverityForAudio { get; set; } = 2; // 1=Low, 2=Medium, 3=High
        
        // Event source
        public string SinkName => "ThreatResponseService";
        
        // Events
        public event EventHandler<ThreatResponseEventArgs>? ThreatResponseTriggered;
        
        public ThreatResponseService()
        {
            _audioCaptureService = new AudioCaptureService();
            _audioCaptureService.RecordingDuration = AudioDurationOnThreat;
        }
        
        /// <summary>
        /// Check if this service can handle the event type
        /// </summary>
        public bool CanHandle(Type eventType)
        {
            return eventType == typeof(BadWordDetectedEvent) ||
                   eventType == typeof(UrlThreatEvent) ||
                   eventType == typeof(UrlDetectedEvent) ||
                   eventType == typeof(ProcessBlockedEvent);
        }
        
        /// <summary>
        /// Handle incoming events
        /// </summary>
        public async Task HandleEventAsync(IEvent evt)
        {
            if (!_isEnabled) return;
            
            // Handle BadWordDetectedEvent
            if (evt is BadWordDetectedEvent badWordEvent)
            {
                await HandleBadWordThreatAsync(badWordEvent);
            }
            // Handle UrlThreatEvent
            else if (evt is UrlThreatEvent urlThreatEvent)
            {
                await HandleUrlThreatAsync(urlThreatEvent);
            }
            // Handle UrlDetectedEvent with threat
            else if (evt is UrlDetectedEvent urlDetectedEvent && !urlDetectedEvent.IsSafe)
            {
                await HandleUnsafeUrlAsync(urlDetectedEvent);
            }
            // Handle ProcessBlockedEvent
            else if (evt is ProcessBlockedEvent processBlockedEvent)
            {
                await HandleBlockedProcessAsync(processBlockedEvent);
            }
        }
        
        /// <summary>
        /// Handle bad word threat
        /// </summary>
        private async Task HandleBadWordThreatAsync(BadWordDetectedEvent evt)
        {
            var severity = ConvertSeverity(evt.Severity);
            
            var response = new ThreatResponse
            {
                ThreatType = "BadWord",
                ThreatDetails = $"Detected: {evt.Word}",
                Context = evt.Context,
                WindowTitle = evt.WindowTitle,
                ProcessName = evt.ProcessName,
                Severity = severity,
                Timestamp = evt.Timestamp
            };
            
            // Trigger responses based on severity
            if (severity >= MinThreatSeverityForAudio)
            {
                await TriggerResponseActionsAsync(response);
            }
            
            OnThreatResponseTriggered(response);
        }
        
        /// <summary>
        /// Handle URL threat
        /// </summary>
        private async Task HandleUrlThreatAsync(UrlThreatEvent evt)
        {
            var severity = ParseRiskLevel(evt.RiskLevel);
            
            var response = new ThreatResponse
            {
                ThreatType = "UnsafeUrl",
                ThreatDetails = $"URL: {evt.Domain}",
                Context = evt.ThreatReason,
                WindowTitle = evt.WindowTitle,
                ProcessName = evt.Source,
                Severity = severity,
                Timestamp = evt.Timestamp
            };
            
            // Always trigger response for URL threats
            await TriggerResponseActionsAsync(response);
            
            OnThreatResponseTriggered(response);
        }
        
        /// <summary>
        /// Handle unsafe URL detection
        /// </summary>
        private async Task HandleUnsafeUrlAsync(UrlDetectedEvent evt)
        {
            var response = new ThreatResponse
            {
                ThreatType = "UnsafeUrl",
                ThreatDetails = $"URL: {evt.Url}",
                Context = evt.ThreatType ?? "Potentially unsafe",
                WindowTitle = evt.WindowTitle,
                ProcessName = evt.ProcessName,
                Severity = 2, // Medium by default
                Timestamp = evt.Timestamp
            };
            
            await TriggerResponseActionsAsync(response);
            
            OnThreatResponseTriggered(response);
        }
        
        /// <summary>
        /// Handle blocked process
        /// </summary>
        private async Task HandleBlockedProcessAsync(ProcessBlockedEvent evt)
        {
            var response = new ThreatResponse
            {
                ThreatType = "BlockedProcess",
                ThreatDetails = $"Process: {evt.ProcessName}",
                Context = evt.Reason,
                WindowTitle = evt.ProcessName,
                ProcessName = evt.ProcessName,
                Severity = 3, // High
                Timestamp = evt.Timestamp
            };
            
            // Always trigger response for blocked processes
            await TriggerResponseActionsAsync(response);
            
            OnThreatResponseTriggered(response);
        }
        
        /// <summary>
        /// Trigger response actions based on configuration
        /// </summary>
        private async Task TriggerResponseActionsAsync(ThreatResponse response)
        {
            var tasks = new List<Task>();
            
            // Audio capture
            if (EnableAudioCapture && !_audioCaptureService.IsRecording)
            {
                tasks.Add(CaptureAudioAsync(response));
            }
            
            // Screenshot capture (will be implemented later)
            if (EnableScreenshot)
            {
                // TODO: Implement screenshot capture
                // tasks.Add(CaptureScreenshotAsync(response));
            }
            
            // Wait for all response actions to complete
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
        
        /// <summary>
        /// Capture audio for threat
        /// </summary>
        private async Task CaptureAudioAsync(ThreatResponse response)
        {
            try
            {
                var result = await _audioCaptureService.RecordThreatAudioAsync(
                    response.ThreatType,
                    response.ThreatDetails,
                    AudioDurationOnThreat
                );
                
                if (result != null)
                {
                    response.AudioCapturePath = result.FilePath;
                    response.HasAudioCapture = true;
                    
                    System.Diagnostics.Debug.WriteLine($"[ThreatResponse] Audio captured: {result.FileName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThreatResponse] Audio capture failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Convert EventSeverity to int
        /// </summary>
        private int ConvertSeverity(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Low => 1,
                EventSeverity.Medium => 2,
                EventSeverity.High => 3,
                EventSeverity.Critical => 4,
                _ => 2
            };
        }
        
        /// <summary>
        /// Parse risk level string to int
        /// </summary>
        private int ParseRiskLevel(string riskLevel)
        {
            if (string.IsNullOrEmpty(riskLevel))
                return 2;
            
            return riskLevel.ToLowerInvariant() switch
            {
                "low" => 1,
                "medium" => 2,
                "high" => 3,
                "critical" => 4,
                _ => 2
            };
        }
        
        private void OnThreatResponseTriggered(ThreatResponse response)
        {
            ThreatResponseTriggered?.Invoke(this, new ThreatResponseEventArgs
            {
                Response = response,
                Timestamp = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// Get audio capture statistics
        /// </summary>
        public AudioCaptureStats GetAudioStats()
        {
            var files = _audioCaptureService.GetCapturedFiles();
            var totalSize = _audioCaptureService.GetTotalStorageUsed();
            
            return new AudioCaptureStats
            {
                TotalFiles = files.Count,
                TotalSizeBytes = totalSize,
                OldestFile = files.Count > 0 ? files.Min(f => f.CreatedAt) : null,
                NewestFile = files.Count > 0 ? files.Max(f => f.CreatedAt) : null
            };
        }
        
        /// <summary>
        /// Clean up old audio files
        /// </summary>
        public async Task<int> CleanupOldAudioFilesAsync(int daysToKeep = 7)
        {
            return await _audioCaptureService.CleanupOldFilesAsync(daysToKeep);
        }
        
        /// <summary>
        /// Test audio device
        /// </summary>
        public bool TestAudioDevice()
        {
            return _audioCaptureService.TestAudioDevice();
        }
        
        public void Dispose()
        {
            _audioCaptureService?.Dispose();
        }
    }
    
    /// <summary>
    /// Threat response model
    /// </summary>
    public class ThreatResponse
    {
        public string ThreatType { get; set; } = string.Empty;
        public string ThreatDetails { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int Severity { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Response actions taken
        public bool HasAudioCapture { get; set; }
        public string? AudioCapturePath { get; set; }
        public bool HasScreenshot { get; set; }
        public string? ScreenshotPath { get; set; }
        public bool ProcessKilled { get; set; }
        public bool NotificationSent { get; set; }
    }
    
    /// <summary>
    /// Audio capture statistics
    /// </summary>
    public class AudioCaptureStats
    {
        public int TotalFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public DateTime? OldestFile { get; set; }
        public DateTime? NewestFile { get; set; }
        
        public string FormattedSize => FormatBytes(TotalSizeBytes);
        
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }
    
    /// <summary>
    /// Threat response event args
    /// </summary>
    public class ThreatResponseEventArgs : EventArgs
    {
        public ThreatResponse Response { get; set; } = new ThreatResponse();
        public DateTime Timestamp { get; set; }
    }
}
