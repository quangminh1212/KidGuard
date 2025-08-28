using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChildGuard.Core.Abstractions;
using ChildGuard.Core.Data;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;
using Microsoft.Extensions.Logging;

namespace ChildGuard.Core.Screenshot
{
    public class ScreenshotService : IDisposable
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<ScreenshotService> _logger;
        private readonly string _screenshotDirectory;
        private System.Threading.Timer? _periodicTimer;
        private bool _isCapturing;
        private readonly object _captureLock = new object();
        private bool _disposed;

        public ScreenshotService(
            IEventDispatcher eventDispatcher,
            IEventRepository eventRepository,
            ILogger<ScreenshotService> logger)
        {
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set up screenshot directory
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _screenshotDirectory = Path.Combine(appDataPath, "ChildGuard", "Screenshots");
            Directory.CreateDirectory(_screenshotDirectory);

            // TODO: Subscribe to threat events when interface is properly configured
            // _eventDispatcher.Subscribe(this);
        }

        public bool CanHandle(EventLog eventLog)
        {
            if (eventLog == null) return false;
            
            // Handle threat events that should trigger screenshot
            return eventLog.Type == EventType.BadWordDetected ||
                   eventLog.Type == EventType.UrlThreat ||
                   eventLog.Type == EventType.ProcessBlocked;
        }

        public async Task HandleAsync(EventLog eventLog)
        {
            if (!CanHandle(eventLog)) return;

            _logger.LogInformation($"Triggering screenshot capture for event: {eventLog.Type}");
            
            // Capture screenshot asynchronously
            await Task.Run(() => CaptureScreenshot($"Threat_{eventLog.Type}"));
        }

        public void StartPeriodicCapture(int intervalMinutes = 5)
        {
            if (_periodicTimer != null)
            {
                _periodicTimer.Dispose();
            }

            _logger.LogInformation($"Starting periodic screenshot capture every {intervalMinutes} minutes");
            
            var interval = TimeSpan.FromMinutes(intervalMinutes);
            _periodicTimer = new System.Threading.Timer(
                callback: _ => CaptureScreenshot("Periodic"),
                state: null,
                dueTime: interval,
                period: interval);
        }

        public void StopPeriodicCapture()
        {
            _logger.LogInformation("Stopping periodic screenshot capture");
            _periodicTimer?.Dispose();
            _periodicTimer = null;
        }

        public string CaptureScreenshot(string reason = "Manual", bool captureAllScreens = true)
        {
            lock (_captureLock)
            {
                if (_isCapturing)
                {
                    _logger.LogWarning("Screenshot capture already in progress, skipping");
                    return string.Empty;
                }
                _isCapturing = true;
            }

            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"Screenshot_{reason}_{timestamp}.png";
                var filePath = Path.Combine(_screenshotDirectory, fileName);

                if (captureAllScreens)
                {
                    CaptureAllScreens(filePath);
                }
                else
                {
                    CapturePrimaryScreen(filePath);
                }

                // Create and save event log
                var screenshotEvent = new EventLog
                {
                    Type = EventType.ScreenshotCaptured,
                    TimestampUtc = DateTime.UtcNow,
                    Title = $"Screenshot Captured - {reason}",
                    Content = $"Screenshot saved to: {fileName}",
                    Source = "ScreenshotService",
                    Severity = EventSeverity.Info,
                    MetaJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        FilePath = filePath,
                        Reason = reason,
                        FileSize = new FileInfo(filePath).Length,
                        ScreenCount = System.Windows.Forms.Screen.AllScreens.Length
                    })
                };

                // Save to database
                Task.Run(async () =>
                {
                    try
                    {
                        await _eventRepository.AddEventAsync(screenshotEvent);
                        // TODO: Publish event when EventLog implements IEvent interface
                        // await _eventDispatcher.PublishAsync(screenshotEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save screenshot event to database");
                    }
                });

                _logger.LogInformation($"Screenshot captured successfully: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing screenshot");
                return string.Empty;
            }
            finally
            {
                lock (_captureLock)
                {
                    _isCapturing = false;
                }
            }
        }

        private void CapturePrimaryScreen(string filePath)
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                }
                
                // Save as PNG with compression
                SaveCompressedPng(bitmap, filePath);
            }
        }

        private void CaptureAllScreens(string filePath)
        {
            // Calculate total bounds for all screens
            var totalBounds = Rectangle.Empty;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                totalBounds = Rectangle.Union(totalBounds, screen.Bounds);
            }

            using (var bitmap = new Bitmap(totalBounds.Width, totalBounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // Capture each screen
                    foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                    {
                        var screenBounds = screen.Bounds;
                        graphics.CopyFromScreen(
                            screenBounds.Location,
                            new Point(screenBounds.Left - totalBounds.Left, screenBounds.Top - totalBounds.Top),
                            screenBounds.Size);
                    }
                }

                // Save as PNG with compression
                SaveCompressedPng(bitmap, filePath);
            }
        }

        private void SaveCompressedPng(Bitmap bitmap, string filePath)
        {
            // Create encoder with compression settings
            var encoder = ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Png.Guid);

            if (encoder != null)
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(
                    System.Drawing.Imaging.Encoder.Compression, 
                    (long)EncoderValue.CompressionLZW);

                bitmap.Save(filePath, encoder, encoderParams);
            }
            else
            {
                // Fallback to standard save
                bitmap.Save(filePath, ImageFormat.Png);
            }
        }

        public Bitmap? GetThumbnail(string filePath, int width = 200, int height = 150)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                using (var originalImage = Image.FromFile(filePath))
                {
                    var thumbnail = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(thumbnail))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    return thumbnail;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating thumbnail for {filePath}");
                return null;
            }
        }

        public long GetTotalStorageSize()
        {
            try
            {
                if (!Directory.Exists(_screenshotDirectory))
                    return 0;

                var directoryInfo = new DirectoryInfo(_screenshotDirectory);
                return directoryInfo.GetFiles("*.png", SearchOption.AllDirectories)
                    .Sum(file => file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating storage size");
                return 0;
            }
        }

        public void CleanupOldScreenshots(int daysToKeep = 7)
        {
            try
            {
                if (!Directory.Exists(_screenshotDirectory))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var directoryInfo = new DirectoryInfo(_screenshotDirectory);
                
                var filesToDelete = directoryInfo.GetFiles("*.png", SearchOption.AllDirectories)
                    .Where(file => file.CreationTime < cutoffDate);

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation($"Deleted old screenshot: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete screenshot: {file.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during screenshot cleanup");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopPeriodicCapture();
            // TODO: Unsubscribe when properly configured
            // _eventDispatcher?.Unsubscribe(this);
            _disposed = true;
        }
    }
}
