using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ChildGuard.Core.Audio;
using ChildGuard.Core.Detection;

namespace TestAudioMonitor;

public class ModernAudioMonitorForm : Form
{
    // Core components
    private EnhancedAudioMonitor _monitor;
    private Timer _animationTimer;
    private Timer _updateTimer;
    
    // UI Elements
    private Panel _headerPanel;
    private Panel _sidePanel;
    private Panel _mainContentPanel;
    private Panel _statusPanel;
    private ModernButton _startBtn;
    private ModernButton _stopBtn;
    private ModernButton _settingsBtn;
    private ModernTextBox _badWordsInput;
    private ModernButton _addWordsBtn;
    private RichTextBox _logBox;
    private ModernListBox _detectedList;
    private AudioLevelIndicator _audioLevel;
    private PulseAnimation _pulseAnimation;
    private Label _statusLabel;
    private Label _titleLabel;
    private Label _detectionCountLabel;
    private Panel _quickStatsPanel;
    
    // Colors - Material Design Palette
    private readonly Color _primaryColor = Color.FromArgb(33, 150, 243); // Blue
    private readonly Color _primaryDark = Color.FromArgb(25, 118, 210);
    private readonly Color _accent = Color.FromArgb(255, 87, 34); // Deep Orange
    private readonly Color _success = Color.FromArgb(76, 175, 80); // Green
    private readonly Color _warning = Color.FromArgb(255, 152, 0); // Orange
    private readonly Color _danger = Color.FromArgb(244, 67, 54); // Red
    private readonly Color _textPrimary = Color.FromArgb(33, 33, 33);
    private readonly Color _textSecondary = Color.FromArgb(117, 117, 117);
    private readonly Color _background = Color.FromArgb(250, 250, 250);
    private readonly Color _surface = Color.White;
    
    // State
    private bool _isMonitoring = false;
    private int _detectionCount = 0;
    private List<string> _detectedPhrases = new List<string>();
    
    public ModernAudioMonitorForm()
    {
        InitializeModernUI();
        InitializeMonitor();
        SetupAnimations();
    }
    
    private void InitializeModernUI()
    {
        // Form settings
        Text = "ChildGuard Audio Monitor";
        Size = new Size(1200, 750);
        MinimumSize = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = _background;
        Font = new Font("Segoe UI", 9F);
        
        // Remove default border for custom design
        FormBorderStyle = FormBorderStyle.None;
        
        // Custom window controls
        CreateCustomWindowControls();
        
        // Header Panel
        CreateHeaderPanel();
        
        // Side Panel (Navigation)
        CreateSidePanel();
        
        // Main Content Area
        CreateMainContent();
        
        // Status Bar
        CreateStatusBar();
        
        // Enable double buffering for smooth animations
        SetStyle(ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint | 
                ControlStyles.DoubleBuffer | 
                ControlStyles.ResizeRedraw, true);
    }
    
    private void CreateCustomWindowControls()
    {
        // Title bar with custom controls
        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32,
            BackColor = _primaryDark
        };
        
        // Window control buttons
        var closeBtn = CreateWindowButton("×", _danger);
        closeBtn.Location = new Point(Width - 45, 0);
        closeBtn.Click += (s, e) => Close();
        
        var maximizeBtn = CreateWindowButton("□", _textSecondary);
        maximizeBtn.Location = new Point(Width - 90, 0);
        maximizeBtn.Click += (s, e) =>
        {
            WindowState = WindowState == FormWindowState.Maximized ? 
                         FormWindowState.Normal : FormWindowState.Maximized;
        };
        
        var minimizeBtn = CreateWindowButton("─", _textSecondary);
        minimizeBtn.Location = new Point(Width - 135, 0);
        minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;
        
        // Title
        var title = new Label
        {
            Text = "🛡️ ChildGuard Audio Monitor",
            ForeColor = Color.White,
            Location = new Point(10, 6),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        
        titleBar.Controls.AddRange(new Control[] { title, minimizeBtn, maximizeBtn, closeBtn });
        Controls.Add(titleBar);
        
        // Make window draggable
        MakeWindowDraggable(titleBar);
    }
    
    private Button CreateWindowButton(string text, Color hoverColor)
    {
        return new Button
        {
            Text = text,
            Size = new Size(45, 32),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 12F),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = hoverColor }
        };
    }
    
    private void MakeWindowDraggable(Control control)
    {
        bool dragging = false;
        Point dragCursorPoint = Point.Empty;
        Point dragFormPoint = Point.Empty;
        
        control.MouseDown += (s, e) =>
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        };
        
        control.MouseMove += (s, e) =>
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(dif));
            }
        };
        
        control.MouseUp += (s, e) => dragging = false;
    }
    
    private void CreateHeaderPanel()
    {
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = _primaryColor,
            Padding = new Padding(30, 20, 30, 20)
        };
        _headerPanel.Location = new Point(0, 32);
        
        // Create gradient background
        _headerPanel.Paint += (s, e) =>
        {
            using (var brush = new LinearGradientBrush(
                _headerPanel.ClientRectangle,
                _primaryColor,
                _primaryDark,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, _headerPanel.ClientRectangle);
            }
        };
        
        // Title and subtitle
        _titleLabel = new Label
        {
            Text = "Audio Monitoring System",
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        
        var subtitleLabel = new Label
        {
            Text = "Real-time speech recognition and content filtering",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(220, 220, 220),
            AutoSize = true,
            Location = new Point(30, 60)
        };
        
        // Quick stats
        _quickStatsPanel = new Panel
        {
            Size = new Size(300, 60),
            Location = new Point(Width - 350, 30),
            BackColor = Color.FromArgb(50, 255, 255, 255)
        };
        
        _detectionCountLabel = new Label
        {
            Text = "0",
            Font = new Font("Segoe UI", 28F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 5)
        };
        
        var detectionLabel = new Label
        {
            Text = "Detections",
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(220, 220, 220),
            AutoSize = true,
            Location = new Point(10, 40)
        };
        
        _quickStatsPanel.Controls.AddRange(new Control[] { _detectionCountLabel, detectionLabel });
        _headerPanel.Controls.AddRange(new Control[] { _titleLabel, subtitleLabel, _quickStatsPanel });
        
        Controls.Add(_headerPanel);
    }
    
    private void CreateSidePanel()
    {
        _sidePanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 280,
            BackColor = _surface,
            Padding = new Padding(20)
        };
        _sidePanel.Location = new Point(0, 152);
        
        // Add shadow effect
        _sidePanel.Paint += (s, e) =>
        {
            var rect = new Rectangle(Width - 3, 0, 3, Height);
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(30, 0, 0, 0),
                Color.Transparent,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        };
        
        // Control section
        var controlLabel = CreateSectionLabel("CONTROLS", 20, 20);
        
        // Start button with icon
        _startBtn = new ModernButton
        {
            Text = "▶ Start Monitoring",
            Location = new Point(20, 50),
            Size = new Size(240, 45),
            BackColor = _success,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        _startBtn.Click += async (s, e) => await StartMonitoring();
        
        // Stop button
        _stopBtn = new ModernButton
        {
            Text = "■ Stop Monitoring",
            Location = new Point(20, 105),
            Size = new Size(240, 45),
            BackColor = _danger,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Enabled = false
        };
        _stopBtn.Click += (s, e) => StopMonitoring();
        
        // Settings button
        _settingsBtn = new ModernButton
        {
            Text = "⚙ Settings",
            Location = new Point(20, 160),
            Size = new Size(240, 45),
            BackColor = _primaryColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
        _settingsBtn.Click += (s, e) => ShowSettings();
        
        // Bad words section
        var badWordsLabel = CreateSectionLabel("BAD WORDS FILTER", 20, 230);
        
        _badWordsInput = new ModernTextBox
        {
            Location = new Point(20, 260),
            Size = new Size(240, 35),
            PlaceholderText = "Enter words (comma separated)",
            Font = new Font("Segoe UI", 9F)
        };
        
        _addWordsBtn = new ModernButton
        {
            Text = "+ Add Words",
            Location = new Point(20, 305),
            Size = new Size(240, 40),
            BackColor = _accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        _addWordsBtn.Click += (s, e) => AddBadWords();
        
        // Audio level indicator
        var audioLabel = CreateSectionLabel("AUDIO LEVEL", 20, 370);
        
        _audioLevel = new AudioLevelIndicator
        {
            Location = new Point(20, 400),
            Size = new Size(240, 80),
            BackColor = _surface
        };
        
        // Status indicator
        _pulseAnimation = new PulseAnimation
        {
            Location = new Point(20, 500),
            Size = new Size(240, 40)
        };
        
        _sidePanel.Controls.AddRange(new Control[] {
            controlLabel, _startBtn, _stopBtn, _settingsBtn,
            badWordsLabel, _badWordsInput, _addWordsBtn,
            audioLabel, _audioLevel, _pulseAnimation
        });
        
        Controls.Add(_sidePanel);
    }
    
    private Label CreateSectionLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = _textSecondary,
            Location = new Point(x, y),
            AutoSize = true
        };
    }
    
    private void CreateMainContent()
    {
        _mainContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _background,
            Padding = new Padding(20)
        };
        
        // Activity log card
        var logCard = CreateCard("Activity Log", 20, 20, 400, 300);
        
        _logBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = _textPrimary,
            ReadOnly = true
        };
        
        logCard.Controls.Add(_logBox);
        
        // Detected phrases card
        var detectedCard = CreateCard("Detected Phrases", 440, 20, 380, 300);
        
        _detectedList = new ModernListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = _textPrimary
        };
        
        detectedCard.Controls.Add(_detectedList);
        
        _mainContentPanel.Controls.AddRange(new Control[] { logCard, detectedCard });
        Controls.Add(_mainContentPanel);
    }
    
    private Panel CreateCard(string title, int x, int y, int width, int height)
    {
        var card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = _surface
        };
        
        // Card shadow
        card.Paint += (s, e) =>
        {
            var rect = new Rectangle(0, 0, card.Width, card.Height);
            using (var path = GetRoundedRectPath(rect, 8))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.TranslateTransform(2, 2);
                    e.Graphics.FillPath(shadowBrush, path);
                    e.Graphics.TranslateTransform(-2, -2);
                }
                
                // Card background
                using (var brush = new SolidBrush(_surface))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
        };
        
        // Title label
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = _textPrimary,
            Location = new Point(15, 15),
            AutoSize = true
        };
        
        card.Controls.Add(titleLabel);
        
        // Content panel
        var contentPanel = new Panel
        {
            Location = new Point(15, 45),
            Size = new Size(width - 30, height - 60),
            BackColor = Color.Transparent
        };
        
        card.Controls.Add(contentPanel);
        
        return contentPanel;
    }
    
    private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
    
    private void CreateStatusBar()
    {
        _statusPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(10, 5, 10, 5)
        };
        
        _statusLabel = new Label
        {
            Text = "● Ready",
            ForeColor = _textSecondary,
            AutoSize = true,
            Location = new Point(10, 6),
            Font = new Font("Segoe UI", 9F)
        };
        
        var versionLabel = new Label
        {
            Text = "v1.0.0",
            ForeColor = _textSecondary,
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Location = new Point(Width - 60, 6),
            Font = new Font("Segoe UI", 8F)
        };
        
        _statusPanel.Controls.AddRange(new Control[] { _statusLabel, versionLabel });
        Controls.Add(_statusPanel);
    }
    
    private void SetupAnimations()
    {
        _animationTimer = new Timer { Interval = 50 };
        _animationTimer.Tick += (s, e) =>
        {
            if (_isMonitoring)
            {
                _pulseAnimation?.Pulse();
                _audioLevel?.UpdateAnimation();
            }
        };
        _animationTimer.Start();
        
        _updateTimer = new Timer { Interval = 1000 };
        _updateTimer.Tick += (s, e) => UpdateDetectedList();
    }
    
    private void InitializeMonitor()
    {
        _monitor = new EnhancedAudioMonitor();
        
        _monitor.OnStatusChanged += (s, status) =>
        {
            Invoke(() =>
            {
                LogMessage($"[STATUS] {status}", Color.Blue);
                UpdateStatus(status);
            });
        };
        
        _monitor.OnSpeechDetected += (s, e) =>
        {
            Invoke(() =>
            {
                LogMessage($"[SPEECH] {e.Text} (Confidence: {e.Confidence:F2})", Color.Green);
            });
        };
        
        _monitor.OnBadWordDetected += (s, e) =>
        {
            Invoke(() =>
            {
                _detectionCount++;
                UpdateDetectionCount();
                
                LogMessage($"[ALERT] Bad words detected: {string.Join(", ", e.DetectionResult.DetectedWords)}", Color.Red);
                LogMessage($"  Text: {e.TranscribedText}", Color.OrangeRed);
                
                _detectedPhrases.Add($"[{DateTime.Now:HH:mm:ss}] {e.TranscribedText}");
                
                // Show modern notification
                ShowNotification("Inappropriate Content Detected", 
                    $"Words: {string.Join(", ", e.DetectionResult.DetectedWords)}",
                    NotificationType.Warning);
            });
        };
        
        _monitor.OnAudioLevelChanged += (s, e) =>
        {
            Invoke(() =>
            {
                _audioLevel?.SetLevel((float)e.Level);
            });
        };
        
        // Add default bad words
        _monitor.SetCustomBadWords(new[]
        {
            "damn", "hell", "stupid", "idiot", "shut up", "hate",
            "đồ ngu", "im đi", "chết tiệt", "khốn nạn"
        });
    }
    
    private async Task StartMonitoring()
    {
        try
        {
            _isMonitoring = true;
            _startBtn.Enabled = false;
            _stopBtn.Enabled = true;
            
            LogMessage("Starting audio monitoring...", Color.Blue);
            await _monitor.StartMonitoringAsync();
            
            _updateTimer.Start();
            _pulseAnimation?.Start();
            
            LogMessage("Audio monitoring started successfully!", Color.Green);
            UpdateStatus("Monitoring Active");
        }
        catch (Exception ex)
        {
            LogMessage($"Error: {ex.Message}", Color.Red);
            _isMonitoring = false;
            _startBtn.Enabled = true;
            _stopBtn.Enabled = false;
        }
    }
    
    private void StopMonitoring()
    {
        _isMonitoring = false;
        _monitor.Stop();
        _updateTimer.Stop();
        _pulseAnimation?.Stop();
        
        _startBtn.Enabled = true;
        _stopBtn.Enabled = false;
        
        LogMessage("Audio monitoring stopped", Color.Orange);
        UpdateStatus("Ready");
    }
    
    private void AddBadWords()
    {
        var words = _badWordsInput.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            var trimmedWords = Array.ConvertAll(words, w => w.Trim());
            _monitor.SetCustomBadWords(trimmedWords);
            LogMessage($"Added {trimmedWords.Length} bad words: {string.Join(", ", trimmedWords)}", Color.Purple);
            _badWordsInput.Clear();
            
            ShowNotification("Words Added", $"Added {trimmedWords.Length} words to filter", NotificationType.Success);
        }
    }
    
    private void ShowSettings()
    {
        ShowNotification("Settings", "Settings panel coming soon!", NotificationType.Info);
    }
    
    private void UpdateStatus(string status)
    {
        _statusLabel.Text = _isMonitoring ? $"● {status}" : $"○ {status}";
        _statusLabel.ForeColor = _isMonitoring ? _success : _textSecondary;
    }
    
    private void UpdateDetectionCount()
    {
        _detectionCountLabel.Text = _detectionCount.ToString();
        
        // Animate the count change
        var originalSize = _detectionCountLabel.Font.Size;
        _detectionCountLabel.Font = new Font(_detectionCountLabel.Font.FontFamily, originalSize + 4);
        
        var timer = new Timer { Interval = 200 };
        timer.Tick += (s, e) =>
        {
            _detectionCountLabel.Font = new Font(_detectionCountLabel.Font.FontFamily, originalSize);
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }
    
    private void UpdateDetectedList()
    {
        var detected = _monitor.GetDetectedPhrases();
        if (detected.Count != _detectedList.Items.Count)
        {
            _detectedList.Items.Clear();
            foreach (var phrase in detected)
            {
                _detectedList.Items.Add(phrase);
            }
            
            if (_detectedList.Items.Count > 0)
            {
                _detectedList.SelectedIndex = _detectedList.Items.Count - 1;
            }
        }
    }
    
    private void LogMessage(string message, Color color)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        
        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.SelectionLength = 0;
        _logBox.SelectionColor = _textSecondary;
        _logBox.AppendText($"[{timestamp}] ");
        
        _logBox.SelectionColor = color;
        _logBox.AppendText($"{message}\n");
        
        _logBox.SelectionStart = _logBox.Text.Length;
        _logBox.ScrollToCaret();
    }
    
    private void ShowNotification(string title, string message, NotificationType type)
    {
        var notification = new NotificationPopup(title, message, type)
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(Right - 320, Bottom - 120)
        };
        notification.Show();
    }
    
    private void Invoke(Action action)
    {
        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _monitor?.Dispose();
        _animationTimer?.Dispose();
        _updateTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

// Custom Controls
public class ModernButton : Button
{
    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        
        MouseEnter += (s, e) =>
        {
            BackColor = ControlPaint.Light(BackColor, 0.1f);
        };
        
        MouseLeave += (s, e) =>
        {
            BackColor = ControlPaint.Dark(BackColor, 0.1f);
        };
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        using (var path = GetRoundedRectPath(ClientRectangle, 4))
        using (var brush = new SolidBrush(BackColor))
        {
            e.Graphics.FillPath(brush, path);
        }
        
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
    
    private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2 - 1, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2 - 1, rect.Bottom - radius * 2 - 1, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2 - 1, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class ModernTextBox : TextBox
{
    private string _placeholderText = "Enter text...";
    private Color _placeholderColor = Color.FromArgb(150, 150, 150);
    
    public string PlaceholderText
    {
        get => _placeholderText;
        set
        {
            _placeholderText = value;
            if (string.IsNullOrEmpty(Text))
            {
                Text = _placeholderText;
                ForeColor = _placeholderColor;
            }
        }
    }
    
    public ModernTextBox()
    {
        BorderStyle = BorderStyle.None;
        Font = new Font("Segoe UI", 10F);
        
        Enter += (s, e) =>
        {
            if (Text == _placeholderText)
            {
                Text = "";
                ForeColor = Color.Black;
            }
        };
        
        Leave += (s, e) =>
        {
            if (string.IsNullOrEmpty(Text))
            {
                Text = _placeholderText;
                ForeColor = _placeholderColor;
            }
        };
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
        {
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}

public class ModernListBox : ListBox
{
    public ModernListBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 30;
        BorderStyle = BorderStyle.None;
    }
    
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        
        e.DrawBackground();
        
        var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var backgroundColor = isSelected ? Color.FromArgb(33, 150, 243) : 
                              (e.Index % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 248));
        var textColor = isSelected ? Color.White : Color.FromArgb(60, 60, 60);
        
        using (var brush = new SolidBrush(backgroundColor))
        {
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
        
        if (Items[e.Index] != null)
        {
            using (var brush = new SolidBrush(textColor))
            {
                var text = Items[e.Index].ToString();
                var textRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 5, 
                                            e.Bounds.Width - 20, e.Bounds.Height - 10);
                e.Graphics.DrawString(text, Font, brush, textRect);
            }
        }
        
        e.DrawFocusRectangle();
    }
}

public class AudioLevelIndicator : Control
{
    private float _level = 0;
    private float _targetLevel = 0;
    private float _currentLevel = 0;
    private readonly List<float> _history = new List<float>();
    private readonly int _maxHistory = 50;
    
    public void SetLevel(float level)
    {
        _targetLevel = Math.Min(1f, Math.Max(0f, level));
        
        _history.Add(_targetLevel);
        if (_history.Count > _maxHistory)
            _history.RemoveAt(0);
    }
    
    public void UpdateAnimation()
    {
        _currentLevel += (_targetLevel - _currentLevel) * 0.3f;
        Invalidate();
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        // Background
        using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
        {
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
        
        // Draw waveform history
        if (_history.Count > 1)
        {
            var points = new List<PointF>();
            var step = (float)Width / _maxHistory;
            
            for (int i = 0; i < _history.Count; i++)
            {
                var x = i * step;
                var y = Height - (_history[i] * Height * 0.8f) - 10;
                points.Add(new PointF(x, y));
            }
            
            using (var pen = new Pen(Color.FromArgb(100, 33, 150, 243), 2))
            {
                if (points.Count > 1)
                    e.Graphics.DrawLines(pen, points.ToArray());
            }
        }
        
        // Current level bar
        var barHeight = (int)(_currentLevel * Height * 0.8f);
        var barRect = new Rectangle(Width - 40, Height - barHeight - 10, 30, barHeight);
        
        // Gradient fill for bar
        if (barHeight > 0)
        {
            using (var brush = new LinearGradientBrush(barRect,
                GetLevelColor(_currentLevel),
                ControlPaint.Dark(GetLevelColor(_currentLevel), 0.2f),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, barRect);
            }
        }
        
        // Level percentage
        var levelText = $"{(_currentLevel * 100):F0}%";
        using (var brush = new SolidBrush(Color.FromArgb(100, 100, 100)))
        {
            e.Graphics.DrawString(levelText, new Font("Segoe UI", 8F), brush, 
                new PointF(Width - 35, 5));
        }
    }
    
    private Color GetLevelColor(float level)
    {
        if (level < 0.3f) return Color.FromArgb(76, 175, 80);  // Green
        if (level < 0.7f) return Color.FromArgb(255, 152, 0);  // Orange
        return Color.FromArgb(244, 67, 54);  // Red
    }
}

public class PulseAnimation : Control
{
    private float _pulseSize = 0;
    private bool _isActive = false;
    
    public void Start() => _isActive = true;
    public void Stop() => _isActive = false;
    
    public void Pulse()
    {
        if (_isActive)
        {
            _pulseSize = (_pulseSize + 0.05f) % 1f;
            Invalidate();
        }
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        if (_isActive)
        {
            var centerX = Width / 2;
            var centerY = Height / 2;
            var maxRadius = Math.Min(Width, Height) / 3;
            
            for (int i = 0; i < 3; i++)
            {
                var offset = i * 0.3f;
                var size = (_pulseSize + offset) % 1f;
                var radius = size * maxRadius;
                var opacity = (int)(255 * (1 - size));
                
                using (var brush = new SolidBrush(Color.FromArgb(opacity, 33, 150, 243)))
                {
                    e.Graphics.FillEllipse(brush, 
                        centerX - radius, centerY - radius, 
                        radius * 2, radius * 2);
                }
            }
        }
        
        // Status text
        var status = _isActive ? "Monitoring" : "Idle";
        using (var brush = new SolidBrush(Color.FromArgb(60, 60, 60)))
        {
            var textSize = e.Graphics.MeasureString(status, Font);
            e.Graphics.DrawString(status, Font, brush, 
                (Width - textSize.Width) / 2, 
                (Height - textSize.Height) / 2);
        }
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class NotificationPopup : Form
{
    private Timer _autoCloseTimer;
    
    public NotificationPopup(string title, string message, NotificationType type)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        Size = new Size(300, 100);
        StartPosition = FormStartPosition.Manual;
        
        var color = type switch
        {
            NotificationType.Success => Color.FromArgb(76, 175, 80),
            NotificationType.Warning => Color.FromArgb(255, 152, 0),
            NotificationType.Error => Color.FromArgb(244, 67, 54),
            _ => Color.FromArgb(33, 150, 243)
        };
        
        BackColor = color;
        
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 15),
            AutoSize = true
        };
        
        var messageLabel = new Label
        {
            Text = message,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.White,
            Location = new Point(15, 40),
            Size = new Size(270, 40),
            AutoEllipsis = true
        };
        
        Controls.AddRange(new Control[] { titleLabel, messageLabel });
        
        // Auto close after 3 seconds
        _autoCloseTimer = new Timer { Interval = 3000 };
        _autoCloseTimer.Tick += (s, e) =>
        {
            _autoCloseTimer.Stop();
            Close();
        };
        _autoCloseTimer.Start();
        
        // Fade in effect
        Opacity = 0;
        var fadeTimer = new Timer { Interval = 50 };
        fadeTimer.Tick += (s, e) =>
        {
            Opacity += 0.1;
            if (Opacity >= 1)
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
            }
        };
        fadeTimer.Start();
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // Border
        using (var pen = new Pen(Color.FromArgb(50, 0, 0, 0), 1))
        {
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
