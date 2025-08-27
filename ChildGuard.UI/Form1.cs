using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.Windows.Forms.Layout;

namespace ChildGuard.UI;

public partial class Form1 : Form
{
    private readonly HookManager _hookManager = new();
    private long _lastKeys;
    private long _lastMouse;
    private volatile bool _running;
    private AppConfig _cfg = new();

public Form1()
{
InitializeComponent();
    _cfg = ConfigManager.Load(out _);
    UIStrings.SetLanguage(_cfg.UILanguage);
    EnsureHelpMenu();
    ApplyLocalization();
    ModernStyle.Apply(this, ParseTheme(_cfg.Theme));
    RebuildLayoutModern(ParseTheme(_cfg.Theme));
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

    private void mnuPolicy_Click(object? sender, EventArgs e)
    {
        using var dlg = new PolicyEditorForm();
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
    lblKeys.Text = UIStrings.Format("Labels.KeysCount", _lastKeys);
    lblMouse.Text = UIStrings.Format("Labels.MouseCount", _lastMouse);
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
        this.Text = UIStrings.Get("App.Title");
        try { this.MainMenuStrip!.Font = new Font("Segoe UI", 10F); } catch { }
        mnuSettings.Text = UIStrings.Get("Menu.Settings");
        mnuReports.Text = UIStrings.Get("Menu.Reports");
        mnuPolicy.Text = UIStrings.Get("Menu.PolicyEditor");
        // Update help menu text if present
        var helpItem = this.menuStrip1.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => (string?)i.Tag == "help");
        if (helpItem != null)
        {
            helpItem.Text = UIStrings.Get("Menu.Help");
            if (helpItem.DropDownItems.Count > 0)
            {
                var about = helpItem.DropDownItems[0] as ToolStripMenuItem;
                if (about != null) about.Text = UIStrings.Get("Menu.About");
            }
        }
        chkEnableInput.Text = UIStrings.Get("Form1.EnableInput");
        btnStart.Text = UIStrings.Get("Buttons.Start");
        btnStop.Text = UIStrings.Get("Buttons.Stop");
    }

    private void EnsureHelpMenu()
    {
        try
        {
            // Avoid duplicate
            var exists = this.menuStrip1.Items.OfType<ToolStripMenuItem>().Any(mi => (string?)mi.Tag == "help");
            if (exists) return;
            var help = new ToolStripMenuItem { Name = "mnuHelp", Tag = "help", Text = UIStrings.Get("Menu.Help") };
            var about = new ToolStripMenuItem { Name = "mnuAbout", Text = UIStrings.Get("Menu.About") };
            about.Click += (s, e) =>
            {
                // Placeholder - will be replaced by AboutForm dialog
                MessageBox.Show(this, "ChildGuard", UIStrings.Get("Menu.About"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            help.DropDownItems.Add(about);
            this.menuStrip1.Items.Add(help);
        }
        catch { }
    }

    private void RebuildLayoutModern(ThemeMode mode)
    {
        bool dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());

        // Prepare controls appearance
        lblKeys.Font = new Font(this.Font, FontStyle.Regular);
        lblMouse.Font = new Font(this.Font, FontStyle.Regular);
        btnStart.Width = Math.Max(96, btnStart.Width);
        btnStop.Width = Math.Max(96, btnStop.Width);
        // Primary/secondary styles
        ModernStyle.MakePrimary(btnStart, dark);
        ModernStyle.MakeSecondary(btnStop, dark);

        // Sections builder
        RoundedPanel MakeSection(string title)
        {
            var rp = new RoundedPanel { Dock = DockStyle.Top, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12) };
            rp.Dark = dark;
            var header = new Label { Text = title, AutoSize = true, Font = new Font(this.Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 8) };
            rp.Controls.Add(header);
            header.Location = new Point(8, 8);
            return rp;
        }

        // Build root layout with Menu on top
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Optional sidebar navigation
        Panel? side = null;
        if (_cfg.UseSidebarNavigation)
        {
            side = new Panel { Dock = DockStyle.Left, Width = 180, Padding = new Padding(12) };
            side.BackColor = this.BackColor;
            side.ForeColor = this.ForeColor;
            var stack = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            Button MakeSideBtn(string text, char glyph, EventHandler onClick)
            {
                var b = new Button { Text = text, TextAlign = ContentAlignment.MiddleLeft, Width = 156, Height = 36, ImageAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,8,0), Margin = new Padding(0,0,0,8) };
                b.Image = GlyphIcons.Render(glyph, 16, ThemeHelper.GetAccentColor());
                b.Click += onClick;
                ModernStyle.MakeSecondary(b, dark);
                return b;
            }

            var btnSettings = MakeSideBtn(UIStrings.Get("Menu.Settings"), GlyphIcons.Settings, (s,e)=> { using var dlg = new SettingsForm(); dlg.ShowDialog(this); });
            var btnReports = MakeSideBtn(UIStrings.Get("Menu.Reports"), GlyphIcons.Reports, (s,e)=> { using var dlg = new ReportsForm(); dlg.ShowDialog(this); });
            var btnPolicy  = MakeSideBtn(UIStrings.Get("Menu.PolicyEditor"), GlyphIcons.Policy, (s,e)=> { using var dlg = new PolicyEditorForm(); dlg.ShowDialog(this); });

            stack.Controls.Add(btnSettings);
            stack.Controls.Add(btnReports);
            stack.Controls.Add(btnPolicy);
            side.Controls.Add(stack);
        }

        // Activity section (stats)
        var secActivity = MakeSection(UIStrings.Get("Form1.Section.Activity"));
        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 32, 8, 8), WrapContents = true };
        // Key card
        var keyCard = new RoundedPanel { Width = 220, Height = 72, Padding = new Padding(12), Margin = new Padding(0,0,12,0), Dark = dark };
        var keyIcon = new PictureBox { Image = GlyphIcons.Render(GlyphIcons.Keyboard, 18, ThemeHelper.GetAccentColor()), SizeMode = PictureBoxSizeMode.CenterImage, Width = 24, Height = 24, Location = new Point(8, 10) };
        var keyTitle = new Label { Text = UIStrings.Get("Form1.Card.Keys"), AutoSize = true, Location = new Point(38, 10) };
        var keyValue = new Label { Text = "0", AutoSize = true, Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold), Location = new Point(38, 32) };
        keyCard.Controls.Add(keyIcon); keyCard.Controls.Add(keyTitle); keyCard.Controls.Add(keyValue);
        // Mouse card
        var mouseCard = new RoundedPanel { Width = 220, Height = 72, Padding = new Padding(12), Dark = dark };
        var mouseIcon = new PictureBox { Image = GlyphIcons.Render(GlyphIcons.Mouse, 18, ThemeHelper.GetAccentColor()), SizeMode = PictureBoxSizeMode.CenterImage, Width = 24, Height = 24, Location = new Point(8, 10) };
        var mouseTitle = new Label { Text = UIStrings.Get("Form1.Card.Mouse"), AutoSize = true, Location = new Point(38, 10) };
        var mouseValue = new Label { Text = "0", AutoSize = true, Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold), Location = new Point(38, 32) };
        mouseCard.Controls.Add(mouseIcon); mouseCard.Controls.Add(mouseTitle); mouseCard.Controls.Add(mouseValue);
        cards.Controls.Add(keyCard);
        cards.Controls.Add(mouseCard);
        secActivity.Controls.Add(cards);
        // Bind dynamic updates
        uiTimer.Tick += (_, __) => { keyValue.Text = _lastKeys.ToString(); mouseValue.Text = _lastMouse.ToString(); };
        root.Controls.Add(secActivity);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Controls section
        var secControls = MakeSection(UIStrings.Get("Form1.Section.Controls"));
        var flCtrls = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 32, 8, 8), WrapContents = true };
        // Toggle switch + label
        var toggle = new ToggleSwitch { Name = "toggleInput", Checked = chkEnableInput.Checked, Width = 50, Height = 28, Margin = new Padding(0, 2, 8, 0) };
        toggle.CheckedChanged += (_, __) => chkEnableInput.Checked = toggle.Checked;
        var lblToggle = new Label { AutoSize = true, Text = UIStrings.Get("Form1.EnableInput"), Margin = new Padding(0, 6, 16, 0) };
        flCtrls.Controls.Add(toggle);
        flCtrls.Controls.Add(lblToggle);
        // Buttons
        flCtrls.Controls.Add(btnStart);
        flCtrls.Controls.Add(btnStop);
        secControls.Controls.Add(flCtrls);
        root.Controls.Add(secControls);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Place into form: keep menu, then root
        this.SuspendLayout();
        var menu = this.menuStrip1;
        this.Controls.Clear();
        menu.Dock = DockStyle.Top;
        this.Controls.Add(menu);
        this.MainMenuStrip = menu;
        if (side != null) this.Controls.Add(side);
        this.Controls.Add(root);
        root.BringToFront();
        this.ResumeLayout(true);

        // Window sizing
        this.MinimumSize = new Size(520, 300);
        this.ClientSize = new Size(Math.Max(520, this.ClientSize.Width), Math.Max(300, this.ClientSize.Height));
    }
}
