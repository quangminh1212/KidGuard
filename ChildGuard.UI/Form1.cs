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

    private void RebuildLayoutModern(ThemeMode mode)
    {
        bool dark = mode == ThemeMode.Dark || (mode == ThemeMode.System && ThemeHelper.IsSystemDark());

        // Clear existing controls but preserve important ones
        this.SuspendLayout();
        var menu = this.menuStrip1;
        var existingControls = new List<Control>();
        foreach (Control c in this.Controls)
        {
            if (c == menu || c == lblKeys || c == lblMouse || c == btnStart || c == btnStop || c == chkEnableInput)
            {
                existingControls.Add(c);
            }
        }
        this.Controls.Clear();
        
        // Setup menu at top
        menu.Dock = DockStyle.Top;
        this.Controls.Add(menu);
        this.MainMenuStrip = menu;

        // Create main container panel
        var mainContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        
        // Optional sidebar navigation
        if (_cfg.UseSidebarNavigation)
        {
            var sidebar = new Panel 
            { 
                Dock = DockStyle.Left, 
                Width = 200, 
                BackColor = dark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(245, 245, 245),
                Padding = new Padding(8)
            };
            
            var sidebarFlow = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.TopDown, 
                AutoScroll = true
            };

            void AddSidebarButton(string text, char icon, EventHandler onClick)
            {
                var btn = new Button 
                { 
                    Text = "  " + text,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    Width = 180,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 0, 4),
                    Cursor = Cursors.Hand
                };
                btn.Image = GlyphIcons.Render(icon, 16, ThemeHelper.GetAccentColor());
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                btn.Click += onClick;
                ModernStyle.MakeSecondary(btn, dark);
                sidebarFlow.Controls.Add(btn);
            }

            AddSidebarButton(UIStrings.Get("Menu.Settings"), GlyphIcons.Settings, 
                (s,e) => { using var dlg = new SettingsForm(); dlg.ShowDialog(this); });
            AddSidebarButton(UIStrings.Get("Menu.Reports"), GlyphIcons.Reports, 
                (s,e) => { using var dlg = new ReportsForm(); dlg.ShowDialog(this); });
            AddSidebarButton(UIStrings.Get("Menu.PolicyEditor"), GlyphIcons.Policy, 
                (s,e) => { using var dlg = new PolicyEditorForm(); dlg.ShowDialog(this); });
            
            sidebar.Controls.Add(sidebarFlow);
            this.Controls.Add(sidebar);
        }

        // Main content area
        var contentPanel = new Panel 
        { 
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            AutoScroll = true
        };

        // Title section
        var titleLabel = new Label
        {
            Text = UIStrings.Get("Form1.Section.Activity"),
            Font = new Font("Segoe UI Variable Display", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        contentPanel.Controls.Add(titleLabel);

        // Stats Cards Container
        var cardsPanel = new FlowLayoutPanel
        {
            Location = new Point(0, 50),
            Size = new Size(500, 100),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        // Keys Card
        var keyCard = new Panel
        {
            Size = new Size(220, 80),
            BackColor = dark ? Color.FromArgb(45, 45, 48) : Color.White,
            Margin = new Padding(0, 0, 16, 0)
        };
        if (!dark) keyCard.BorderStyle = BorderStyle.FixedSingle;
        
        var keyIcon = new PictureBox 
        { 
            Image = GlyphIcons.Render(GlyphIcons.Keyboard, 24, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Size = new Size(32, 32),
            Location = new Point(16, 24)
        };
        
        var keyLabel = new Label
        {
            Text = UIStrings.Get("Form1.Card.Keys"),
            Font = new Font("Segoe UI", 9),
            ForeColor = dark ? Color.LightGray : Color.Gray,
            Location = new Point(56, 16),
            AutoSize = true
        };
        
        lblKeys.Text = "0";
        lblKeys.Font = new Font("Segoe UI Variable Display", 16, FontStyle.Bold);
        lblKeys.Location = new Point(56, 36);
        lblKeys.AutoSize = true;
        
        keyCard.Controls.AddRange(new Control[] { keyIcon, keyLabel, lblKeys });
        
        // Mouse Card  
        var mouseCard = new Panel
        {
            Size = new Size(220, 80),
            BackColor = dark ? Color.FromArgb(45, 45, 48) : Color.White
        };
        if (!dark) mouseCard.BorderStyle = BorderStyle.FixedSingle;
        
        var mouseIcon = new PictureBox
        {
            Image = GlyphIcons.Render(GlyphIcons.Mouse, 24, ThemeHelper.GetAccentColor()),
            SizeMode = PictureBoxSizeMode.CenterImage, 
            Size = new Size(32, 32),
            Location = new Point(16, 24)
        };
        
        var mouseLabel = new Label
        {
            Text = UIStrings.Get("Form1.Card.Mouse"),
            Font = new Font("Segoe UI", 9),
            ForeColor = dark ? Color.LightGray : Color.Gray,
            Location = new Point(56, 16),
            AutoSize = true
        };
        
        lblMouse.Text = "0";
        lblMouse.Font = new Font("Segoe UI Variable Display", 16, FontStyle.Bold);
        lblMouse.Location = new Point(56, 36);
        lblMouse.AutoSize = true;
        
        mouseCard.Controls.AddRange(new Control[] { mouseIcon, mouseLabel, lblMouse });
        
        cardsPanel.Controls.Add(keyCard);
        cardsPanel.Controls.Add(mouseCard);
        contentPanel.Controls.Add(cardsPanel);

        // Controls Section
        var controlsTitle = new Label
        {
            Text = UIStrings.Get("Form1.Section.Controls"),
            Font = new Font("Segoe UI Variable Display", 14, FontStyle.Bold),
            Location = new Point(0, 160),
            AutoSize = true
        };
        contentPanel.Controls.Add(controlsTitle);

        // Control panel
        var controlPanel = new Panel
        {
            Location = new Point(0, 200),
            Size = new Size(500, 60),
            BackColor = dark ? Color.FromArgb(40, 40, 43) : Color.FromArgb(250, 250, 250),
            Padding = new Padding(16)
        };
        
        // Enable monitoring checkbox
        chkEnableInput.Text = UIStrings.Get("Form1.EnableInput");
        chkEnableInput.Location = new Point(16, 18);
        chkEnableInput.AutoSize = true;
        chkEnableInput.Font = new Font("Segoe UI", 10);
        
        // Start button
        btnStart.Text = UIStrings.Get("Buttons.Start");
        btnStart.Location = new Point(250, 12);
        btnStart.Size = new Size(100, 36);
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.Cursor = Cursors.Hand;
        ModernStyle.MakePrimary(btnStart, dark);
        
        // Stop button  
        btnStop.Text = UIStrings.Get("Buttons.Stop");
        btnStop.Location = new Point(360, 12);
        btnStop.Size = new Size(100, 36);
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.Cursor = Cursors.Hand;
        ModernStyle.MakeSecondary(btnStop, dark);
        
        controlPanel.Controls.AddRange(new Control[] { chkEnableInput, btnStart, btnStop });
        contentPanel.Controls.Add(controlPanel);
        
        mainContainer.Controls.Add(contentPanel);
        this.Controls.Add(mainContainer);
        
        // Buttons already styled above, no need to re-apply
        
        // Set minimum form size
        this.MinimumSize = new Size(600, 400);
        if (this.Width < 600) this.Width = 600;
        if (this.Height < 400) this.Height = 400;
        
        this.ResumeLayout(true);
    }
}
