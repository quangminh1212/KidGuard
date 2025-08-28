using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Core.Services;
using ChildGuard.Core.Events;
using ChildGuard.Hooking;
using ChildGuard.UI.Controls;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Services;
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
        
        // Services
        private readonly ServiceManager _serviceManager;
        private KeyboardMouseMonitor? _keyboardMouseMonitor;
        private NotificationService? _notificationService;
        
        // Stats
        private long _lastKeys;
        private long _lastMouse;
        private long _threatsDetected;
        private int _keystrokeCount;
        private int _mouseClickCount;
        
        // Timers
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Timer animationTimer;
        
        public ModernMainForm()
        {
            _serviceManager = ServiceManager.Instance;
            InitializeForm();
            InitializeComponents();
            LoadConfiguration();
            SetupEventHandlers();
            ApplyTheme();
            InitializeServicesAsync();
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
            // IMPORTANT: Create panels in correct order for proper layout
            // 1. First create content panel (will fill remaining space)
            CreateContentPanel();
            
            // 2. Then create sidebar (will dock to left)
            CreateSidebarPanel();
            
            // 3. Finally create header (will dock to top)
            CreateHeaderPanel();
            
            // Initialize timers
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
            
            animationTimer = new System.Windows.Forms.Timer();
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
            
            var dashboardItem = CreateSidebarItem("Dashboard", "\uE80F", 0); // Home icon
            var monitoringItem = CreateSidebarItem("Monitoring", "\uE7B3", 1); // Eye icon
            var protectionItem = CreateSidebarItem("Protection", "\uEA18", 2); // Shield icon
            var attachmentsItem = CreateSidebarItem("Attachments", "\uE896", 3); // Attachment icon
            var reportsItem = CreateSidebarItem("Reports", "\uE9D9", 4); // Chart icon
            var settingsItem = CreateSidebarItem("Settings", "\uE713", 5); // Settings icon
            
            // Set dashboard as active by default
            SetActiveSidebarItem(dashboardItem);
            
            this.Controls.Add(sidebarPanel);
        }
        
        private SidebarItem CreateSidebarItem(string text, string icon, int index)
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
            // Check if contentPanel is initialized
            if (contentPanel == null)
                return;
                
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
                case "Attachments":
                    LoadAttachmentsContent();
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
            // Use the new DashboardControl
            var dashboardControl = new DashboardControl
            {
                Dock = DockStyle.Fill
            };
            
            // Update protection status
            dashboardControl.UpdateProtectionStatus(_running);
            
            contentPanel.Controls.Add(dashboardControl);
            
            // Refresh data asynchronously
            Task.Run(async () =>
            {
                await dashboardControl.RefreshDataAsync();
            });
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
                Image = CreateIcon("\uE765", 40, ColorScheme.Modern.Primary) // Keyboard icon
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
                Image = CreateIcon("\uE962", 40, ColorScheme.Modern.Success) // Mouse icon
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
                    Location = new Point(80, 10),  // Moved right from 70 to 80
                    Size = new Size(450, 20),  // Set fixed width instead of AutoSize
                    ForeColor = ColorScheme.Modern.TextPrimary
                };
                optionPanel.Controls.Add(titleLbl);
                
                var descLbl = new Label
                {
                    Text = description,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(80, 32),  // Moved right from 70 to 80
                    Size = new Size(450, 20),  // Set fixed width instead of AutoSize
                    ForeColor = ColorScheme.Modern.TextSecondary
                };
                optionPanel.Controls.Add(descLbl);
                
                optionsCard.Controls.Add(optionPanel);
                y += 70;
            }
            
            protectionPanel.Controls.Add(optionsCard);
            
            contentPanel.Controls.Add(protectionPanel);
        }
        
        private void LoadAttachmentsContent()
        {
            // Use the new AttachmentsManager control
            var attachmentsManager = new AttachmentsManager()
            {
                Dock = DockStyle.Fill
            };
            
            contentPanel.Controls.Add(attachmentsManager);
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
            // Use the new SettingsControl
            var settingsControl = new SettingsControl
            {
                Dock = DockStyle.Fill
            };
            
            // Handle settings change event
            settingsControl.OnSettingsChanged += (settings) =>
            {
                // Apply new settings to protection manager
                _config.EnableInputMonitoring = settings.ContainsKey("EnableKeyboard") && (bool)settings["EnableKeyboard"];
                _config.EnableAudioMonitoring = settings.ContainsKey("EnableAudio") && (bool)settings["EnableAudio"];
                _config.BlockScreenshots = settings.ContainsKey("BlockScreenshots") && (bool)settings["BlockScreenshots"];
                _config.CheckUrls = settings.ContainsKey("EnableUrlCheck") && (bool)settings["EnableUrlCheck"];
                _config.BlockInappropriateContent = settings.ContainsKey("EnableBadWords") && (bool)settings["EnableBadWords"];
                
                // Save configuration
                ConfigManager.Save(_config, out _);
                
                // Restart protection if running to apply new settings
                if (_running)
                {
                    StopProtection();
                    StartProtection();
                }
            };
            
            contentPanel.Controls.Add(settingsControl);
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
                if (headerPanel != null)
                    headerPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                if (sidebarPanel != null)
                    sidebarPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                if (contentPanel != null)
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
            _protectionManager.OnActivity += OnActivity;
        }
        
        private void OnActivity(object? sender, ActivityEvent evt)
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
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    LoadContent(activeSidebarItem?.Text ?? "Dashboard");
                }));
            }
            else
            {
                LoadContent(activeSidebarItem?.Text ?? "Dashboard");
            }
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
                keyLabel.Text = _keystrokeCount.ToString("N0");
            
            var mouseLabel = contentPanel.Controls.Find("mouseValueLabel", true).FirstOrDefault() as Label;
            if (mouseLabel != null)
                mouseLabel.Text = _mouseClickCount.ToString("N0");
            
            // Update threat counter
            var threatLabel = contentPanel.Controls.Find("threatValueLabel", true).FirstOrDefault() as Label;
            if (threatLabel != null)
                threatLabel.Text = _threatsDetected.ToString("N0");
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
        
        private Image CreateIcon(string icon, int size, Color color)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                using (var font = new Font("Segoe MDL2 Assets", size * 0.5f))
                using (var brush = new SolidBrush(color))
                {
                    var measured = g.MeasureString(icon, font);
                    g.DrawString(icon, font, brush, (size - measured.Width) / 2, (size - measured.Height) / 2);
                }
            }
            return bitmap;
        }
        
        private async void InitializeServicesAsync()
        {
            try
            {
                // Initialize ServiceManager
                await _serviceManager.InitializeAsync();
                
                // Initialize KeyboardMouseMonitor
                _keyboardMouseMonitor = new KeyboardMouseMonitor();
                _keyboardMouseMonitor.KeystrokeDetected += OnKeystrokeDetected;
                _keyboardMouseMonitor.MouseActivityDetected += OnMouseActivityDetected;
                _serviceManager.RegisterService(_keyboardMouseMonitor);
                
                // Initialize NotificationService
                if (_notificationService == null)
                {
                    _notificationService = new NotificationService();
                    _notificationService.Initialize(this);
                    _serviceManager.RegisterService(_notificationService);
                }
                
                // Subscribe to events from EventDispatcher
                SubscribeToEvents();
                
                // Start monitoring by default (can be configured later)
                _keyboardMouseMonitor.StartMonitoring();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize services: {ex.Message}", 
                    "Initialization Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                );
            }
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to threat events
            _serviceManager.EventDispatcher.Subscribe<BadWordDetectedEvent>((evt) =>
            {
                _threatsDetected++;
                BeginInvoke(new Action(() =>
                {
                    // Show system tray notification
                    _notificationService?.ShowTrayNotification(
                        "Threat Detected",
                        $"Bad word detected: {evt.Word}",
                        NotificationType.Warning
                    );
                }));
            });
            
            _serviceManager.EventDispatcher.Subscribe<UrlDetectedEvent>((evt) =>
            {
                if (!evt.IsSafe)
                {
                    _threatsDetected++;
                    BeginInvoke(new Action(() =>
                    {
                        // Show system tray notification
                        _notificationService?.ShowTrayNotification(
                            "Dangerous URL",
                            $"Blocked access to: {evt.Url}",
                            NotificationType.Error
                        );
                    }));
                }
            });
            
            _serviceManager.EventDispatcher.Subscribe<ProcessBlockedEvent>((evt) =>
            {
                _threatsDetected++;
                BeginInvoke(new Action(() =>
                {
                    // Show system tray notification
                    _notificationService?.ShowTrayNotification(
                        "Process Blocked",
                        $"Blocked: {evt.ProcessName}",
                        NotificationType.Warning
                    );
                }));
            });
        }
        
        private void OnKeystrokeDetected(object? sender, KeystrokeEventArgs e)
        {
            // Update keystroke counter
            _keystrokeCount += e.Text.Length;
            
            // Log to activity list if monitoring panel is active
            if (activeSidebarItem?.Text == "Monitoring")
            {
                BeginInvoke(new Action(() =>
                {
                    var logList = contentPanel.Controls.Find("activityLog", true).FirstOrDefault() as ListBox;
                    if (logList != null)
                    {
                        logList.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Keystroke: {e.Text.Length} chars");
                        if (logList.Items.Count > 100) 
                            logList.Items.RemoveAt(logList.Items.Count - 1);
                    }
                }));
            }
        }
        
        private void OnMouseActivityDetected(object? sender, MouseActivityEventArgs e)
        {
            // Update mouse counter
            if (e.Action == "Click")
            {
                _mouseClickCount++;
            }
            
            // Log significant events
            if (e.Action == "Click" && activeSidebarItem?.Text == "Monitoring")
            {
                BeginInvoke(new Action(() =>
                {
                    var logList = contentPanel.Controls.Find("activityLog", true).FirstOrDefault() as ListBox;
                    if (logList != null)
                    {
                        logList.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Mouse {e.Action}: {e.Button} at ({e.Location.X},{e.Location.Y})");
                        if (logList.Items.Count > 100)
                            logList.Items.RemoveAt(logList.Items.Count - 1);
                    }
                }));
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // Stop monitoring
            _keyboardMouseMonitor?.StopMonitoring();
            
            // Shutdown services
            _serviceManager.ShutdownAsync().GetAwaiter().GetResult();
            
            // Dispose services
            _keyboardMouseMonitor?.Dispose();
            _notificationService?.Dispose();
        }
    }
    
    /// <summary>
    /// Sidebar navigation item
    /// </summary>
    public class SidebarItem : Panel
    {
        private bool isActive;
        private bool isHovered;
        public string Icon { get; set; } = string.Empty;
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
            using (var font = new Font("Segoe MDL2 Assets", 14))
            using (var brush = new SolidBrush(isActive ? ColorScheme.Modern.Primary : ColorScheme.Modern.TextSecondary))
            {
                if (!string.IsNullOrEmpty(Icon))
                    g.DrawString(Icon, font, brush, 20, 14);
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
