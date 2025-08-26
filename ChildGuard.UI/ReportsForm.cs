using System.Text;
using System.Text.Json;
using ChildGuard.Core.Configuration;

namespace ChildGuard.UI;

public partial class ReportsForm : Form
{
    private Dictionary<string,int> _lastCounts = new(StringComparer.OrdinalIgnoreCase);

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
            lblSummary.Text = "Summary: none";
            _lastCounts = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
            pnlChart.Invalidate();
            return;
        }
        var counts = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
        var total = 0;
        var typeFilter = cmbType.SelectedItem?.ToString();
        var procFilter = (txtProcFilter.Text ?? string.Empty).Trim();
        bool hasProcFilter = procFilter.Length > 0;
        foreach (var line in File.ReadLines(path))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString() ?? "?";
                if (typeFilter != null && typeFilter != "All" && !string.Equals(type, typeFilter, StringComparison.OrdinalIgnoreCase)) continue;
                var ts = root.GetProperty("timestamp").GetString() ?? string.Empty;
                var dataElem = root.GetProperty("data");
                if (hasProcFilter)
                {
                    string? pn = null;
                    if (dataElem.ValueKind == JsonValueKind.Object && dataElem.TryGetProperty("processName", out var pnElem))
                    {
                        pn = pnElem.GetString();
                    }
                    if (string.IsNullOrEmpty(pn) || pn?.IndexOf(procFilter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }
                var dataStr = dataElem.ToString();
                grid.Rows.Add(ts, type, dataStr);
                total++;
                // grouping key: by hour or by type
                if (chkByHour.Checked)
                {
                    if (DateTimeOffset.TryParse(ts, out var dto))
                    {
                        var label = dto.ToLocalTime().ToString("HH:00");
                        counts[label] = counts.TryGetValue(label, out var c) ? c+1 : 1;
                    }
                }
                else
                {
                    counts[type] = counts.TryGetValue(type, out var c) ? c+1 : 1;
                }
            }
            catch { }
        }
        var parts = counts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}");
        lblSummary.Text = $"Summary: total={total} | " + string.Join(" | ", parts);
        _lastCounts = counts;
        pnlChart.Invalidate();
    }

    private void pnlChart_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(SystemColors.Control);
        if (_lastCounts == null || _lastCounts.Count == 0) return;
        var rect = e.ClipRectangle;
        int n = _lastCounts.Count;
        int idx = 0;
        int max = Math.Max(1, _lastCounts.Values.Max());
        int barWidth = Math.Max(20, rect.Width / Math.Max(1,n) - 10);
        foreach (var kv in _lastCounts.OrderBy(k => k.Key))
        {
            int x = rect.Left + 10 + idx * (barWidth + 10);
            int h = (int)( (kv.Value / (float)max) * (rect.Height - 30));
            int y = rect.Bottom - 20 - h;
            using var brush = new SolidBrush(Color.FromArgb(80, 120, 200));
            g.FillRectangle(brush, new Rectangle(x, y, barWidth, h));
            using var pen = new Pen(Color.DodgerBlue, 1);
            g.DrawRectangle(pen, new Rectangle(x, y, barWidth, h));
            var label = kv.Key;
            var size = g.MeasureString(label, this.Font);
            g.DrawString(label, this.Font, Brushes.Black, x, rect.Bottom - 18);
            g.DrawString(kv.Value.ToString(), this.Font, Brushes.Black, x, y - 16);
            idx++;
        }
    }

    private void btnExport_Click(object? sender, EventArgs e)
    {
        if (grid.Rows.Count == 0)
        {
            MessageBox.Show(this, "Không có dữ liệu để export.", "ChildGuard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = $"childguard_{dtp.Value:yyyyMMdd}.csv" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8);
            // header
            sw.WriteLine("Timestamp,Type,Data");
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                string ts = Convert.ToString(row.Cells[0].Value) ?? string.Empty;
                string type = Convert.ToString(row.Cells[1].Value) ?? string.Empty;
                string data = Convert.ToString(row.Cells[2].Value) ?? string.Empty;
                // escape quotes and commas in CSV (simple)
                ts = '"' + ts.Replace("\"", "\"\"") + '"';
                type = '"' + type.Replace("\"", "\"\"") + '"';
                data = '"' + data.Replace("\"", "\"\"") + '"';
                sw.WriteLine(string.Join(',', ts, type, data));
            }
            MessageBox.Show(this, "Đã xuất CSV.", "ChildGuard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Lỗi export: {ex.Message}", "ChildGuard", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
