using System.Windows.Forms;
using ChildGuard.Core.Configuration;

namespace ChildGuard.UI;

public partial class SettingsForm : Form
{
    private AppConfig _config = new();
    private string _path = string.Empty;

    public SettingsForm()
    {
        InitializeComponent();
    }

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        _config = ConfigManager.Load(out _path);
        chkInput.Checked = _config.EnableInputMonitoring;
        chkActiveWindow.Checked = _config.EnableActiveWindowTracking;
        lblPath.Text = _path;
        // Blocked list
        txtBlocked.Text = _config.BlockedProcesses is { Length: >0 }
            ? string.Join(Environment.NewLine, _config.BlockedProcesses)
            : string.Empty;
        // Quiet hours
        dtStart.Value = ParseTimeOrDefault(_config.QuietHoursStart, new DateTime(2000,1,1,21,0,0));
        dtEnd.Value = ParseTimeOrDefault(_config.QuietHoursEnd, new DateTime(2000,1,1,6,30,0));
        // Allowed during Quiet Hours
        txtAllowedQuiet.Text = _config.AllowedProcessesDuringQuietHours is { Length: >0 }
            ? string.Join(Environment.NewLine, _config.AllowedProcessesDuringQuietHours)
            : string.Empty;
        // Retention
        numRetention.Value = Math.Max(1, _config.LogRetentionDays);
        // Block close warning seconds
        numCloseWarn.Value = Math.Max(0, _config.BlockCloseWarningSeconds);
        // Max log size MB
        numMaxSize.Value = Math.Max(0, _config.LogMaxSizeMB);
    }

    private static DateTime ParseTimeOrDefault(string? s, DateTime fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        if (TimeSpan.TryParseExact(s, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture, out var t))
        {
            return new DateTime(2000,1,1) + t;
        }
        return fallback;
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        _config.EnableInputMonitoring = chkInput.Checked;
        _config.EnableActiveWindowTracking = chkActiveWindow.Checked;
        // Blocked list
        var lines = (txtBlocked.Text ?? string.Empty)
            .Split(new[]{"\r\n","\n"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length>0)
            .Select(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? x[..^4] : x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _config.BlockedProcesses = lines;
        // Quiet hours
        _config.QuietHoursStart = dtStart.Value.ToString("HH:mm");
        _config.QuietHoursEnd = dtEnd.Value.ToString("HH:mm");
        // Allowed during Quiet Hours
        var allowLines = (txtAllowedQuiet.Text ?? string.Empty)
            .Split(new[]{"\r\n","\n"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length>0)
            .Select(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? x[..^4] : x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _config.AllowedProcessesDuringQuietHours = allowLines;
        // Retention
        _config.LogRetentionDays = (int)numRetention.Value;
        // Warning seconds and max size
        _config.BlockCloseWarningSeconds = (int)numCloseWarn.Value;
        _config.LogMaxSizeMB = (int)numMaxSize.Value;

        ConfigManager.Save(_config, out var savedPath);
        lblPath.Text = savedPath;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
