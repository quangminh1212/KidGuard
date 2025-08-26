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
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        _config.EnableInputMonitoring = chkInput.Checked;
        _config.EnableActiveWindowTracking = chkActiveWindow.Checked;
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
