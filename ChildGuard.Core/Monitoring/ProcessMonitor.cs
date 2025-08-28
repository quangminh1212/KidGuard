using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ChildGuard.Core.Data;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChildGuard.Core.Monitoring
{
    public class ProcessMonitor : IDisposable
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<ProcessMonitor> _logger;
        private readonly HashSet<string> _blockedProcesses;
        private readonly HashSet<string> _monitoredProcesses;
        private ManagementEventWatcher? _processStartWatcher;
        private ManagementEventWatcher? _processStopWatcher;
        private System.Threading.Timer? _scanTimer;
        private readonly Dictionary<int, ProcessInfo> _runningProcesses;
        private readonly object _lock = new object();
        private bool _isMonitoring;
        private bool _disposed;

        public class ProcessInfo
        {
            public int ProcessId { get; set; }
            public string ProcessName { get; set; } = string.Empty;
            public string? FilePath { get; set; }
            public DateTime StartTime { get; set; }
            public long MemoryUsage { get; set; }
            public double CpuUsage { get; set; }
            public string? CommandLine { get; set; }
            public string? UserName { get; set; }
        }

        public ProcessMonitor(
            IEventDispatcher eventDispatcher,
            IEventRepository eventRepository,
            ILogger<ProcessMonitor> logger)
        {
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _blockedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _monitoredProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _runningProcesses = new Dictionary<int, ProcessInfo>();
            
            LoadDefaultBlockedProcesses();
        }

        private void LoadDefaultBlockedProcesses()
        {
            // Add default dangerous/unwanted processes
            var defaultBlocked = new[]
            {
                "cmd.exe", "powershell.exe", "pwsh.exe", // Command line tools (optional)
                "regedit.exe", "taskmgr.exe", // System tools
                "tor.exe", "torBrowser.exe", // Privacy tools
                // Games (example)
                "minecraft.exe", "fortnite.exe", "roblox.exe",
                "steam.exe", "epicgameslauncher.exe",
                // Social media apps
                "telegram.exe", "discord.exe",
                // P2P
                "utorrent.exe", "bittorrent.exe", "qbittorrent.exe"
            };

            foreach (var process in defaultBlocked)
            {
                _blockedProcesses.Add(process);
            }
        }

        public void AddBlockedProcess(string processName)
        {
            lock (_lock)
            {
                _blockedProcesses.Add(processName.ToLowerInvariant());
                _logger.LogInformation($"Added blocked process: {processName}");
            }
        }

        public void RemoveBlockedProcess(string processName)
        {
            lock (_lock)
            {
                _blockedProcesses.Remove(processName.ToLowerInvariant());
                _logger.LogInformation($"Removed blocked process: {processName}");
            }
        }

        public void AddMonitoredProcess(string processName)
        {
            lock (_lock)
            {
                _monitoredProcesses.Add(processName.ToLowerInvariant());
                _logger.LogInformation($"Added monitored process: {processName}");
            }
        }

        public IEnumerable<string> GetBlockedProcesses()
        {
            lock (_lock)
            {
                return _blockedProcesses.ToList();
            }
        }

        public IEnumerable<ProcessInfo> GetRunningProcesses()
        {
            lock (_lock)
            {
                return _runningProcesses.Values.ToList();
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring)
            {
                _logger.LogWarning("Process monitoring is already active");
                return;
            }

            _logger.LogInformation("Starting process monitoring");
            _isMonitoring = true;

            try
            {
                // Set up WMI watchers for process events
                SetupWmiWatchers();
                
                // Initial scan of running processes
                ScanRunningProcesses();
                
                // Set up periodic scanning as fallback
                _scanTimer = new System.Threading.Timer(
                    callback: _ => ScanRunningProcesses(),
                    state: null,
                    dueTime: TimeSpan.FromSeconds(10),
                    period: TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start process monitoring");
                _isMonitoring = false;
                throw;
            }
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _logger.LogInformation("Stopping process monitoring");
            _isMonitoring = false;

            _processStartWatcher?.Stop();
            _processStartWatcher?.Dispose();
            _processStartWatcher = null;

            _processStopWatcher?.Stop();
            _processStopWatcher?.Dispose();
            _processStopWatcher = null;

            _scanTimer?.Dispose();
            _scanTimer = null;
        }

        private void SetupWmiWatchers()
        {
            try
            {
                // Watch for process creation
                var startQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _processStartWatcher = new ManagementEventWatcher(startQuery);
                _processStartWatcher.EventArrived += OnProcessStarted;
                _processStartWatcher.Start();

                // Watch for process termination
                var stopQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
                _processStopWatcher = new ManagementEventWatcher(stopQuery);
                _processStopWatcher.EventArrived += OnProcessStopped;
                _processStopWatcher.Start();

                _logger.LogInformation("WMI process watchers initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set up WMI watchers, falling back to polling");
            }
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processName = e.NewEvent.Properties["ProcessName"].Value?.ToString();
                var processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

                if (string.IsNullOrEmpty(processName))
                    return;

                _logger.LogDebug($"Process started: {processName} (PID: {processId})");

                // Check if process should be blocked
                Task.Run(() => HandleNewProcess(processId, processName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling process start event");
            }
        }

        private void OnProcessStopped(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processName = e.NewEvent.Properties["ProcessName"].Value?.ToString();
                var processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

                lock (_lock)
                {
                    _runningProcesses.Remove(processId);
                }

                _logger.LogDebug($"Process stopped: {processName} (PID: {processId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling process stop event");
            }
        }

        private void ScanRunningProcesses()
        {
            try
            {
                var currentProcesses = Process.GetProcesses();
                var currentProcessIds = new HashSet<int>();

                foreach (var process in currentProcesses)
                {
                    try
                    {
                        currentProcessIds.Add(process.Id);
                        
                        lock (_lock)
                        {
                            if (!_runningProcesses.ContainsKey(process.Id))
                            {
                                var processInfo = CreateProcessInfo(process);
                                _runningProcesses[process.Id] = processInfo;
                                
                                // Check if this is a blocked process
                                Task.Run(() => HandleNewProcess(process.Id, process.ProcessName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, $"Error scanning process {process.Id}");
                    }
                }

                // Remove processes that are no longer running
                lock (_lock)
                {
                    var toRemove = _runningProcesses.Keys
                        .Where(pid => !currentProcessIds.Contains(pid))
                        .ToList();
                    
                    foreach (var pid in toRemove)
                    {
                        _runningProcesses.Remove(pid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning running processes");
            }
        }

        private ProcessInfo CreateProcessInfo(Process process)
        {
            var info = new ProcessInfo
            {
                ProcessId = process.Id,
                ProcessName = process.ProcessName
            };

            try
            {
                info.StartTime = process.StartTime;
                info.MemoryUsage = process.WorkingSet64;
                
                // Try to get file path
                try
                {
                    info.FilePath = process.MainModule?.FileName;
                }
                catch { }

                // Try to get command line using WMI
                try
                {
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT CommandLine, UserName FROM Win32_Process WHERE ProcessId = {process.Id}"))
                    {
                        using (var results = searcher.Get())
                        {
                            var mo = results.Cast<ManagementObject>().FirstOrDefault();
                            if (mo != null)
                            {
                                info.CommandLine = mo["CommandLine"]?.ToString();
                                
                                // Get username if available
                                var owner = new string[2];
                                mo.InvokeMethod("GetOwner", owner);
                                info.UserName = owner[0];
                            }
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"Error getting process info for {process.ProcessName}");
            }

            return info;
        }

        private async Task HandleNewProcess(int processId, string processName)
        {
            var isBlocked = false;
            var isMonitored = false;

            lock (_lock)
            {
                isBlocked = _blockedProcesses.Any(blocked => 
                    processName.IndexOf(blocked, StringComparison.OrdinalIgnoreCase) >= 0);
                
                isMonitored = _monitoredProcesses.Any(monitored => 
                    processName.IndexOf(monitored, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (isBlocked)
            {
                _logger.LogWarning($"Blocked process detected: {processName} (PID: {processId})");
                
                // Create event log
                var eventLog = new Models.EventLog
                {
                    Type = EventType.ProcessBlocked,
                    TimestampUtc = DateTime.UtcNow,
                    Title = $"Blocked Process: {processName}",
                    Content = $"Attempted to run blocked process: {processName}",
                    Source = "ProcessMonitor",
                    Severity = EventSeverity.High,
                    MetaJson = JsonConvert.SerializeObject(new
                    {
                        ProcessId = processId,
                        ProcessName = processName,
                        Action = "Blocked"
                    })
                };

                // Save to database and notify
                await _eventRepository.AddEventAsync(eventLog);
                // TODO: Publish event when EventLog implements IEvent
                // await _eventDispatcher.PublishAsync(eventLog);

                // Try to terminate the process
                await Task.Run(() => TerminateProcess(processId, processName));
            }
            else if (isMonitored)
            {
                _logger.LogInformation($"Monitored process started: {processName} (PID: {processId})");
                
                // Create event log for monitoring
                var eventLog = new Models.EventLog
                {
                    Type = EventType.ProcessStarted,
                    TimestampUtc = DateTime.UtcNow,
                    Title = $"Process Started: {processName}",
                    Content = $"Monitored process started: {processName}",
                    Source = "ProcessMonitor",
                    Severity = EventSeverity.Info,
                    MetaJson = JsonConvert.SerializeObject(new
                    {
                        ProcessId = processId,
                        ProcessName = processName
                    })
                };

                await _eventRepository.AddEventAsync(eventLog);
                // TODO: Publish event when EventLog implements IEvent
                // await _eventDispatcher.PublishAsync(eventLog);
            }
        }

        private void TerminateProcess(int processId, string processName)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                // Give the process a chance to close gracefully
                process.CloseMainWindow();
                
                // Wait a moment for graceful closure
                if (!process.WaitForExit(2000))
                {
                    // Force kill if it didn't close
                    process.Kill();
                    _logger.LogWarning($"Force terminated blocked process: {processName} (PID: {processId})");
                }
                else
                {
                    _logger.LogInformation($"Gracefully closed blocked process: {processName} (PID: {processId})");
                }
            }
            catch (ArgumentException)
            {
                // Process already exited
                _logger.LogDebug($"Process {processName} (PID: {processId}) already exited");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to terminate process: {processName} (PID: {processId})");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopMonitoring();
            _disposed = true;
        }
    }
}
