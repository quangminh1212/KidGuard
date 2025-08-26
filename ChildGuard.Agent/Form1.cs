using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Text;
using ChildGuard.Core.Abstractions;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Core.Sinks;
using ChildGuard.Hooking;

namespace ChildGuard.Agent;

public partial class Form1 : Form
{
    private readonly HookManager _hookManager = new();
    private readonly Channel<ActivityEvent> _queue = Channel.CreateUnbounded<ActivityEvent>(new UnboundedChannelOptions{ SingleReader = true, SingleWriter = false });
    private CancellationTokenSource? _cts;
    private Task? _writerTask;
    private IEventSink? _sink;

    private IntPtr _lastWindow = IntPtr.Zero;
    private string _lastTitle = string.Empty;

    private ManagementEventWatcher? _procStart;
    private ManagementEventWatcher? _procStop;

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        // Ẩn cửa sổ, chỉ chạy dưới khay hệ thống
        this.Hide();
        this.ShowInTaskbar = false;
        LoadOrInitConfig();
        activeWindowTimer.Start();
    }

    private AppConfig _config = new();
    private string _configPath = string.Empty;

    private void LoadOrInitConfig()
    {
        _config = ConfigManager.Load(out _configPath);
        // Ensure DataDirectory aligns with chosen config root if empty
        if (string.IsNullOrWhiteSpace(_config.DataDirectory))
        {
            _config.DataDirectory = Path.GetDirectoryName(_configPath) ?? ConfigManager.GetLocalAppDataDir();
        }
    }

    private string GetLogPath()
    {
        var dir = Path.Combine(_config.DataDirectory, "logs");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"events-{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }

    private void StartMonitoring()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        _sink = new JsonlFileEventSink(GetLogPath());
        // Refresh config on start (in case changed)
        LoadOrInitConfig();
        _writerTask = Task.Run(() => WriterLoopAsync(_cts.Token));

        // Hooking
        _hookManager.OnEvent += evt => _queue.Writer.TryWrite(evt);
        _hookManager.Start(_config);

        // Process WMI watchers
        try
        {
            _procStart = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _procStart.EventArrived += (s, e) =>
            {
                try
                {
                    var name = (string?)e.NewEvent.Properties["ProcessName"].Value ?? "";
                    var pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                    _queue.Writer.TryWrite(new ActivityEvent(DateTimeOffset.Now, ActivityEventType.ProcessStart, new { ProcessName = name, ProcessId = pid }));
                }
                catch { }
            };
            _procStart.Start();

            _procStop = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _procStop.EventArrived += (s, e) =>
            {
                try
                {
                    var name = (string?)e.NewEvent.Properties["ProcessName"].Value ?? "";
                    var pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                    _queue.Writer.TryWrite(new ActivityEvent(DateTimeOffset.Now, ActivityEventType.ProcessStop, new { ProcessName = name, ProcessId = pid }));
                }
                catch { }
            };
            _procStop.Start();
        }
        catch { /* WMI có thể bị hạn chế trong một số môi trường */ }

        notifyIcon.BalloonTipTitle = "ChildGuard Agent";
        notifyIcon.BalloonTipText = "Monitoring started";
        notifyIcon.ShowBalloonTip(2000);
    }

    private async Task WriterLoopAsync(CancellationToken ct)
    {
        if (_sink == null) return;
        while (await _queue.Reader.WaitToReadAsync(ct))
        {
            while (_queue.Reader.TryRead(out var evt))
            {
                try { await _sink.WriteAsync(evt, ct); }
                catch { /* swallow to keep agent alive */ }
            }
        }
    }

    private async Task StopMonitoringAsync()
    {
        if (_cts == null) return;
        _hookManager.Stop();

        try { _procStart?.Stop(); } catch { }
        try { _procStop?.Stop(); } catch { }

        _cts.Cancel();
        try { if (_writerTask != null) await _writerTask; } catch { }
        try { if (_sink != null) await _sink.DisposeAsync(); } catch { }
        _sink = null;
        _writerTask = null;
        _cts.Dispose();
        _cts = null;

        notifyIcon.BalloonTipTitle = "ChildGuard Agent";
        notifyIcon.BalloonTipText = "Monitoring stopped";
        notifyIcon.ShowBalloonTip(2000);
    }

    private void mnuStart_Click(object? sender, EventArgs e) => StartMonitoring();
    private async void mnuStop_Click(object? sender, EventArgs e) => await StopMonitoringAsync();
    private async void mnuExit_Click(object? sender, EventArgs e)
    {
        await StopMonitoringAsync();
        notifyIcon.Visible = false;
        Application.Exit();
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_DEVICECHANGE = 0x0219;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        if (m.Msg == WM_DEVICECHANGE)
        {
            if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL)
            {
                _queue.Writer.TryWrite(new ActivityEvent(DateTimeOffset.Now, ActivityEventType.UsbDeviceChange, new { Action = "Arrival" }));
            }
            else if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
            {
                _queue.Writer.TryWrite(new ActivityEvent(DateTimeOffset.Now, ActivityEventType.UsbDeviceChange, new { Action = "Remove" }));
            }
        }
        base.WndProc(ref m);
    }

    private readonly Dictionary<int, DateTime> _lastEnforce = new();

    private void activeWindowTimer_Tick(object? sender, EventArgs e)
    {
        if (!IsHandleCreated) return;
        if (!_config.EnableActiveWindowTracking) return;
        var h = GetForegroundWindow();
        if (h == IntPtr.Zero) return;
        if (h == _lastWindow) return;
        string title = GetWindowTitle(h);
        var (pid, procName) = GetProcessInfo(h);
        _lastWindow = h;
        _lastTitle = title;
        _queue.Writer.TryWrite(new ActivityEvent(DateTimeOffset.Now, ActivityEventType.ActiveWindow, new ActiveWindowInfo(title, procName, pid)));

        // Simple policy enforcement: block configured processes by name (case-insensitive)
        if (_config.BlockedProcesses?.Length > 0 && !string.IsNullOrWhiteSpace(procName))
        {
            var pn = procName.ToLowerInvariant();
            bool isBlocked = _config.BlockedProcesses.Any(x => string.Equals(x.Trim().TrimEnd(".exe".ToCharArray()).ToLowerInvariant(), pn));
            if (isBlocked)
            {
                var now = DateTime.UtcNow;
                if (!_lastEnforce.TryGetValue(pid, out var last) || (now - last) > TimeSpan.FromSeconds(30))
                {
                    _lastEnforce[pid] = now;
                    notifyIcon.BalloonTipTitle = "ChildGuard Policy";
                    notifyIcon.BalloonTipText = $"Ứng dụng '{procName}' bị chặn theo cấu hình.";
                    notifyIcon.ShowBalloonTip(2000);
                    try
                    {
                        using var p = System.Diagnostics.Process.GetProcessById(pid);
                        _ = p.CloseMainWindow();
                    }
                    catch { }
                }
            }
        }
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        var sb = new StringBuilder(1024);
        int len = GetWindowText(hWnd, sb, sb.Capacity);
        return len > 0 ? sb.ToString(0, len) : string.Empty;
    }

    private static (int Pid, string ProcessName) GetProcessInfo(IntPtr hWnd)
    {
        _ = GetWindowThreadProcessId(hWnd, out uint pid);
        try
        {
            using var p = System.Diagnostics.Process.GetProcessById((int)pid);
            return ((int)pid, p.ProcessName);
        }
        catch { return ((int)pid, ""); }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
