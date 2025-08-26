using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;

namespace ChildGuard.UI;

public partial class Form1 : Form
{
    private readonly HookManager _hookManager = new();
    private long _lastKeys;
    private long _lastMouse;
    private volatile bool _running;

    public Form1()
    {
        InitializeComponent();
        uiTimer.Start();
        _hookManager.OnEvent += OnActivity;
    }

    private void mnuSettings_Click(object? sender, EventArgs e)
    {
        using var dlg = new SettingsForm();
        dlg.ShowDialog(this);
    }

    private void mnuReports_Click(object? sender, EventArgs e)
    {
        using var dlg = new ReportsForm();
        dlg.ShowDialog(this);
    }

    private void OnActivity(ActivityEvent evt)
    {
        if (evt.Data is InputActivitySummary s)
        {
            Interlocked.Exchange(ref _lastKeys, s.KeyPressCount);
            Interlocked.Exchange(ref _lastMouse, s.MouseEventCount);
        }
    }

    private void uiTimer_Tick(object? sender, EventArgs e)
    {
        lblKeys.Text = $"KeyPress Count: {_lastKeys}";
        lblMouse.Text = $"Mouse Event Count: {_lastMouse}";
    }

    private void btnStart_Click(object? sender, EventArgs e)
    {
        if (_running) return;
        var cfg = new AppConfig { EnableInputMonitoring = chkEnableInput.Checked };
        _hookManager.Start(cfg);
        _running = true;
    }

    private void btnStop_Click(object? sender, EventArgs e)
    {
        if (!_running) return;
        _hookManager.Stop();
        _running = false;
    }
}
