using System.Text;
using System.Text.Json;
using ChildGuard.Core.Configuration;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.Windows.Forms.Layout;

namespace ChildGuard.UI;

public partial class ReportsForm : Form
{
    private Dictionary<string,int> _lastCounts = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<DateTime,int> _trendCounts = new();

public ReportsForm()
    {
        InitializeComponent();
        // Theme applied after loading config
    }

    private void ReportsForm_Load(object? sender, EventArgs e)
    {
        var cfg = ConfigManager.Load(out _);
        UIStrings.SetLanguage(cfg.UILanguage);
        ApplyLocalization();
        var theme = ParseTheme(cfg.Theme);
        ModernStyle.Apply(this, theme);
        RebuildLayoutModern(theme);

        // Combo items with display labels but stable keys for logic
        cmbType.DisplayMember = "Value"; // label
        cmbType.ValueMember = "Key";     // actual key used in logic
        cmbType.Items.Clear();
        cmbType.Items.Add(new KeyValuePair<string,string>("All", UIStrings.Get("Reports.Event.All")));
        cmbType.Items.Add(new KeyValuePair<string,string>("Keyboard", UIStrings.Get("Reports.Event.Keyboard")));
        cmbType.Items.Add(new KeyValuePair<string,string>("Mouse", UIStrings.Get("Reports.Event.Mouse")));
        cmbType.Items.Add(new KeyValuePair<string,string>("ActiveWindow", UIStrings.Get("Reports.Event.ActiveWindow")));
        cmbType.Items.Add(new KeyValuePair<string,string>("ProcessStart", UIStrings.Get("Reports.Event.ProcessStart")));
        cmbType.Items.Add(new KeyValuePair<string,string>("ProcessStop", UIStrings.Get("Reports.Event.ProcessStop")));
        cmbType.Items.Add(new KeyValuePair<string,string>("UsbDeviceChange", UIStrings.Get("Reports.Event.UsbDeviceChange")));
        cmbType.SelectedIndex = 0;
        dtp.Value = DateTime.Today;
        dtpTo.Value = DateTime.Today;
        // default time filter window 00:00 - 23:59
        try
        {
            dtpTimeFrom.Value = DateTime.Today.AddHours(0);
            dtpTimeTo.Value = DateTime.Today.AddHours(23).AddMinutes(59);
        }
        catch { }
    }

    private static bool InTimeRange(TimeSpan t, TimeSpan from, TimeSpan to)
        => from <= to ? (t >= from && t <= to) : (t >= from || t <= to);

private void btnLoad_Click(object? sender, EventArgs e)
{
    grid.Rows.Clear();
    _trendCounts = new();
    var cfg = ConfigManager.Load(out _);
    var startDate = dtp.Value.Date;
    var endDate = dtpTo.Value.Date;
    if (endDate < startDate) endDate = startDate;
    var counts = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
    var total = 0;
    string? typeFilter = (cmbType.SelectedItem is KeyValuePair<string,string> kv)
        ? kv.Key : cmbType.SelectedItem?.ToString();
    var procFilter = (txtProcFilter.Text ?? string.Empty).Trim();
    bool hasProcFilter = procFilter.Length > 0;
    bool applyTime = chkTimeFilter.Checked;
    var timeFrom = dtpTimeFrom.Value.TimeOfDay;
    var timeTo = dtpTimeTo.Value.TimeOfDay;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var path = Path.Combine(cfg.DataDirectory, "logs", $"events-{date:yyyyMMdd}.jsonl");
            if (!File.Exists(path)) continue;
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

                    // time-of-day filter
                    DateTimeOffset dto;
                    bool parsedTs = DateTimeOffset.TryParse(ts, out dto);
                    if (applyTime)
                    {
                        if (!parsedTs) continue; // cannot evaluate time range without timestamp
                        var tod = dto.ToLocalTime().TimeOfDay;
                        if (!InTimeRange(tod, timeFrom, timeTo)) continue;
                    }

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
                        if (parsedTs)
                        {
                            var label = dto.ToLocalTime().ToString("HH:00");
                            counts[label] = counts.TryGetValue(label, out var c) ? c+1 : 1;
                        }
                    }
                    else
                    {
                        counts[type] = counts.TryGetValue(type, out var c) ? c+1 : 1;
                    }
                    // trend by day
                    DateTime dayKey = parsedTs ? dto.ToLocalTime().Date : date;
                    _trendCounts[dayKey] = _trendCounts.TryGetValue(dayKey, out var tc) ? tc+1 : 1;
                }
                catch { }
            }
        }
var parts = counts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}");
lblSummary.Text = UIStrings.Get("Reports.SummaryPrefix") + ": " + UIStrings.Format("Reports.SummaryTotal", total) + " | " + string.Join(" | ", parts);
        _lastCounts = counts;
        pnlChart.Invalidate();
        pnlTrend.Invalidate();
    }

private static ThemeMode ParseTheme(string? s)
{
    return (s?.ToLowerInvariant()) switch
    {
        "dark" => ThemeMode.Dark,
        "light" => ThemeMode.Light,
        _ => ThemeMode.System
    };
}

private void ApplyLocalization()
{
    this.Text = UIStrings.Get("Reports.Title");
    btnLoad.Text = UIStrings.Get("Reports.Load");
    btnExport.Text = UIStrings.Get("Buttons.ExportCsv");
    btnExportChart.Text = UIStrings.Get("Buttons.ExportChart");
    btnExportTrendChart.Text = UIStrings.Get("Buttons.TrendChart");
    lblProcFilter.Text = UIStrings.Get("Reports.ProcessFilter");
    chkByHour.Text = UIStrings.Get("Reports.GroupByHour");
    lblTo.Text = UIStrings.Get("Reports.DateTo");
    chkTimeFilter.Text = UIStrings.Get("Reports.TimeFilter");
    lblTimeFrom.Text = UIStrings.Get("Reports.TimeFrom");
    lblTimeTo.Text = UIStrings.Get("Reports.TimeTo");
    if (grid.Columns.Count >= 3)
    {
        grid.Columns[0].HeaderText = UIStrings.Get("Reports.Timestamp");
        grid.Columns[1].HeaderText = UIStrings.Get("Reports.Type");
        grid.Columns[2].HeaderText = UIStrings.Get("Reports.Data");
    }
}

    private void pnlChart_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
            g.DrawString(label, this.Font, Brushes.Black, x, rect.Bottom - 18);
            g.DrawString(kv.Value.ToString(), this.Font, Brushes.Black, x, y - 16);
            idx++;
        }
    }

    private void pnlTrend_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(SystemColors.Control);
        if (_trendCounts == null || _trendCounts.Count == 0) return;
        var rect = e.ClipRectangle;
        int n = _trendCounts.Count;
        int idx = 0;
        int max = Math.Max(1, _trendCounts.Values.Max());
        int barWidth = Math.Max(20, rect.Width / Math.Max(1,n) - 10);
        foreach (var kv in _trendCounts.OrderBy(k => k.Key))
        {
            int x = rect.Left + 10 + idx * (barWidth + 10);
            int h = (int)((kv.Value / (float)max) * (rect.Height - 30));
            int y = rect.Bottom - 20 - h;
            using var brush = new SolidBrush(Color.FromArgb(100, 160, 120));
            g.FillRectangle(brush, new Rectangle(x, y, barWidth, h));
            using var pen = new Pen(Color.SeaGreen, 1);
            g.DrawRectangle(pen, new Rectangle(x, y, barWidth, h));
            var label = kv.Key.ToString("MM-dd");
            g.DrawString(label, this.Font, Brushes.Black, x, rect.Bottom - 18);
            g.DrawString(kv.Value.ToString(), this.Font, Brushes.Black, x, y - 16);
            idx++;
        }
    }

    private void btnExportChart_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog { Filter = "PNG Image (*.png)|*.png|All files (*.*)|*.*", FileName = $"childguard_chart_{DateTime.Now:yyyyMMdd_HHmmss}.png" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var bmp = new Bitmap(pnlChart.Width, pnlChart.Height);
            pnlChart.DrawToBitmap(bmp, new Rectangle(Point.Empty, pnlChart.Size));
            bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
MessageBox.Show(this, UIStrings.Get("Reports.PngExported"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
MessageBox.Show(this, UIStrings.Format("Reports.ExportError", ex.Message), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExportTrendChart_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog { Filter = "PNG Image (*.png)|*.png|All files (*.*)|*.*", FileName = $"childguard_trend_{DateTime.Now:yyyyMMdd_HHmmss}.png" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var bmp = new Bitmap(pnlTrend.Width, pnlTrend.Height);
            pnlTrend.DrawToBitmap(bmp, new Rectangle(Point.Empty, pnlTrend.Size));
            bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
MessageBox.Show(this, UIStrings.Get("Reports.PngTrendExported"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
MessageBox.Show(this, UIStrings.Format("Reports.ExportError", ex.Message), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExport_Click(object? sender, EventArgs e)
    {
        if (grid.Rows.Count == 0)
        {
MessageBox.Show(this, UIStrings.Get("Reports.NoDataExport"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
MessageBox.Show(this, UIStrings.Get("Reports.ExportedCsv"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
MessageBox.Show(this, UIStrings.Format("Reports.ExportError", ex.Message), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExportTrendCsv_Click(object? sender, EventArgs e)
    {
        if (_trendCounts == null || _trendCounts.Count == 0)
        {
MessageBox.Show(this, UIStrings.Get("Reports.TrendNoData"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = $"childguard_trend_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8);
            sw.WriteLine("Date,Count");
            foreach (var kv in _trendCounts.OrderBy(k => k.Key))
            {
                var dateStr = kv.Key.ToString("yyyy-MM-dd");
                var count = kv.Value;
                sw.WriteLine(string.Join(',', dateStr, count.ToString()));
            }
            MessageBox.Show(this, UIStrings.Get("Reports.ExportedCsv"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, UIStrings.Format("Reports.ExportError", ex.Message), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RebuildLayoutModern(ThemeMode mode)
    {
        // Build sections: filters, table, counts chart, trend chart
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bool dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());
        RoundedPanel MakeSection(string title)
        {
            var rp = new RoundedPanel { Dock = DockStyle.Top, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12) };
            rp.Dark = dark;
            var header = new Label { Text = title, AutoSize = true, Font = new Font(this.Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 8) };
            rp.Controls.Add(header);
            header.Location = new Point(8, 8);
            return rp;
        }

        // Filters section
        var secFilters = MakeSection(UIStrings.Get("Reports.Section.Filters"));
        var flFilters = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 32, 8, 8), FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        flFilters.Controls.Add(new Label { Text = dtp.Name, Visible = false }); // placeholder to keep alignment
        flFilters.Controls.Add(dtp);
        flFilters.Controls.Add(lblTo);
        flFilters.Controls.Add(dtpTo);
        flFilters.Controls.Add(cmbType);
        flFilters.Controls.Add(lblProcFilter);
        flFilters.Controls.Add(txtProcFilter);
        flFilters.Controls.Add(chkByHour);
        flFilters.Controls.Add(chkTimeFilter);
        flFilters.Controls.Add(lblTimeFrom);
        flFilters.Controls.Add(dtpTimeFrom);
        flFilters.Controls.Add(lblTimeTo);
        flFilters.Controls.Add(dtpTimeTo);
        flFilters.Controls.Add(btnLoad);
        flFilters.Controls.Add(btnExport);
        flFilters.Controls.Add(btnExportChart);
        flFilters.Controls.Add(btnExportTrendCsv);
        flFilters.Controls.Add(btnExportTrendChart);
        secFilters.Controls.Add(flFilters);
        root.Controls.Add(secFilters);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Table section (summary + grid)
        var secTable = MakeSection(UIStrings.Get("Reports.Section.Table"));
        lblSummary.Dock = DockStyle.Top;
        grid.Dock = DockStyle.Fill;
        secTable.Controls.Add(grid);
        secTable.Controls.Add(lblSummary);
        secTable.Height = 420;
        root.Controls.Add(secTable);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 430));

        // Counts chart section
        var secCounts = MakeSection(UIStrings.Get("Reports.Section.Counts"));
        pnlChart.Dock = DockStyle.Fill;
        secCounts.Controls.Add(pnlChart);
        secCounts.Height = 140;
        root.Controls.Add(secCounts);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        // Trend chart section
        var secTrend = MakeSection(UIStrings.Get("Reports.Section.Trend"));
        pnlTrend.Dock = DockStyle.Fill;
        secTrend.Controls.Add(pnlTrend);
        secTrend.Height = 140;
        root.Controls.Add(secTrend);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        // Place root
        this.Controls.Clear();
        this.Controls.Add(root);
    }
}
