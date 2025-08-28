using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Audio
{
    /// <summary>
    /// Service ghi âm audio khi có sự kiện threat
    /// </summary>
    public class AudioCaptureService : IEventSource, IDisposable
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveWriter;
        private readonly object _lockObject = new object();
        private bool _isRecording;
        private string? _currentRecordingPath;
        private DateTime _recordingStartTime;
        private CancellationTokenSource? _recordingCts;
        
        // Configuration
        private int _recordingDuration = 15; // seconds
        private int _sampleRate = 44100;
        private int _channels = 2;
        private string _outputDirectory;
        
        // Event source implementation
        public string SourceName => "AudioCaptureService";
        public IEventDispatcher? Dispatcher { get; set; }
        
        // Events
        public event EventHandler<AudioCapturedEventArgs>? AudioCaptured;
        public event EventHandler<AudioCaptureErrorEventArgs>? CaptureError;
        
        // Properties
        public bool IsRecording => _isRecording;
        public int RecordingDuration 
        { 
            get => _recordingDuration;
            set => _recordingDuration = Math.Max(1, Math.Min(60, value)); // 1-60 seconds
        }
        
        
        public AudioCaptureService()
        {
            // Default output directory in AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _outputDirectory = Path.Combine(appDataPath, "ChildGuard", "AudioCaptures");
            
            // Ensure directory exists
            Directory.CreateDirectory(_outputDirectory);
        }
        
        /// <summary>
        /// Start recording audio for threat event
        /// </summary>
        public async Task<string?> StartRecordingAsync(string triggerReason, int? durationSeconds = null)
        {
            if (_isRecording)
            {
                OnCaptureError("Recording already in progress");
                return null;
            }
            
            lock (_lockObject)
            {
                if (_isRecording) return null;
                _isRecording = true;
            }
            
            try
            {
                var duration = durationSeconds ?? _recordingDuration;
                _recordingStartTime = DateTime.UtcNow;
                _recordingCts = new CancellationTokenSource();
                
                // Generate filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"audio_capture_{timestamp}_{triggerReason.Replace(" ", "_")}.wav";
                _currentRecordingPath = Path.Combine(_outputDirectory, fileName);
                
                // Initialize recording device
                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(_sampleRate, _channels),
                    BufferMilliseconds = 100,
                    NumberOfBuffers = 3
                };
                
                // Initialize writer
                _waveWriter = new WaveFileWriter(_currentRecordingPath, _waveIn.WaveFormat);
                _waveIn.DataAvailable += (s, e) =>
                {
                    _waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
                };
                
                // Start recording
                _waveIn.StartRecording();
                
                // Log start event
                await PublishEventAsync(new SystemEvent
                {
                    Message = $"Audio recording started: {triggerReason}",
                    Level = EventLevel.Info
                });
                
                // Auto-stop after duration
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(duration * 1000, _recordingCts.Token);
                        await StopRecordingAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        // Recording was stopped manually
                    }
                });
                
                return _currentRecordingPath;
            }
            catch (Exception ex)
            {
                _isRecording = false;
                OnCaptureError($"Failed to start recording: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Stop current recording
        /// </summary>
        public async Task<AudioCaptureResult?> StopRecordingAsync()
        {
            if (!_isRecording)
                return null;
            
            lock (_lockObject)
            {
                if (!_isRecording) return null;
                _isRecording = false;
            }
            
            try
            {
                // Cancel auto-stop timer
                _recordingCts?.Cancel();
                
                // Stop recording
                _waveIn?.StopRecording();
                
                // Close writers
                _waveWriter?.Dispose();
                _waveIn?.Dispose();
                
                _waveWriter = null;
                _waveIn = null;
                
                if (!string.IsNullOrEmpty(_currentRecordingPath) && File.Exists(_currentRecordingPath))
                {
                    var fileInfo = new FileInfo(_currentRecordingPath);
                    var duration = (int)(DateTime.UtcNow - _recordingStartTime).TotalSeconds;
                    
                    var result = new AudioCaptureResult
                    {
                        FilePath = _currentRecordingPath,
                        FileName = Path.GetFileName(_currentRecordingPath),
                        FileSize = fileInfo.Length,
                        DurationSeconds = duration,
                        CapturedAt = _recordingStartTime,
                        Format = "WAV"
                    };
                    
                    // Fire event
                    OnAudioCaptured(result);
                    
                    // Publish to event bus
                    await PublishEventAsync(new AudioCapturedEvent(
                        _currentRecordingPath,
                        "Threat Detection",
                        fileInfo.Length,
                        duration
                    ));
                    
                    return result;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                OnCaptureError($"Error stopping recording: {ex.Message}");
                return null;
            }
            finally
            {
                _currentRecordingPath = null;
                _recordingCts?.Dispose();
                _recordingCts = null;
            }
        }
        
        /// <summary>
        /// Record audio for specific threat event
        /// </summary>
        public async Task<AudioCaptureResult?> RecordThreatAudioAsync(string threatType, string details, int? durationSeconds = null)
        {
            var triggerReason = $"{threatType}_{DateTime.Now:HHmmss}";
            var path = await StartRecordingAsync(triggerReason, durationSeconds);
            
            if (path == null)
                return null;
            
            // Wait for recording to complete
            var duration = durationSeconds ?? _recordingDuration;
            await Task.Delay(duration * 1000);
            
            return await StopRecordingAsync();
        }
        
        /// <summary>
        /// Get list of captured audio files
        /// </summary>
        public List<AudioFileInfo> GetCapturedFiles()
        {
            var files = new List<AudioFileInfo>();
            
            if (!Directory.Exists(_outputDirectory))
                return files;
            
            var audioFiles = Directory.GetFiles(_outputDirectory, "audio_capture_*.*");
            
            foreach (var file in audioFiles)
            {
                var fileInfo = new FileInfo(file);
                files.Add(new AudioFileInfo
                {
                    FilePath = file,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTime,
                    Format = fileInfo.Extension.TrimStart('.')
                });
            }
            
            return files;
        }
        
        /// <summary>
        /// Get total storage used by audio captures
        /// </summary>
        public long GetTotalStorageUsed()
        {
            if (!Directory.Exists(_outputDirectory))
                return 0;
            
            long totalSize = 0;
            var files = Directory.GetFiles(_outputDirectory, "audio_capture_*.*");
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }
            
            return totalSize;
        }
        
        /// <summary>
        /// Clean up old audio files
        /// </summary>
        public async Task<int> CleanupOldFilesAsync(int daysToKeep = 7)
        {
            if (!Directory.Exists(_outputDirectory))
                return 0;
            
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_outputDirectory, "audio_capture_*.*");
            var deletedCount = 0;
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch
                    {
                        // Skip files that can't be deleted
                    }
                }
            }
            
            if (deletedCount > 0)
            {
                await PublishEventAsync(new SystemEvent
                {
                    Message = $"Cleaned up {deletedCount} old audio files",
                    Level = EventLevel.Info
                });
            }
            
            return deletedCount;
        }
        
        /// <summary>
        /// Delete specific audio file
        /// </summary>
        public bool DeleteAudioFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Test audio recording capability
        /// </summary>
        public bool TestAudioDevice()
        {
            try
            {
                using (var waveIn = new WaveInEvent())
                {
                    waveIn.WaveFormat = new WaveFormat(_sampleRate, _channels);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        
        private void OnAudioCaptured(AudioCaptureResult result)
        {
            var args = new AudioCapturedEventArgs
            {
                Result = result,
                Timestamp = DateTime.UtcNow
            };
            
            AudioCaptured?.Invoke(this, args);
        }
        
        private void OnCaptureError(string message)
        {
            var args = new AudioCaptureErrorEventArgs
            {
                ErrorMessage = message,
                Timestamp = DateTime.UtcNow
            };
            
            CaptureError?.Invoke(this, args);
            
            // Also log to event bus
            PublishEventAsync(new SystemEvent
            {
                Message = $"Audio capture error: {message}",
                Level = EventLevel.Error
            });
        }
        
        private async Task PublishEventAsync(IEvent evt)
        {
            if (Dispatcher != null)
            {
                await Dispatcher.PublishAsync(evt);
            }
        }
        
        public void Dispose()
        {
            if (_isRecording)
            {
                StopRecordingAsync().Wait();
            }
            
            _waveIn?.Dispose();
            _waveWriter?.Dispose();
            _recordingCts?.Dispose();
        }
    }
    
    /// <summary>
    /// Result of audio capture
    /// </summary>
    public class AudioCaptureResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CapturedAt { get; set; }
        public string Format { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Audio file information
    /// </summary>
    public class AudioFileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Format { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Event args for audio captured
    /// </summary>
    public class AudioCapturedEventArgs : EventArgs
    {
        public AudioCaptureResult Result { get; set; } = new AudioCaptureResult();
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Event args for capture error
    /// </summary>
    public class AudioCaptureErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
