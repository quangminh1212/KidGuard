using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.Windows.Forms.Layout;
using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace ChildGuard.UI;

public partial class Form1 : Form
{
    private readonly AdvancedProtectionManager _protectionManager = new();
    private long _lastKeys;
    private long _lastMouse;
    private long _threatsDetected;
    private volatile bool _running;
    private AppConfig _cfg = new();
    private readonly ConcurrentQueue<ThreatNotification> _threatQueue = new();
    private Panel _threatPanel;
    private ListBox _activityLog;
    private Label _statusLabel;

public Form1()
{
InitializeComponent();
    _cfg = ConfigManager.Load(out _);
    UIStrings.SetLanguage(_cfg.UILanguage);
    EnsureHelpMenu();
    ApplyLocalization();
    ModernStyle.Apply(this, ParseTheme(_cfg.Theme));
    
    // Use new simple modern layout
    var isDark = ParseTheme(_cfg.Theme) == ThemeMode.Dark || 
                 (ParseTheme(_cfg.Theme) == ThemeMode.System && ThemeHelper.IsSystemDark());
    SimpleModernLayout.ApplyToForm(this, isDark);
    SetupEventHandlers();
    
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
    
    private void SetupEventHandlers()
    {
        // Hook up buttons
        var startBtn = this.Controls.Find("startBtn", true).FirstOrDefault() as Button;
        if (startBtn != null)
        {
            startBtn.Click += (s, e) => {
                if (_running) return;
                var checkbox = this.Controls.Find("monitoringCheck", true).FirstOrDefault() as CheckBox;
                var cfg = new AppConfig { EnableInputMonitoring = checkbox?.Checked ?? false };
                _hookManager.Start(cfg);
                _running = true;
            };
        }
        
        var stopBtn = this.Controls.Find("stopBtn", true).FirstOrDefault() as Button;
        if (stopBtn != null)
        {
            stopBtn.Click += (s, e) => {
                if (!_running) return;
                _hookManager.Stop();
                _running = false;
            };
        }
        
        // Update timer to show values
        uiTimer.Tick += (s, e) => {
            var keyValue = this.Controls.Find("keyValue", true).FirstOrDefault() as Label;
            if (keyValue != null) keyValue.Text = _lastKeys.ToString("N0");
            
            var mouseValue = this.Controls.Find("mouseValue", true).FirstOrDefault() as Label;
            if (mouseValue != null) mouseValue.Text = _lastMouse.ToString("N0");
        };
        
        // Hide original controls
        lblKeys.Visible = false;
        lblMouse.Visible = false;
        chkEnableInput.Visible = false;
        btnStart.Visible = false;
        btnStop.Visible = false;
    }

    private void SetupModernLayout()
    {
        var mode = ParseTheme(_cfg.Theme);
        bool dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());

        this.SuspendLayout();
        
        // Ẩn các control cũ từ Designer
        lblKeys.Visible = false;
        lblMouse.Visible = false;
        chkEnableInput.Visible = false;
        btnStart.Visible = false;
        btnStop.Visible = false;
        
        // Setup form
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        this.BackColor = dark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(251, 251, 251);
        this.ForeColor = dark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(40, 40, 40);
        
        // Main panel - đơn giản hóa
        var mainPanel = new Panel
        {
            Location = new Point(0, menuStrip1.Height),
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - menuStrip1.Height),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = this.BackColor,
            Padding = new Padding(40)
        };
        
        // Content area
        var contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 20, 0, 0),
            BackColor = Color.Transparent
        };
        
        // Section: Activity Monitoring với icon
        var activitySection = new Panel
        {
            Location = new Point(0, 20),
            Size = new Size(640, 140),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        
        var activityHeader = new Panel
        {
            Height = 35,
            Dock = DockStyle.Top
        };
        
        var activityIcon = new PictureBox
        {
            Size = new Size(20, 20),
            Location = new Point(0, 7),
            Image = GlyphIcons.Render('◉', 18, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        activityHeader.Controls.Add(activityIcon);
        
        var activityTitle = new Label
        {
            Text = UIStrings.Get("Form1.Section.Activity"),
            Font = new Font("Segoe UI Semibold", 12),
            Location = new Point(25, 8),
            AutoSize = true,
            ForeColor = this.ForeColor
        };
        activityHeader.Controls.Add(activityTitle);
        activitySection.Controls.Add(activityHeader);
        
        // Cards container với spacing đều
        var cardsPanel = new FlowLayoutPanel
        {
            Location = new Point(0, 45),
            Size = new Size(640, 90),
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        
        // Helper function để tạo card đẹp
        Panel CreateStatCard(string title, char icon, string valueName, Color accentColor)
        {
            var card = new Panel
            {
                Size = new Size(280, 85),
                Margin = new Padding(0, 0, 20, 0),
                BackColor = dark ? Color.FromArgb(38, 38, 41) : Color.White
            };
            
            // Rounded corners và shadow effect
            card.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Shadow
                if (!dark)
                {
                    using var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
                    g.FillRectangle(shadowBrush, new Rectangle(2, 2, card.Width - 2, card.Height - 2));
                }
                
                // Main card background với rounded corners
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                var rect = new Rectangle(0, 0, card.Width - 3, card.Height - 3);
                int radius = 8;
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                
                using var brush = new SolidBrush(card.BackColor);
                g.FillPath(brush, path);
                
                // Accent line on left
                using var accentBrush = new SolidBrush(accentColor);
                g.FillRectangle(accentBrush, new Rectangle(0, 10, 3, card.Height - 20));
            };
            
            // Icon container với background circula
            var iconContainer = new Panel
            {
                Size = new Size(45, 45),
                Location = new Point(15, 20),
                BackColor = Color.Transparent
            };
            iconContainer.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.FromArgb(30, accentColor));
                g.FillEllipse(brush, 0, 0, 44, 44);
            };
            
            var iconPic = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(10, 10),
                Image = GlyphIcons.Render(icon, 20, accentColor),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };
            iconContainer.Controls.Add(iconPic);
            card.Controls.Add(iconContainer);
            
            // Title label
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = dark ? Color.FromArgb(170, 170, 170) : Color.FromArgb(100, 100, 100),
                Location = new Point(70, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(titleLabel);
            
            // Value label với animation ready
            var valueLabel = new Label
            {
                Name = valueName,
                Text = "0",
                Font = new Font("Segoe UI Variable Display", 22, FontStyle.Bold),
                ForeColor = this.ForeColor,
                Location = new Point(70, 38),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(valueLabel);
            
            // Unit label  
            var unitLabel = new Label
            {
                Text = "lần",
                Font = new Font("Segoe UI", 9),
                ForeColor = dark ? Color.FromArgb(130, 130, 130) : Color.FromArgb(150, 150, 150),
                Location = new Point(120, 48),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(unitLabel);
            
            return card;
        }
        
        // Tạo 2 cards
        var keyCard = CreateStatCard(
            UIStrings.Get("Form1.Card.Keys"), 
            GlyphIcons.Keyboard, 
            "keysValue", 
            Color.FromArgb(0, 120, 212));
        cardsPanel.Controls.Add(keyCard);
        
        var mouseCard = CreateStatCard(
            UIStrings.Get("Form1.Card.Mouse"), 
            GlyphIcons.Mouse, 
            "mouseValue", 
            Color.FromArgb(16, 124, 16));
        cardsPanel.Controls.Add(mouseCard);
        
        activitySection.Controls.Add(cardsPanel);
        contentArea.Controls.Add(activitySection);
        
        // Section: Controls
        var controlSection = new Panel
        {
            Location = new Point(0, 180),
            Size = new Size(640, 120),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        
        var controlHeader = new Panel
        {
            Height = 35,
            Dock = DockStyle.Top
        };
        
        var controlIcon = new PictureBox
        {
            Size = new Size(20, 20),
            Location = new Point(0, 7),
            Image = GlyphIcons.Render('⚙', 18, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        controlHeader.Controls.Add(controlIcon);
        
        var controlTitle = new Label
        {
            Text = UIStrings.Get("Form1.Section.Controls"),
            Font = new Font("Segoe UI Semibold", 12),
            Location = new Point(25, 8),
            AutoSize = true,
            ForeColor = this.ForeColor
        };
        controlHeader.Controls.Add(controlTitle);
        controlSection.Controls.Add(controlHeader);
        
        // Control panel với modern style
        var controlPanel = new Panel
        {
            Location = new Point(0, 45),
            Size = new Size(580, 65),
            BackColor = dark ? Color.FromArgb(38, 38, 41) : Color.White
        };
        controlPanel.Paint += (s, e) => {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var rect = new Rectangle(0, 0, controlPanel.Width - 1, controlPanel.Height - 1);
            int radius = 8;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            
            using var pen = new Pen(dark ? Color.FromArgb(50, 50, 53) : Color.FromArgb(230, 230, 230), 1);
            g.DrawPath(pen, path);
        };
        
        // Modern toggle switch thay cho checkbox
        var toggleSwitch = new Panel
        {
            Size = new Size(50, 24),
            Location = new Point(25, 20),
            BackColor = Color.FromArgb(200, 200, 200),
            Cursor = Cursors.Hand
        };
        bool isChecked = chkEnableInput.Checked;
        
        var toggleThumb = new Panel
        {
            Size = new Size(20, 20),
            Location = isChecked ? new Point(26, 2) : new Point(2, 2),
            BackColor = isChecked ? ThemeHelper.GetAccentColor() : Color.White
        };
        toggleSwitch.Controls.Add(toggleThumb);
        
        toggleSwitch.Paint += (s, e) => {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 23, 23, 90, 180);
            path.AddArc(26, 0, 23, 23, 270, 180);
            path.CloseAllFigures();
            
            var bgColor = isChecked ? 
                Color.FromArgb(100, ThemeHelper.GetAccentColor()) : 
                Color.FromArgb(200, 200, 200);
            using var brush = new SolidBrush(bgColor);
            g.FillPath(brush, path);
        };
        
        toggleSwitch.Click += (s, e) => {
            isChecked = !isChecked;
            chkEnableInput.Checked = isChecked;
            toggleThumb.Location = isChecked ? new Point(26, 2) : new Point(2, 2);
            toggleThumb.BackColor = isChecked ? ThemeHelper.GetAccentColor() : Color.White;
            toggleSwitch.Invalidate();
        };
        controlPanel.Controls.Add(toggleSwitch);
        
        var toggleLabel = new Label
        {
            Text = UIStrings.Get("Form1.EnableInput"),
            Font = new Font("Segoe UI", 10),
            Location = new Point(85, 21),
            AutoSize = true,
            ForeColor = this.ForeColor,
            BackColor = Color.Transparent
        };
        controlPanel.Controls.Add(toggleLabel);
        
        // Modern buttons với hover effect
        var startBtn = new Button
        {
            Text = UIStrings.Get("Buttons.Start"),
            Font = new Font("Segoe UI Semibold", 10),
            Location = new Point(320, 15),
            Size = new Size(110, 38),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = ThemeHelper.GetAccentColor(),
            ForeColor = Color.White
        };
        startBtn.FlatAppearance.BorderSize = 0;
        startBtn.Click += btnStart_Click;
        startBtn.MouseEnter += (s, e) => startBtn.BackColor = ControlPaint.Light(ThemeHelper.GetAccentColor(), 0.1f);
        startBtn.MouseLeave += (s, e) => startBtn.BackColor = ThemeHelper.GetAccentColor();
        controlPanel.Controls.Add(startBtn);
        
        var stopBtn = new Button
        {
            Text = UIStrings.Get("Buttons.Stop"),
            Font = new Font("Segoe UI Semibold", 10),
            Location = new Point(440, 15),
            Size = new Size(110, 38),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = dark ? Color.FromArgb(60, 60, 63) : Color.FromArgb(240, 240, 240),
            ForeColor = dark ? Color.White : Color.FromArgb(50, 50, 50)
        };
        stopBtn.FlatAppearance.BorderSize = 0;
        stopBtn.Click += btnStop_Click;
        stopBtn.MouseEnter += (s, e) => stopBtn.BackColor = dark ? 
            Color.FromArgb(70, 70, 73) : Color.FromArgb(230, 230, 230);
        stopBtn.MouseLeave += (s, e) => stopBtn.BackColor = dark ? 
            Color.FromArgb(60, 60, 63) : Color.FromArgb(240, 240, 240);
        controlPanel.Controls.Add(stopBtn);
        
        controlSection.Controls.Add(controlPanel);
        contentArea.Controls.Add(controlSection);
        
        // Update timer
        uiTimer.Tick -= uiTimer_Tick;
        uiTimer.Tick += (s, e) => {
            var keys = contentArea.Controls.Find("keysValue", true).FirstOrDefault() as Label;
            if (keys != null) keys.Text = _lastKeys.ToString("N0");
            
            var mouse = contentArea.Controls.Find("mouseValue", true).FirstOrDefault() as Label;
            if (mouse != null) mouse.Text = _lastMouse.ToString("N0");
        };
        
        // Add content to main panel
        mainPanel.Controls.Add(contentArea);
        
        // Add main panel to form
        this.Controls.Add(mainPanel);
        
        // Ensure correct z-order  
        mainPanel.BringToFront();
        menuStrip1.BringToFront();
        
        this.ResumeLayout(true);
    }
}
