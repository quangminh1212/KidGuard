using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChildGuard.Core.Data;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Services
{
    /// <summary>
    /// Service Manager - Quản lý singleton của các services và event dispatcher
    /// </summary>
    public class ServiceManager : IDisposable
    {
        private static ServiceManager? _instance;
        private static readonly object _lock = new object();
        
        private readonly EventDispatcher _eventDispatcher;
        private readonly DatabaseInitializer _dbInitializer;
        private readonly IEventRepository _eventRepository;
        private readonly Dictionary<Type, object> _services;
        private bool _isInitialized;
        private bool _disposed;
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ServiceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ServiceManager();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Event Dispatcher chung cho toàn ứng dụng
        /// </summary>
        public IEventDispatcher EventDispatcher => _eventDispatcher;
        
        /// <summary>
        /// Repository để lưu trữ events
        /// </summary>
        public IEventRepository EventRepository => _eventRepository;
        
        private ServiceManager()
        {
            _eventDispatcher = new EventDispatcher(throwOnError: false);
            _dbInitializer = new DatabaseInitializer();
            _eventRepository = new EventRepository(_dbInitializer);
            _services = new Dictionary<Type, object>();
            _isInitialized = false;
        }
        
        /// <summary>
        /// Khởi tạo services và database
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
            
            try
            {
                // Khởi tạo database
                await _dbInitializer.InitializeAsync();
                
                // Đăng ký handler để lưu events vào database
                RegisterEventHandlers();
                
                // Log sự kiện khởi động
                var startEvent = new ApplicationStartedEvent(GetApplicationVersion());
                await _eventDispatcher.PublishAsync(startEvent);
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize ServiceManager", ex);
            }
        }
        
        /// <summary>
        /// Đăng ký service
        /// </summary>
        public void RegisterService<TService>(TService service) where TService : class
        {
            var serviceType = typeof(TService);
            
            if (_services.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Service {serviceType.Name} is already registered");
            }
            
            _services[serviceType] = service;
            
            // Nếu service là IEventSource, gán dispatcher cho nó
            if (service is IEventSource eventSource)
            {
                eventSource.Dispatcher = _eventDispatcher;
            }
        }
        
        /// <summary>
        /// Lấy service đã đăng ký
        /// </summary>
        public TService? GetService<TService>() where TService : class
        {
            var serviceType = typeof(TService);
            
            if (_services.ContainsKey(serviceType))
            {
                return _services[serviceType] as TService;
            }
            
            return null;
        }
        
        /// <summary>
        /// Kiểm tra service đã được đăng ký chưa
        /// </summary>
        public bool IsServiceRegistered<TService>() where TService : class
        {
            return _services.ContainsKey(typeof(TService));
        }
        
        /// <summary>
        /// Đăng ký các event handlers mặc định
        /// </summary>
        private void RegisterEventHandlers()
        {
            // Handler để lưu BadWordDetectedEvent vào database
            _eventDispatcher.Subscribe<BadWordDetectedEvent>(async (evt) =>
            {
                var eventLog = new Models.EventLog(
                    EventType.BadWordDetected,
                    evt.Severity,
                    $"Phát hiện từ nhạy cảm: {evt.Word}",
                    $"Context: {evt.Context}\nWindow: {evt.WindowTitle}\nProcess: {evt.ProcessName}"
                );
                
                await _eventRepository.AddEventAsync(eventLog);
            });
            
            // Handler để lưu UrlDetectedEvent vào database
            _eventDispatcher.Subscribe<UrlDetectedEvent>(async (evt) =>
            {
                var eventType = evt.IsSafe ? EventType.UrlVisited : EventType.UrlThreat;
                var severity = evt.IsSafe ? EventSeverity.Info : EventSeverity.High;
                var title = evt.IsSafe ? $"Truy cập URL: {evt.Url}" : $"Phát hiện URL nguy hiểm: {evt.Url}";
                
                var eventLog = new Models.EventLog(
                    eventType,
                    severity,
                    title,
                    $"Window: {evt.WindowTitle}\nProcess: {evt.ProcessName}\nThreat: {evt.ThreatType ?? "None"}"
                );
                
                await _eventRepository.AddEventAsync(eventLog);
            });
            
            // Handler để lưu ProcessBlockedEvent vào database
            _eventDispatcher.Subscribe<ProcessBlockedEvent>(async (evt) =>
            {
                var eventLog = new Models.EventLog(
                    EventType.ProcessBlocked,
                    EventSeverity.High,
                    $"Chặn process: {evt.ProcessName}",
                    $"Path: {evt.ProcessPath}\nPID: {evt.ProcessId}\nReason: {evt.Reason}"
                );
                
                await _eventRepository.AddEventAsync(eventLog);
            });
            
            // Handler để lưu ScreenshotCapturedEvent vào database
            _eventDispatcher.Subscribe<ScreenshotCapturedEvent>(async (evt) =>
            {
                var eventLog = new Models.EventLog(
                    EventType.ScreenshotCaptured,
                    EventSeverity.Info,
                    "Chụp màn hình",
                    $"Trigger: {evt.TriggerReason}\nSize: {evt.FileSize / 1024}KB\nResolution: {evt.Width}x{evt.Height}"
                )
                {
                    AttachmentPath = evt.FilePath
                };
                
                await _eventRepository.AddEventAsync(eventLog);
            });
            
            // Handler để lưu AudioCapturedEvent vào database
            _eventDispatcher.Subscribe<AudioCapturedEvent>(async (evt) =>
            {
                var eventLog = new Models.EventLog(
                    EventType.AudioCaptured,
                    EventSeverity.Info,
                    "Ghi âm",
                    $"Trigger: {evt.TriggerReason}\nDuration: {evt.DurationSeconds}s\nSize: {evt.FileSize / 1024}KB"
                )
                {
                    AttachmentPath = evt.FilePath
                };
                
                await _eventRepository.AddEventAsync(eventLog);
            });
        }
        
        /// <summary>
        /// Lấy version của ứng dụng
        /// </summary>
        private string GetApplicationVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetName()
                .Version?.ToString() ?? "1.0.0";
        }
        
        /// <summary>
        /// Shutdown và cleanup
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (!_isInitialized)
                return;
            
            try
            {
                // Log sự kiện shutdown
                var stopEvent = new ApplicationStoppedEvent(
                    "User shutdown",
                    DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                );
                
                await _eventDispatcher.PublishAsync(stopEvent);
                
                // Dispose các services
                foreach (var service in _services.Values)
                {
                    if (service is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                
                _services.Clear();
                _eventDispatcher.ClearAllHandlers();
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw
                System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex}");
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
            {
                ShutdownAsync().GetAwaiter().GetResult();
                _eventDispatcher?.Dispose();
            }
            
            _disposed = true;
        }
    }
}
