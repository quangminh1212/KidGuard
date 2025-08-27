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

        this.SuspendLayout();
        
        // Ẩn các control cũ từ Designer (chúng vẫn được thêm vào form nhưng sẽ không hiển thị)
        lblKeys.Visible = false;
        lblMouse.Visible = false;
        chkEnableInput.Visible = false;
        btnStart.Visible = false;
        btnStop.Visible = false;
        
        // Panel chính chứa toàn bộ content
        var mainPanel = new Panel
        {
            Name = "mainPanel",
            Location = new Point(0, 30),
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = false,
            BackColor = this.BackColor
        };
        
        // Container bên trong với padding
        var contentContainer = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(660, 380),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoScroll = false
        };
        
        // Tiêu đề Activity
        var activityTitle = new Label
        {
            Text = UIStrings.Get("Form1.Section.Activity"),
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(0, 0),
            AutoSize = true,
            ForeColor = this.ForeColor
        };
        contentContainer.Controls.Add(activityTitle);
        
        // Container cho 2 card
        var cardsContainer = new Panel
        {
            Location = new Point(0, 40),
            Size = new Size(500, 100),
            BackColor = Color.Transparent
        };
        
        // Keys Card
        var keysCard = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(230, 85),
            BackColor = dark ? Color.FromArgb(50, 50, 53) : Color.FromArgb(250, 250, 250),
            BorderStyle = BorderStyle.None
        };
        keysCard.Paint += (s, e) => {
            using var pen = new Pen(dark ? Color.FromArgb(70, 70, 73) : Color.FromArgb(220, 220, 220), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, keysCard.Width - 1, keysCard.Height - 1);
        };
        
        var keysIcon = new PictureBox
        {
            Size = new Size(28, 28),
            Location = new Point(15, 28),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Image = GlyphIcons.Render(GlyphIcons.Keyboard, 22, ThemeHelper.GetAccentColor())
        };
        keysCard.Controls.Add(keysIcon);
        
        var keysLabel = new Label
        {
            Text = UIStrings.Get("Form1.Card.Keys"),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = dark ? Color.FromArgb(180, 180, 180) : Color.FromArgb(100, 100, 100),
            Location = new Point(55, 20),
            AutoSize = true
        };
        keysCard.Controls.Add(keysLabel);
        
        var keysValue = new Label
        {
            Name = "keysValue",
            Text = "0",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = this.ForeColor,
            Location = new Point(55, 38),
            AutoSize = true
        };
        keysCard.Controls.Add(keysValue);
        cardsContainer.Controls.Add(keysCard);
        
        // Mouse Card
        var mouseCard = new Panel
        {
            Location = new Point(250, 0),
            Size = new Size(230, 85),
            BackColor = dark ? Color.FromArgb(50, 50, 53) : Color.FromArgb(250, 250, 250),
            BorderStyle = BorderStyle.None
        };
        mouseCard.Paint += (s, e) => {
            using var pen = new Pen(dark ? Color.FromArgb(70, 70, 73) : Color.FromArgb(220, 220, 220), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, mouseCard.Width - 1, mouseCard.Height - 1);
        };
        
        var mouseIcon = new PictureBox
        {
            Size = new Size(28, 28),
            Location = new Point(15, 28),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Image = GlyphIcons.Render(GlyphIcons.Mouse, 22, ThemeHelper.GetAccentColor())
        };
        mouseCard.Controls.Add(mouseIcon);
        
        var mouseLabel = new Label
        {
            Text = UIStrings.Get("Form1.Card.Mouse"),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = dark ? Color.FromArgb(180, 180, 180) : Color.FromArgb(100, 100, 100),
            Location = new Point(55, 20),
            AutoSize = true
        };
        mouseCard.Controls.Add(mouseLabel);
        
        var mouseValue = new Label
        {
            Name = "mouseValue",
            Text = "0",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = this.ForeColor,
            Location = new Point(55, 38),
            AutoSize = true
        };
        mouseCard.Controls.Add(mouseValue);
        cardsContainer.Controls.Add(mouseCard);
        
        contentContainer.Controls.Add(cardsContainer);
        
        // Tiêu đề Controls
        var controlsTitle = new Label
        {
            Text = UIStrings.Get("Form1.Section.Controls"),
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(0, 160),
            AutoSize = true,
            ForeColor = this.ForeColor
        };
        contentContainer.Controls.Add(controlsTitle);
        
        // Panel chứa controls
        var controlsPanel = new Panel
        {
            Location = new Point(0, 200),
            Size = new Size(500, 70),
            BackColor = dark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(248, 248, 248),
            BorderStyle = BorderStyle.None
        };
        controlsPanel.Paint += (s, e) => {
            using var pen = new Pen(dark ? Color.FromArgb(70, 70, 73) : Color.FromArgb(220, 220, 220), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, controlsPanel.Width - 1, controlsPanel.Height - 1);
        };
        
        // Checkbox mới
        var newCheckbox = new CheckBox
        {
            Text = UIStrings.Get("Form1.EnableInput"),
            Font = new Font("Segoe UI", 10),
            Location = new Point(20, 23),
            AutoSize = true,
            Checked = chkEnableInput.Checked,
            ForeColor = this.ForeColor
        };
        newCheckbox.CheckedChanged += (s, e) => chkEnableInput.Checked = newCheckbox.Checked;
        controlsPanel.Controls.Add(newCheckbox);
        
        // Button Start mới
        var newStartBtn = new Button
        {
            Text = UIStrings.Get("Buttons.Start"),
            Font = new Font("Segoe UI", 10),
            Location = new Point(270, 18),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        newStartBtn.Click += btnStart_Click;
        ModernStyle.MakePrimary(newStartBtn, dark);
        controlsPanel.Controls.Add(newStartBtn);
        
        // Button Stop mới
        var newStopBtn = new Button
        {
            Text = UIStrings.Get("Buttons.Stop"),
            Font = new Font("Segoe UI", 10),
            Location = new Point(380, 18),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        newStopBtn.Click += btnStop_Click;
        ModernStyle.MakeSecondary(newStopBtn, dark);
        controlsPanel.Controls.Add(newStopBtn);
        
        contentContainer.Controls.Add(controlsPanel);
        
        // Cập nhật timer để hiển thị giá trị lên card mới
        uiTimer.Tick -= uiTimer_Tick; // Remove old handler
        uiTimer.Tick += (s, e) => {
            if (contentContainer.Controls.Find("keysValue", true).FirstOrDefault() is Label kv)
                kv.Text = _lastKeys.ToString();
            if (contentContainer.Controls.Find("mouseValue", true).FirstOrDefault() is Label mv)
                mv.Text = _lastMouse.ToString();
        };
        
        mainPanel.Controls.Add(contentContainer);
        this.Controls.Add(mainPanel);
        
        // Đảm bảo mainPanel nằm dưới menuStrip
        mainPanel.BringToFront();
        menuStrip1.BringToFront();
        
        this.ResumeLayout(true);
    }
}
