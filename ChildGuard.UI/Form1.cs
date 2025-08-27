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
    SetupModernLayout();
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
    lblKeys.Text = _lastKeys.ToString();
    lblMouse.Text = _lastMouse.ToString();
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
                try { using var dlg = new AboutForm(); dlg.ShowDialog(this); } catch { }
            };
            help.DropDownItems.Add(about);
            this.menuStrip1.Items.Add(help);
        }
        catch { }
    }

    private void SetupModernLayout()
    {
        var mode = ParseTheme(_cfg.Theme);
        bool dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());

        // Chỉ cập nhật vị trí và style cho các control đã có sẵn từ Designer
        this.SuspendLayout();
        
        // Cập nhật form size
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.ClientSize = new Size(700, 450);
        this.MinimumSize = new Size(600, 400);
        
        // Panel chính chứa content
        var mainPanel = new Panel
        {
            Location = new Point(12, 40),
            Size = new Size(676, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true
        };
        
        // Tiêu đề section Activity
        var lblActivity = new Label
        {
            Text = UIStrings.Get("Form1.Section.Activity"),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(0, 0),
            AutoSize = true
        };
        mainPanel.Controls.Add(lblActivity);
        
        // Panel chứa 2 card thống kê
        var statsPanel = new Panel
        {
            Location = new Point(0, 35),
            Size = new Size(500, 90)
        };
        
        // Card Keys
        var keyCard = new Panel
        {
            Size = new Size(230, 80),
            Location = new Point(0, 0),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = dark ? Color.FromArgb(45, 45, 48) : Color.White
        };
        
        var keyIcon = new PictureBox
        {
            Image = GlyphIcons.Render(GlyphIcons.Keyboard, 20, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Size = new Size(30, 30),
            Location = new Point(15, 25)
        };
        keyCard.Controls.Add(keyIcon);
        
        var lblKeyText = new Label
        {
            Text = UIStrings.Get("Form1.Card.Keys"),
            Font = new Font("Segoe UI", 9),
            ForeColor = dark ? Color.LightGray : Color.Gray,
            Location = new Point(55, 15),
            AutoSize = true
        };
        keyCard.Controls.Add(lblKeyText);
        
        // Di chuyển lblKeys vào card
        lblKeys.Parent = keyCard;
        lblKeys.Location = new Point(55, 35);
        lblKeys.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblKeys.Text = "0";
        
        statsPanel.Controls.Add(keyCard);
        
        // Card Mouse
        var mouseCard = new Panel
        {
            Size = new Size(230, 80),
            Location = new Point(250, 0),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = dark ? Color.FromArgb(45, 45, 48) : Color.White
        };
        
        var mouseIcon = new PictureBox
        {
            Image = GlyphIcons.Render(GlyphIcons.Mouse, 20, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Size = new Size(30, 30),
            Location = new Point(15, 25)
        };
        mouseCard.Controls.Add(mouseIcon);
        
        var lblMouseText = new Label
        {
            Text = UIStrings.Get("Form1.Card.Mouse"),
            Font = new Font("Segoe UI", 9),
            ForeColor = dark ? Color.LightGray : Color.Gray,
            Location = new Point(55, 15),
            AutoSize = true
        };
        mouseCard.Controls.Add(lblMouseText);
        
        // Di chuyển lblMouse vào card
        lblMouse.Parent = mouseCard;
        lblMouse.Location = new Point(55, 35);
        lblMouse.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblMouse.Text = "0";
        
        statsPanel.Controls.Add(mouseCard);
        mainPanel.Controls.Add(statsPanel);
        
        // Tiêu đề section Controls
        var lblControls = new Label
        {
            Text = UIStrings.Get("Form1.Section.Controls"),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(0, 140),
            AutoSize = true
        };
        mainPanel.Controls.Add(lblControls);
        
        // Panel chứa controls
        var controlPanel = new Panel
        {
            Location = new Point(0, 175),
            Size = new Size(500, 80),
            BackColor = dark ? Color.FromArgb(40, 40, 43) : Color.FromArgb(250, 250, 250),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Di chuyển checkbox
        chkEnableInput.Parent = controlPanel;
        chkEnableInput.Location = new Point(20, 25);
        chkEnableInput.Font = new Font("Segoe UI", 10);
        
        // Di chuyển buttons
        btnStart.Parent = controlPanel;
        btnStart.Location = new Point(260, 20);
        btnStart.Size = new Size(100, 35);
        btnStart.FlatStyle = FlatStyle.Flat;
        ModernStyle.MakePrimary(btnStart, dark);
        
        btnStop.Parent = controlPanel;
        btnStop.Location = new Point(370, 20);
        btnStop.Size = new Size(100, 35);
        btnStop.FlatStyle = FlatStyle.Flat;
        ModernStyle.MakeSecondary(btnStop, dark);
        
        mainPanel.Controls.Add(controlPanel);
        
        // Thêm main panel vào form
        this.Controls.Add(mainPanel);
        
        this.ResumeLayout(true);
    }
}
