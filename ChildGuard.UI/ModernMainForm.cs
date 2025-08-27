using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Controls;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI
{
    /// <summary>
    /// Form chính với giao diện hiện đại theo phong cách Windows 11 và Facebook
    /// </summary>
    public partial class ModernMainForm : Form
    {
        // Panels
        private Panel sidebarPanel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel currentContentPanel;
        
        // Header controls
        private Label titleLabel;
        private PictureBox logoImage;
        private ModernButton profileButton;
        
        // Sidebar items
        private List<SidebarItem> sidebarItems;
        private SidebarItem activeSidebarItem;
        
        // Protection
        private readonly AdvancedProtectionManager _protectionManager = new();
        private volatile bool _running;
        private AppConfig _config = new();
        
        // Stats
        private long _lastKeys;
        private long _lastMouse;
        private long _threatsDetected;
        
        // Timers
        private Timer updateTimer;
        private Timer animationTimer;
        
        public ModernMainForm()
        {
            InitializeForm();
            InitializeComponents();
            LoadConfiguration();
            SetupEventHandlers();
            ApplyTheme();
        }
        
        private void InitializeForm()
        {
            this.Text = "ChildGuard Protection";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.BackColor = ColorScheme.Modern.Background;
            
            // Enable double buffering for smooth animations
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
        }
        
        private void InitializeComponents()
        {
            // Create main layout panels
            CreateHeaderPanel();
            CreateSidebarPanel();
            CreateContentPanel();
            
            // Initialize timers
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
            
            animationTimer = new Timer();
            animationTimer.Interval = 10;
            animationTimer.Tick += AnimationTimer_Tick;
        }
        
        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = ColorScheme.Modern.Surface
            };
            
            // Add shadow
            headerPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var shadowBrush = new LinearGradientBrush(
                    new Point(0, headerPanel.Height - 4),
                    new Point(0, headerPanel.Height),
                    ColorScheme.Modern.ShadowLight,
                    Color.Transparent))
                {
                    g.FillRectangle(shadowBrush, 0, headerPanel.Height - 4, headerPanel.Width, 4);
                }
            };
            
            // Logo
            logoImage = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(15, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            // Tạo logo tạm thời
            logoImage.Image = CreateLogo();
            headerPanel.Controls.Add(logoImage);
            
            // Title
            titleLabel = new Label
            {
                Text = "ChildGuard Protection",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(65, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(titleLabel);
            
            // Profile button (right side)
            profileButton = new ModernButton
            {
                Text = "Admin",
                Size = new Size(100, 36),
                Location = new Point(headerPanel.Width - 120, 12),
                Style = ModernButton.ButtonStyle.Ghost,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            profileButton.Click += ProfileButton_Click;
            headerPanel.Controls.Add(profileButton);
            
            this.Controls.Add(headerPanel);
        }
        
        private void CreateSidebarPanel()
        {
            sidebarPanel = new Panel
            {
                Width = 240,
                Dock = DockStyle.Left,
                BackColor = ColorScheme.Modern.Surface,
                Padding = new Padding(0, 10, 0, 10)
            };
            
            // Add separator line
            sidebarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(ColorScheme.Modern.Border, 1))
                {
                    g.DrawLine(pen, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
                }
            };
            
            // Create sidebar items
            sidebarItems = new List<SidebarItem>();
            
            var dashboardItem = CreateSidebarItem("Dashboard", '🏠', 0);
            var monitoringItem = CreateSidebarItem("Monitoring", '👁', 1);
            var protectionItem = CreateSidebarItem("Protection", '🛡', 2);
            var reportsItem = CreateSidebarItem("Reports", '📊', 3);
            var settingsItem = CreateSidebarItem("Settings", '⚙', 4);
            
            // Set dashboard as active by default
            SetActiveSidebarItem(dashboardItem);
            
            this.Controls.Add(sidebarPanel);
        }
        
        private SidebarItem CreateSidebarItem(string text, char icon, int index)
        {
            var item = new SidebarItem
            {
                Text = text,
                Icon = icon,
                Index = index,
                Size = new Size(220, 48),
                Location = new Point(10, 10 + (index * 52))
            };
            
            item.Click += (s, e) => SetActiveSidebarItem(item);
            
            sidebarPanel.Controls.Add(item);
            sidebarItems.Add(item);
            
            return item;
        }
        
        private void SetActiveSidebarItem(SidebarItem item)
        {
            if (activeSidebarItem != null)
            {
                activeSidebarItem.IsActive = false;
            }
            
            item.IsActive = true;
            activeSidebarItem = item;
            
            // Load corresponding content
            LoadContent(item.Text);
        }
        
        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.Background,
                Padding = new Padding(30)
            };
            
            this.Controls.Add(contentPanel);
        }
        
        private void LoadContent(string section)
        {
            // Clear current content
            contentPanel.Controls.Clear();
            
            switch (section)
            {
                case "Dashboard":
                    LoadDashboardContent();
                    break;
                case "Monitoring":
                    LoadMonitoringContent();
                    break;
                case "Protection":
                    LoadProtectionContent();
                    break;
                case "Reports":
                    LoadReportsContent();
                    break;
                case "Settings":
                    LoadSettingsContent();
                    break;
            }
        }
        
        private void LoadDashboardContent()
        {
            var dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            
            // Title
            var titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            dashboardPanel.Controls.Add(titleLabel);
            
            // Status cards
            var cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 60),
                Size = new Size(800, 120),
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            
            // Protection Status Card
            var statusCard = new ModernHeaderCard
            {
                Title = "Protection Status",
                Subtitle = _running ? "Active" : "Inactive",
                Size = new Size(250, 100),
                Margin = new Padding(0, 0, 20, 0)
            };
            cardsPanel.Controls.Add(statusCard);
            
            // Threats Detected Card
            var threatsCard = new ModernHeaderCard
            {
                Title = "Threats Detected",
                Subtitle = _threatsDetected.ToString(),
                Size = new Size(250, 100),
                Margin = new Padding(0, 0, 20, 0)
            };
            cardsPanel.Controls.Add(threatsCard);
            
            // Activity Card
            var activityCard = new ModernHeaderCard
            {
                Title = "System Activity",
                Subtitle = "Normal",
                Size = new Size(250, 100),
                Margin = new Padding(0, 0, 20, 0)
            };
            cardsPanel.Controls.Add(activityCard);
            
            dashboardPanel.Controls.Add(cardsPanel);
            
            // Quick Actions
            var actionsLabel = new Label
            {
                Text = "Quick Actions",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 200),
                AutoSize = true
            };
            dashboardPanel.Controls.Add(actionsLabel);
            
            var actionsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 240),
                Size = new Size(800, 60),
                FlowDirection = FlowDirection.LeftToRight
            };
            
            var startButton = new ModernButton
            {
                Text = "Start Protection",
                Size = new Size(150, 40),
                Style = ModernButton.ButtonStyle.Primary,
                Margin = new Padding(0, 0, 10, 0)
            };
            startButton.Click += (s, e) => StartProtection();
            actionsPanel.Controls.Add(startButton);
            
            var stopButton = new ModernButton
            {
                Text = "Stop Protection",
                Size = new Size(150, 40),
                Style = ModernButton.ButtonStyle.Danger,
                Margin = new Padding(0, 0, 10, 0)
            };
            stopButton.Click += (s, e) => StopProtection();
            actionsPanel.Controls.Add(stopButton);
            
            var scanButton = new ModernButton
            {
                Text = "Quick Scan",
                Size = new Size(150, 40),
                Style = ModernButton.ButtonStyle.Secondary,
                Margin = new Padding(0, 0, 10, 0)
            };
            actionsPanel.Controls.Add(scanButton);
            
            dashboardPanel.Controls.Add(actionsPanel);
            
            contentPanel.Controls.Add(dashboardPanel);
        }
        
        private void LoadMonitoringContent()
        {
            var monitoringPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            
            var titleLabel = new Label
            {
                Text = "Real-time Monitoring",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            monitoringPanel.Controls.Add(titleLabel);
            
            // Activity cards
            var cardsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 60),
                Size = new Size(800, 150),
                FlowDirection = FlowDirection.LeftToRight
            };
            
            // Keyboard activity
            var keyboardCard = new ModernCard
            {
                Size = new Size(240, 130),
                Margin = new Padding(0, 0, 20, 0)
            };
            
            var keyIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = CreateIcon('⌨', 40, ColorScheme.Modern.Primary)
            };
            keyboardCard.Controls.Add(keyIcon);
            
            var keyLabel = new Label
            {
                Text = "Keyboard",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextSecondary,
                Location = new Point(80, 25),
                AutoSize = true
            };
            keyboardCard.Controls.Add(keyLabel);
            
            var keyValueLabel = new Label
            {
                Name = "keyValueLabel",
                Text = _lastKeys.ToString("N0"),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(80, 45),
                AutoSize = true
            };
            keyboardCard.Controls.Add(keyValueLabel);
            
            cardsPanel.Controls.Add(keyboardCard);
            
            // Mouse activity
            var mouseCard = new ModernCard
            {
                Size = new Size(240, 130),
                Margin = new Padding(0, 0, 20, 0)
            };
            
            var mouseIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = CreateIcon('🖱', 40, ColorScheme.Modern.Success)
            };
            mouseCard.Controls.Add(mouseIcon);
            
            var mouseLabel = new Label
            {
                Text = "Mouse",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorScheme.Modern.TextSecondary,
                Location = new Point(80, 25),
                AutoSize = true
            };
            mouseCard.Controls.Add(mouseLabel);
            
            var mouseValueLabel = new Label
            {
                Name = "mouseValueLabel",
                Text = _lastMouse.ToString("N0"),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(80, 45),
                AutoSize = true
            };
            mouseCard.Controls.Add(mouseValueLabel);
            
            cardsPanel.Controls.Add(mouseCard);
            
            monitoringPanel.Controls.Add(cardsPanel);
            
            // Activity log
            var logLabel = new Label
            {
                Text = "Activity Log",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 230),
                AutoSize = true
            };
            monitoringPanel.Controls.Add(logLabel);
            
            var logCard = new ModernCard
            {
                Location = new Point(0, 270),
                Size = new Size(500, 200)
            };
            
            var logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9),
                BackColor = ColorScheme.Modern.Surface,
                ForeColor = ColorScheme.Modern.TextPrimary
            };
            logCard.Controls.Add(logListBox);
            
            monitoringPanel.Controls.Add(logCard);
            
            contentPanel.Controls.Add(monitoringPanel);
        }
        
        private void LoadProtectionContent()
        {
            var protectionPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            
            var titleLabel = new Label
            {
                Text = "Protection Settings",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            protectionPanel.Controls.Add(titleLabel);
            
            // Protection options
            var optionsCard = new ModernCard
            {
                Location = new Point(0, 60),
                Size = new Size(600, 400)
            };
            
            // Options list
            var options = new[]
            {
                ("Real-time Protection", "Monitor and block threats in real-time"),
                ("Web Protection", "Block malicious websites and downloads"),
                ("Application Control", "Monitor and control application access"),
                ("Content Filtering", "Filter inappropriate content"),
                ("Screen Time Limits", "Set daily usage limits")
            };
            
            int y = 20;
            foreach (var (title, description) in options)
            {
                var optionPanel = new Panel
                {
                    Location = new Point(20, y),
                    Size = new Size(560, 60)
                };
                
                var toggle = new ToggleSwitch
                {
                    Location = new Point(0, 15),
                    Checked = true
                };
                optionPanel.Controls.Add(toggle);
                
                var titleLbl = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(70, 10),
                    AutoSize = true,
                    ForeColor = ColorScheme.Modern.TextPrimary
                };
                optionPanel.Controls.Add(titleLbl);
                
                var descLbl = new Label
                {
                    Text = description,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(70, 32),
                    AutoSize = true,
                    ForeColor = ColorScheme.Modern.TextSecondary
                };
                optionPanel.Controls.Add(descLbl);
                
                optionsCard.Controls.Add(optionPanel);
                y += 70;
            }
            
            protectionPanel.Controls.Add(optionsCard);
            
            contentPanel.Controls.Add(protectionPanel);
        }
        
        private void LoadReportsContent()
        {
            var reportsPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            
            var titleLabel = new Label
            {
                Text = "Reports & Analytics",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            reportsPanel.Controls.Add(titleLabel);
            
            contentPanel.Controls.Add(reportsPanel);
        }
        
        private void LoadSettingsContent()
        {
            var settingsPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            
            var titleLabel = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            settingsPanel.Controls.Add(titleLabel);
            
            contentPanel.Controls.Add(settingsPanel);
        }
        
        private void LoadConfiguration()
        {
            _config = ConfigManager.Load(out _);
            UIStrings.SetLanguage(_config.UILanguage);
        }
        
        private void ApplyTheme()
        {
            var isDark = ParseTheme(_config.Theme) == ThemeMode.Dark ||
                        (ParseTheme(_config.Theme) == ThemeMode.System && ThemeHelper.IsSystemDark());
            
            if (isDark)
            {
                // Apply dark theme colors
                this.BackColor = ColorScheme.Windows11Dark.Background;
                headerPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                sidebarPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                contentPanel.BackColor = ColorScheme.Windows11Dark.Background;
            }
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
        
        private void SetupEventHandlers()
        {
            _protectionManager.ActivityReceived += OnActivity;
        }
        
        private void OnActivity(ActivityEvent evt)
        {
            if (evt.Data is InputActivitySummary s)
            {
                Interlocked.Exchange(ref _lastKeys, s.KeyPressCount);
                Interlocked.Exchange(ref _lastMouse, s.MouseEventCount);
            }
        }
        
        private void StartProtection()
        {
            if (_running) return;
            _protectionManager.Start(_config);
            _running = true;
            UpdateStatus();
        }
        
        private void StopProtection()
        {
            if (!_running) return;
            _protectionManager.Stop();
            _running = false;
            UpdateStatus();
        }
        
        private void UpdateStatus()
        {
            // Update UI based on protection status
            this.BeginInvoke(new Action(() =>
            {
                LoadContent(activeSidebarItem?.Text ?? "Dashboard");
            }));
        }
        
        private void ProfileButton_Click(object sender, EventArgs e)
        {
            // Show profile menu
            var menu = new ContextMenuStrip();
            menu.Items.Add("Profile Settings");
            menu.Items.Add("Account");
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Sign Out");
            menu.Show(profileButton, new Point(0, profileButton.Height));
        }
        
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Update monitoring values
            var keyLabel = contentPanel.Controls.Find("keyValueLabel", true).FirstOrDefault() as Label;
            if (keyLabel != null)
                keyLabel.Text = _lastKeys.ToString("N0");
            
            var mouseLabel = contentPanel.Controls.Find("mouseValueLabel", true).FirstOrDefault() as Label;
            if (mouseLabel != null)
                mouseLabel.Text = _lastMouse.ToString("N0");
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Handle animations
        }
        
        private Image CreateLogo()
        {
            var bitmap = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Draw shield icon
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillEllipse(brush, 5, 5, 30, 30);
                }
                
                using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var text = "C";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (40 - size.Width) / 2, (40 - size.Height) / 2);
                }
            }
            return bitmap;
        }
        
        private Image CreateIcon(char icon, int size, Color color)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                using (var font = new Font("Segoe UI Emoji", size * 0.7f))
                using (var brush = new SolidBrush(color))
                {
                    var text = icon.ToString();
                    var measured = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (size - measured.Width) / 2, (size - measured.Height) / 2);
                }
            }
            return bitmap;
        }
    }
    
    /// <summary>
    /// Sidebar navigation item
    /// </summary>
    public class SidebarItem : Panel
    {
        private bool isActive;
        private bool isHovered;
        public char Icon { get; set; }
        public int Index { get; set; }
        public new string Text { get; set; }
        
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                UpdateAppearance();
            }
        }
        
        public SidebarItem()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
            
            Cursor = Cursors.Hand;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Background
            if (isActive)
            {
                using (var brush = new SolidBrush(ColorScheme.Modern.PrimaryLight))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
                
                // Active indicator
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillRectangle(brush, 0, 8, 3, Height - 16);
                }
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(ColorScheme.Modern.HoverOverlay))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
            
            // Icon
            using (var font = new Font("Segoe UI Emoji", 16))
            using (var brush = new SolidBrush(isActive ? ColorScheme.Modern.Primary : ColorScheme.Modern.TextSecondary))
            {
                g.DrawString(Icon.ToString(), font, brush, 20, 12);
            }
            
            // Text
            using (var font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular))
            using (var brush = new SolidBrush(isActive ? ColorScheme.Modern.Primary : ColorScheme.Modern.TextPrimary))
            {
                g.DrawString(Text, font, brush, 55, 15);
            }
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }
        
        private void UpdateAppearance()
        {
            Invalidate();
        }
    }
}
