using System.Windows.Forms;
using ChildGuard.Core.Configuration;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.Windows.Forms.Layout;

namespace ChildGuard.UI;

public partial class SettingsForm : Form
{
    private AppConfig _config = new();
    private string _path = string.Empty;
    private string _initialLanguage = UIStrings.DefaultLanguage;

    public SettingsForm()
    {
InitializeComponent();
        // Theme will be applied after we read config in Load
    }

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        _config = ConfigManager.Load(out _path);
        UIStrings.SetLanguage(_config.UILanguage);
        _initialLanguage = _config.UILanguage ?? UIStrings.DefaultLanguage;
        // Apply theme based on config and rebuild layout for a modern look
        ModernStyle.Apply(this, ParseTheme(_config.Theme));
        ApplyLocalization();
        RebuildLayoutModern(ParseTheme(_config.Theme));

        chkInput.Checked = _config.EnableInputMonitoring;
        chkActiveWindow.Checked = _config.EnableActiveWindowTracking;
        lblPath.Text = string.Format(UIStrings.Get("Settings.ConfigPath"), _path ?? "-");
        // Language selector
        cmbLanguage.Items.Clear();
        cmbLanguage.Items.AddRange(new object[] { UIStrings.Get("Settings.Language.English"), UIStrings.Get("Settings.Language.Vietnamese") });
        cmbLanguage.SelectedIndex = string.Equals(_config.UILanguage, "vi", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        // Theme selector (created in code, placed in layout)
        EnsureThemeControls();
        cmbTheme.Items.Clear();
        cmbTheme.Items.AddRange(new object[] { UIStrings.Get("Settings.Theme.System"), UIStrings.Get("Settings.Theme.Light"), UIStrings.Get("Settings.Theme.Dark") });
        cmbTheme.SelectedIndex = (_config.Theme?.ToLowerInvariant()) switch { "light" => 1, "dark" => 2, _ => 0 };
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
        // Additional quiet windows
        txtAdditionalQuiet.Text = _config.AdditionalQuietWindows is { Length: >0 }
            ? string.Join(Environment.NewLine, _config.AdditionalQuietWindows)
            : string.Empty;
    }

    private void ApplyLocalization()
    {
        this.Text = UIStrings.Get("Settings.Title");
        chkInput.Text = UIStrings.Get("Settings.EnableInput");
        chkActiveWindow.Text = UIStrings.Get("Settings.EnableActiveWindow");
        lblBlocked.Text = UIStrings.Get("Settings.Blocked");
        lblAllowedQuiet.Text = UIStrings.Get("Settings.AllowedQuiet");
        lblQuiet.Text = UIStrings.Get("Settings.Quiet");
        lblRetention.Text = UIStrings.Get("Settings.Retention");
        lblCloseWarn.Text = UIStrings.Get("Settings.CloseWarn");
        lblMaxSize.Text = UIStrings.Get("Settings.MaxSize");
        lblAdditionalQuiet.Text = UIStrings.Get("Settings.AdditionalQuiet");
        lblLanguage.Text = UIStrings.Get("Settings.Language");
        btnSave.Text = UIStrings.Get("Buttons.Save");
        btnCancel.Text = UIStrings.Get("Buttons.Cancel");
        btnOpenConfig.Text = UIStrings.Get("Buttons.OpenConfig");
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
        // Language and Theme
        _config.UILanguage = (cmbLanguage.SelectedIndex == 1) ? "vi" : "en";
        _config.Theme = cmbTheme.SelectedIndex switch { 1 => "Light", 2 => "Dark", _ => "System" };
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
        // Additional quiet windows
        var addQuiet = (txtAdditionalQuiet.Text ?? string.Empty)
            .Split(new[]{"\r\n","\n"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length>0)
            .ToArray();
        _config.AdditionalQuietWindows = addQuiet;

        ConfigManager.Save(_config, out var savedPath);
        lblPath.Text = string.Format(UIStrings.Get("Settings.ConfigPath"), savedPath);
        if (!string.Equals(_initialLanguage, _config.UILanguage, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, UIStrings.Get("Settings.LanguageChanged"), UIStrings.Get("General.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnOpenConfig_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_path)) _ = ConfigManager.Load(out _path);
            if (!string.IsNullOrWhiteSpace(_path))
            {
                var psi = new System.Diagnostics.ProcessStartInfo("notepad.exe", '"' + _path + '"') { UseShellExecute = true };
                System.Diagnostics.Process.Start(psi);
            }
        }
        catch { }
    }
    private ComboBox? cmbTheme;
    private Label? lblTheme;

    private void EnsureThemeControls()
    {
        if (cmbTheme == null)
        {
            cmbTheme = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, Name = "cmbTheme" };
        }
        if (lblTheme == null)
        {
            lblTheme = new Label { AutoSize = true, Text = UIStrings.Get("Settings.Theme"), Name = "lblTheme" };
        }
        // Update localized text
        lblTheme!.Text = UIStrings.Get("Settings.Theme");
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

    private void RebuildLayoutModern(ThemeMode mode)
    {
        // Keep menu/title; rebuild client area layout
        var margin = 10;
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            ColumnCount = 1,
            RowCount = 0,
            Padding = new Padding(12),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Helper to create a rounded section with header
        RoundedPanel MakeSection(string title)
        {
            var rp = new RoundedPanel { Dock = DockStyle.Top, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12), Height = 10 };
            rp.Dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());
            var header = new Label { Text = title, AutoSize = true, Font = new Font(this.Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 8) };
            rp.Controls.Add(header);
            header.Location = new Point(8, 8);
            return rp;
        }

        // Clear existing layout root (except controls we will re-parent)
        foreach (Control c in this.Controls.OfType<Control>().ToArray())
        {
            if (c is not Button && c is not Label && c is not TextBox && c is not CheckBox && c is not DateTimePicker && c is not NumericUpDown)
            {
                // keep as-is (like form borders)
            }
        }

        // General section
        var secGeneral = MakeSection(UIStrings.Get("Settings.Section.General"));
        var flGeneral = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false, Padding = new Padding(8, 32, 8, 8) };
        // Place Language + Theme + toggles
        var flLang = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        flLang.Controls.Add(lblLanguage);
        flLang.Controls.Add(cmbLanguage);
        flLang.Controls.Add(new Label { Width = 16 });
        flLang.Controls.Add(lblTheme!);
        flLang.Controls.Add(cmbTheme!);
        var flToggles = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        flToggles.Controls.Add(chkInput);
        flToggles.Controls.Add(new Label { Width = 16 });
        flToggles.Controls.Add(chkActiveWindow);
        flGeneral.Controls.Add(flLang);
        flGeneral.Controls.Add(new Label { Width = 24 });
        flGeneral.Controls.Add(flToggles);
        secGeneral.Controls.Add(flGeneral);
        root.Controls.Add(secGeneral);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Blocked section
        var secBlocked = MakeSection(UIStrings.Get("Settings.Section.Blocked"));
        lblBlocked.Dock = DockStyle.Top;
        txtBlocked.Dock = DockStyle.Fill;
        secBlocked.Controls.Add(txtBlocked);
        secBlocked.Controls.Add(lblBlocked);
        secBlocked.Height = 160;
        root.Controls.Add(secBlocked);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));

        // Allowed quiet section
        var secAllowed = MakeSection(UIStrings.Get("Settings.Section.AllowedQuiet"));
        lblAllowedQuiet.Dock = DockStyle.Top;
        txtAllowedQuiet.Dock = DockStyle.Fill;
        secAllowed.Controls.Add(txtAllowedQuiet);
        secAllowed.Controls.Add(lblAllowedQuiet);
        secAllowed.Height = 140;
        root.Controls.Add(secAllowed);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        // Quiet hours section
        var secQuiet = MakeSection(UIStrings.Get("Settings.Section.QuietHours"));
        var flQuiet = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 32, 8, 8) };
        flQuiet.Controls.Add(lblQuiet);
        flQuiet.Controls.Add(dtStart);
        flQuiet.Controls.Add(new Label { Text = "-", AutoSize = true, Margin = new Padding(8, 6, 8, 0) });
        flQuiet.Controls.Add(dtEnd);
        secQuiet.Controls.Add(flQuiet);
        root.Controls.Add(secQuiet);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Retention section
        var secRetention = MakeSection(UIStrings.Get("Settings.Section.Retention"));
        var tlRet = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(8, 32, 8, 8) };
        tlRet.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlRet.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlRet.Controls.Add(lblRetention, 0, 0);
        tlRet.Controls.Add(numRetention, 1, 0);
        tlRet.Controls.Add(lblCloseWarn, 0, 1);
        tlRet.Controls.Add(numCloseWarn, 1, 1);
        tlRet.Controls.Add(lblMaxSize, 0, 2);
        tlRet.Controls.Add(numMaxSize, 1, 2);
        secRetention.Controls.Add(tlRet);
        root.Controls.Add(secRetention);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Additional quiet
        var secAddQuiet = MakeSection(UIStrings.Get("Settings.Section.AdditionalQuiet"));
        lblAdditionalQuiet.Dock = DockStyle.Top;
        txtAdditionalQuiet.Dock = DockStyle.Fill;
        secAddQuiet.Controls.Add(txtAdditionalQuiet);
        secAddQuiet.Controls.Add(lblAdditionalQuiet);
        secAddQuiet.Height = 140;
        root.Controls.Add(secAddQuiet);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        // Config path
        var secConfig = MakeSection(UIStrings.Get("Settings.Section.Config"));
        var flCfg = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 32, 8, 8) };
        flCfg.Controls.Add(lblPath);
        flCfg.Controls.Add(new Label { Width = 16 });
        flCfg.Controls.Add(btnOpenConfig);
        secConfig.Controls.Add(flCfg);
        root.Controls.Add(secConfig);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Buttons
        var flButtons = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        flButtons.Controls.Add(btnSave);
        flButtons.Controls.Add(btnCancel);
        root.Controls.Add(flButtons);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Place root
        this.Controls.Clear();
        this.Controls.Add(root);
        this.Text = UIStrings.Get("Settings.Title");
    }
}
