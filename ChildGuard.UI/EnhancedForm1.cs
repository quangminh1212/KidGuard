using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI;

public partial class EnhancedForm1 : Form
{
    private readonly AdvancedProtectionManager _protectionManager = new();
    private long _totalKeys;
    private long _totalMouse;
    private int _threatsDetected;
    private volatile bool _running;
    private AppConfig _config = new();
    private readonly ConcurrentQueue<ThreatNotification> _threatQueue = new();
    
    // UI Elements
    private Panel _mainPanel;
    private Panel _threatPanel;
    private ListBox _activityLog;
    private Label _statusLabel;
    private Label _keyValueLabel;
    private Label _mouseValueLabel;
    private Label _threatCountLabel;
    private Button _startButton;
    private Button _stopButton;
    private CheckBox _audioCheckBox;
    private CheckBox _urlCheckBox;
    private CheckBox _contentCheckBox;
    private System.Windows.Forms.Timer _uiUpdateTimer;
    private MenuStrip _menuStrip;
    
    public EnhancedForm1()
    {
        InitializeComponent();
        SetupProtectionManager();
        SetupUI();
        LoadConfiguration();
    }
    
    private void InitializeComponent()
    {
        this.Text = "ChildGuard Protection Suite";
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);
        
        _menuStrip = new MenuStrip();
        _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _uiUpdateTimer.Tick += UpdateUI;
        
        this.Controls.Add(_menuStrip);
        this.MainMenuStrip = _menuStrip;
    }
    
    private void SetupProtectionManager()
    {
        _protectionManager.OnThreatDetected += OnThreatDetected;
        _protectionManager.OnStatisticsUpdated += OnStatisticsUpdated;
        _protectionManager.OnActivity += OnActivityDetected;
    }
    
    private void SetupUI()
    {
        var isDark = ThemeHelper.IsSystemDark();
        
        this.BackColor = isDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(250, 250, 250);
        this.ForeColor = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(40, 40, 40);
        
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            BackColor = Color.Transparent
        };
        
        // Create layout with three columns
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 300,
            IsSplitterFixed = true,
            BorderStyle = BorderStyle.None
        };
        
        // Left panel - Statistics
        var statsPanel = CreateStatisticsPanel(isDark);
        splitContainer.Panel1.Controls.Add(statsPanel);
        
        // Right panel - split horizontally
        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300,
            BorderStyle = BorderStyle.None
        };
        
        // Top right - Threats
        _threatPanel = CreateThreatPanel(isDark);
        rightSplit.Panel1.Controls.Add(_threatPanel);
        
        // Bottom right - Activity Log
        var logPanel = CreateActivityLogPanel(isDark);
        rightSplit.Panel2.Controls.Add(logPanel);
        
        splitContainer.Panel2.Controls.Add(rightSplit);
        _mainPanel.Controls.Add(splitContainer);
        
        // Control bar at bottom
        var controlBar = CreateControlBar(isDark);
        _mainPanel.Controls.Add(controlBar);
        
        this.Controls.Add(_mainPanel);
        _mainPanel.BringToFront();
    }
    
    private Panel CreateStatisticsPanel(bool isDark)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            BackColor = isDark ? Color.FromArgb(38, 38, 41) : Color.White
        };
        
        var title = new Label
        {
            Text = "Protection Statistics",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = ThemeHelper.GetAccentColor(),
            Location = new Point(15, 15),
            AutoSize = true
        };
        panel.Controls.Add(title);
        
        // Keyboard stat card
        var keyCard = CreateStatCard("Keyboard Activity", "🔤", 15, 60, isDark);
        _keyValueLabel = new Label
        {
            Name = "keyValue",
            Text = "0",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = this.ForeColor,
            Location = new Point(20, 30),
            AutoSize = true
        };
        keyCard.Controls.Add(_keyValueLabel);
        panel.Controls.Add(keyCard);
        
        // Mouse stat card
        var mouseCard = CreateStatCard("Mouse Activity", "🖱️", 15, 160, isDark);
        _mouseValueLabel = new Label
        {
            Name = "mouseValue",
            Text = "0",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = this.ForeColor,
            Location = new Point(20, 30),
            AutoSize = true
        };
        mouseCard.Controls.Add(_mouseValueLabel);
        panel.Controls.Add(mouseCard);
        
        // Threats stat card
        var threatCard = CreateStatCard("Threats Detected", "⚠️", 15, 260, isDark);
        _threatCountLabel = new Label
        {
            Name = "threatValue",
            Text = "0",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 100, 100),
            Location = new Point(20, 30),
            AutoSize = true
        };
        threatCard.Controls.Add(_threatCountLabel);
        panel.Controls.Add(threatCard);
        
        // Status indicator
        _statusLabel = new Label
        {
            Text = "● Protection Inactive",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(15, 380),
            AutoSize = true
        };
        panel.Controls.Add(_statusLabel);
        
        return panel;
    }
    
    private Panel CreateStatCard(string title, string icon, int x, int y, bool isDark)
    {
        var card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(260, 80),
            BackColor = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245)
        };
        
        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Rounded corners
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            int radius = 10;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            
            using var brush = new SolidBrush(card.BackColor);
            g.FillPath(brush, path);
            
            using var pen = new Pen(isDark ? Color.FromArgb(60, 60, 63) : Color.FromArgb(230, 230, 230), 1);
            g.DrawPath(pen, path);
            
            // Icon
            using var iconFont = new Font("Segoe UI", 20);
            g.DrawString(icon, iconFont, new SolidBrush(ThemeHelper.GetAccentColor()), 210, 25);
        };
        
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(20, 10),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        card.Controls.Add(titleLabel);
        
        return card;
    }
    
    private Panel CreateThreatPanel(bool isDark)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            BackColor = isDark ? Color.FromArgb(38, 38, 41) : Color.White
        };
        
        var title = new Label
        {
            Text = "🛡️ Threat Detection",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 100, 100),
            Location = new Point(15, 15),
            AutoSize = true
        };
        panel.Controls.Add(title);
        
        var threatList = new FlowLayoutPanel
        {
            Location = new Point(15, 50),
            Size = new Size(panel.Width - 30, panel.Height - 70),
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };
        
        panel.Controls.Add(threatList);
        panel.Tag = threatList; // Store reference for adding threats
        
        return panel;
    }
    
    private Panel CreateActivityLogPanel(bool isDark)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            BackColor = isDark ? Color.FromArgb(38, 38, 41) : Color.White
        };
        
        var title = new Label
        {
            Text = "📋 Activity Log",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = ThemeHelper.GetAccentColor(),
            Location = new Point(15, 15),
            AutoSize = true
        };
        panel.Controls.Add(title);
        
        _activityLog = new ListBox
        {
            Location = new Point(15, 50),
            Size = new Size(panel.Width - 30, panel.Height - 70),
            Font = new Font("Consolas", 9),
            BackColor = isDark ? Color.FromArgb(30, 30, 33) : Color.FromArgb(250, 250, 250),
            ForeColor = this.ForeColor,
            BorderStyle = BorderStyle.None,
            ScrollAlwaysVisible = true
        };
        
        panel.Controls.Add(_activityLog);
        
        return panel;
    }
    
    private Panel CreateControlBar(bool isDark)
    {
        var bar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240),
            Padding = new Padding(20, 10, 20, 10)
        };
        
        // Protection options
        var optionsPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 10),
            Size = new Size(400, 60),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };
        
        _audioCheckBox = new CheckBox
        {
            Text = "Audio Monitor",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 20, 0)
        };
        optionsPanel.Controls.Add(_audioCheckBox);
        
        _urlCheckBox = new CheckBox
        {
            Text = "URL Safety",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 20, 0)
        };
        optionsPanel.Controls.Add(_urlCheckBox);
        
        _contentCheckBox = new CheckBox
        {
            Text = "Content Filter",
            Checked = true,
            AutoSize = true
        };
        optionsPanel.Controls.Add(_contentCheckBox);
        
        bar.Controls.Add(optionsPanel);
        
        // Control buttons
        _stopButton = new Button
        {
            Text = "⏹ Stop Protection",
            Size = new Size(150, 40),
            Location = new Point(bar.Width - 330, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(200, 60, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _stopButton.FlatAppearance.BorderSize = 0;
        _stopButton.Click += StopProtection;
        bar.Controls.Add(_stopButton);
        
        _startButton = new Button
        {
            Text = "▶ Start Protection",
            Size = new Size(150, 40),
            Location = new Point(bar.Width - 170, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 180, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _startButton.FlatAppearance.BorderSize = 0;
        _startButton.Click += StartProtection;
        bar.Controls.Add(_startButton);
        
        return bar;
    }
    
    private void LoadConfiguration()
    {
        try
        {
            _config = ConfigManager.Load(out _);
            _audioCheckBox.Checked = _config.EnableAudioMonitoring;
        }
        catch
        {
            _config = new AppConfig();
        }
    }
    
    private void StartProtection(object sender, EventArgs e)
    {
        if (_running) return;
        
        _config.EnableInputMonitoring = true;
        _config.EnableAudioMonitoring = _audioCheckBox.Checked;
        _config.BlockInappropriateContent = _contentCheckBox.Checked;
        _config.CheckUrls = _urlCheckBox.Checked;
        
        _protectionManager.Start(_config);
        _running = true;
        
        _startButton.Enabled = false;
        _stopButton.Enabled = true;
        _statusLabel.Text = "● Protection Active";
        _statusLabel.ForeColor = Color.FromArgb(60, 180, 60);
        
        _uiUpdateTimer.Start();
        LogActivity("Protection started");
    }
    
    private void StopProtection(object sender, EventArgs e)
    {
        if (!_running) return;
        
        _protectionManager.Stop();
        _running = false;
        
        _startButton.Enabled = true;
        _stopButton.Enabled = false;
        _statusLabel.Text = "● Protection Inactive";
        _statusLabel.ForeColor = Color.Gray;
        
        _uiUpdateTimer.Stop();
        LogActivity("Protection stopped");
    }
    
    private void OnThreatDetected(object sender, ThreatDetectedEventArgs e)
    {
        _threatsDetected++;
        
        var notification = new ThreatNotification
        {
            Timestamp = e.Timestamp,
            Type = e.Type.ToString(),
            Description = e.Description,
            Level = e.Level,
            Content = e.Content
        };
        
        _threatQueue.Enqueue(notification);
        
        // Keep only last 10 threats in queue
        while (_threatQueue.Count > 10)
            _threatQueue.TryDequeue(out _);
        
        LogActivity($"THREAT: {e.Type} - {e.Description}");
    }
    
    private void OnStatisticsUpdated(object sender, StatisticsUpdatedEventArgs e)
    {
        Interlocked.Exchange(ref _totalKeys, e.TotalKeysPressed);
        Interlocked.Exchange(ref _totalMouse, e.TotalMouseClicks);
    }
    
    private void OnActivityDetected(object sender, ActivityEvent e)
    {
        // ActivityEvent is a record with Type and Data properties
        LogActivity($"{e.Type}: {e.Data}");
    }
    
    private void LogActivity(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => LogActivity(message)));
            return;
        }
        
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _activityLog.Items.Insert(0, $"[{timestamp}] {message}");
        
        // Keep only last 100 items
        while (_activityLog.Items.Count > 100)
            _activityLog.Items.RemoveAt(_activityLog.Items.Count - 1);
    }
    
    private void UpdateUI(object sender, EventArgs e)
    {
        _keyValueLabel.Text = _totalKeys.ToString("N0");
        _mouseValueLabel.Text = _totalMouse.ToString("N0");
        _threatCountLabel.Text = _threatsDetected.ToString();
        
        // Update threat panel
        if (_threatQueue.TryDequeue(out var threat))
        {
            AddThreatNotification(threat);
        }
    }
    
    private void AddThreatNotification(ThreatNotification threat)
    {
        if (_threatPanel.Tag is FlowLayoutPanel threatList)
        {
            var notification = CreateThreatNotificationCard(threat);
            threatList.Controls.Add(notification);
            
            // Keep only last 5 notifications visible
            while (threatList.Controls.Count > 5)
                threatList.Controls.RemoveAt(0);
        }
    }
    
    private Panel CreateThreatNotificationCard(ThreatNotification threat)
    {
        var isDark = ThemeHelper.IsSystemDark();
        var card = new Panel
        {
            Size = new Size(800, 60),
            Margin = new Padding(0, 0, 0, 10),
            BackColor = GetThreatColor(threat.Level, isDark)
        };
        
        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            int radius = 8;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            
            using var brush = new SolidBrush(card.BackColor);
            g.FillPath(brush, path);
        };
        
        var icon = new Label
        {
            Text = GetThreatIcon(threat.Level),
            Font = new Font("Segoe UI", 16),
            Location = new Point(10, 15),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        card.Controls.Add(icon);
        
        var title = new Label
        {
            Text = threat.Type,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(50, 10),
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.White
        };
        card.Controls.Add(title);
        
        var desc = new Label
        {
            Text = threat.Description,
            Font = new Font("Segoe UI", 9),
            Location = new Point(50, 30),
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(230, 230, 230)
        };
        card.Controls.Add(desc);
        
        var time = new Label
        {
            Text = threat.Timestamp.ToString("HH:mm:ss"),
            Font = new Font("Segoe UI", 8),
            Location = new Point(card.Width - 80, 20),
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(200, 200, 200)
        };
        card.Controls.Add(time);
        
        return card;
    }
    
    private Color GetThreatColor(ThreatLevel level, bool isDark)
    {
        return level switch
        {
            ThreatLevel.Critical => Color.FromArgb(180, 30, 30),
            ThreatLevel.High => Color.FromArgb(200, 60, 60),
            ThreatLevel.Medium => Color.FromArgb(200, 120, 60),
            ThreatLevel.Low => Color.FromArgb(180, 180, 60),
            _ => isDark ? Color.FromArgb(60, 60, 63) : Color.FromArgb(200, 200, 200)
        };
    }
    
    private string GetThreatIcon(ThreatLevel level)
    {
        return level switch
        {
            ThreatLevel.Critical => "🚨",
            ThreatLevel.High => "⚠️",
            ThreatLevel.Medium => "⚡",
            ThreatLevel.Low => "ℹ️",
            _ => "●"
        };
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (_running)
        {
            _protectionManager.Stop();
        }
        _protectionManager.Dispose();
        _uiUpdateTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

public class ThreatNotification
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public ThreatLevel Level { get; set; }
    public string Content { get; set; }
}
