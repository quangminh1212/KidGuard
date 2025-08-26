using System.Text.Json;
using ChildGuard.Core.Configuration;

namespace ChildGuard.UI;

public partial class ReportsForm : Form
{
    public ReportsForm()
    {
        InitializeComponent();
    }

    private void ReportsForm_Load(object? sender, EventArgs e)
    {
        cmbType.Items.AddRange(new object[]{"All","Keyboard","Mouse","ActiveWindow","ProcessStart","ProcessStop","UsbDeviceChange"});
        cmbType.SelectedIndex = 0;
        dtp.Value = DateTime.Today;
    }

    private void btnLoad_Click(object? sender, EventArgs e)
    {
        grid.Rows.Clear();
        var cfg = ConfigManager.Load(out _);
        var path = Path.Combine(cfg.DataDirectory, "logs", $"events-{dtp.Value:yyyyMMdd}.jsonl");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Không tìm thấy log: {path}", "ChildGuard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var filter = cmbType.SelectedItem?.ToString();
        foreach (var line in File.ReadLines(path))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString();
                if (filter != null && filter != "All" && !string.Equals(type, filter, StringComparison.OrdinalIgnoreCase)) continue;
                var ts = root.GetProperty("timestamp").GetString();
                var data = root.GetProperty("data").ToString();
                grid.Rows.Add(ts, type, data);
            }
            catch { }
        }
    }
}
