using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Detection;
using ChildGuard.Core.Audio;
using ChildGuard.Hooking;

namespace ChildGuard.UI;

public class TestProgram : Form
{
    private readonly AdvancedProtectionManager _protectionManager;
    private readonly BadWordsDetector _badWordsDetector;
    private readonly UrlSafetyChecker _urlChecker;
    private RichTextBox _logBox;
    private TextBox _testInput;
    private Button _testBadWordsBtn;
    private Button _testUrlBtn;
    private Button _startProtectionBtn;
    private Button _stopProtectionBtn;
    private Label _statusLabel;
    private Label _statsLabel;
    private AppConfig _config;
    private int _threatCount = 0;
    private int _keyCount = 0;
    private int _mouseCount = 0;
    
    public TestProgram()
    {
        InitializeComponent();
        
        _protectionManager = new AdvancedProtectionManager();
        _badWordsDetector = new BadWordsDetector();
        _urlChecker = new UrlSafetyChecker();
        _config = new AppConfig
        {
            EnableInputMonitoring = true,
            EnableAudioMonitoring = false,
            BlockScreenshots = true,
            CheckUrls = true,
            BlockInappropriateContent = true
        };
        
        SetupEventHandlers();
        Log("🚀 ChildGuard Test Program Started", Color.Green);
        Log("📋 Ready to test protection features", Color.Blue);
    }
    
    private void InitializeComponent()
    {
        this.Text = "ChildGuard Protection Test Suite";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        
        // Main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        
        // Left panel - Controls
        var controlPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        
        // Status
        _statusLabel = new Label
        {
            Text = "⚪ Protection: STOPPED",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(10, 10),
            Size = new Size(300, 30)
        };
        controlPanel.Controls.Add(_statusLabel);
        
        // Statistics
        _statsLabel = new Label
        {
            Text = "Keys: 0 | Mouse: 0 | Threats: 0",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 45),
            Size = new Size(300, 25)
        };
        controlPanel.Controls.Add(_statsLabel);
        
        // Protection controls
        _startProtectionBtn = new Button
        {
            Text = "▶ Start Protection",
            Location = new Point(10, 80),
            Size = new Size(150, 35),
            BackColor = Color.Green,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _startProtectionBtn.Click += StartProtection;
        controlPanel.Controls.Add(_startProtectionBtn);
        
        _stopProtectionBtn = new Button
        {
            Text = "⏹ Stop Protection",
            Location = new Point(170, 80),
            Size = new Size(150, 35),
            BackColor = Color.Red,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _stopProtectionBtn.Click += StopProtection;
        controlPanel.Controls.Add(_stopProtectionBtn);
        
        // Test input
        var inputLabel = new Label
        {
            Text = "Test Input:",
            Location = new Point(10, 130),
            Size = new Size(100, 20)
        };
        controlPanel.Controls.Add(inputLabel);
        
        _testInput = new TextBox
        {
            Location = new Point(10, 155),
            Size = new Size(310, 25),
            Font = new Font("Segoe UI", 10)
        };
        controlPanel.Controls.Add(_testInput);
        
        // Test buttons
        _testBadWordsBtn = new Button
        {
            Text = "🔍 Test Bad Words",
            Location = new Point(10, 190),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        _testBadWordsBtn.Click += TestBadWords;
        controlPanel.Controls.Add(_testBadWordsBtn);
        
        _testUrlBtn = new Button
        {
            Text = "🌐 Test URL Check",
            Location = new Point(170, 190),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        _testUrlBtn.Click += TestUrlCheck;
        controlPanel.Controls.Add(_testUrlBtn);
        
        // Predefined tests
        var predefLabel = new Label
        {
            Text = "Quick Tests:",
            Location = new Point(10, 240),
            Size = new Size(100, 20),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        controlPanel.Controls.Add(predefLabel);
        
        var test1Btn = new Button
        {
            Text = "Test: violence drugs",
            Location = new Point(10, 265),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat
        };
        test1Btn.Click += (s, e) => {
            _testInput.Text = "This contains violence and drugs";
            TestBadWords(s, e);
        };
        controlPanel.Controls.Add(test1Btn);
        
        var test2Btn = new Button
        {
            Text = "Test: phishing URL",
            Location = new Point(10, 300),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat
        };
        test2Btn.Click += (s, e) => {
            _testInput.Text = "http://phishing-site.fake";
            TestUrlCheck(s, e);
        };
        controlPanel.Controls.Add(test2Btn);
        
        var test3Btn = new Button
        {
            Text = "Test: Vietnamese bad words",
            Location = new Point(10, 335),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat
        };
        test3Btn.Click += (s, e) => {
            _testInput.Text = "Nội dung có bạo lực và ma túy";
            TestBadWords(s, e);
        };
        controlPanel.Controls.Add(test3Btn);
        
        // Audio test
        var audioBtn = new Button
        {
            Text = "🎤 Test Audio Monitor",
            Location = new Point(10, 370),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat
        };
        audioBtn.Click += TestAudioMonitor;
        controlPanel.Controls.Add(audioBtn);
        
        // Clear log button
        var clearBtn = new Button
        {
            Text = "Clear Log",
            Location = new Point(10, 410),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat
        };
        clearBtn.Click += (s, e) => _logBox.Clear();
        controlPanel.Controls.Add(clearBtn);
        
        // Right panel - Log
        _logBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 9),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.LightGray,
            BorderStyle = BorderStyle.None
        };
        
        mainPanel.Controls.Add(controlPanel, 0, 0);
        mainPanel.Controls.Add(_logBox, 1, 0);
        
        this.Controls.Add(mainPanel);
    }
    
    private void SetupEventHandlers()
    {
        // Protection Manager Events
        _protectionManager.OnThreatDetected += (sender, e) =>
        {
            _threatCount++;
            BeginInvoke(new Action(() =>
            {
                var color = e.Level switch
                {
                    ThreatLevel.Critical => Color.Magenta,
                    ThreatLevel.High => Color.Red,
                    ThreatLevel.Medium => Color.Orange,
                    ThreatLevel.Low => Color.Yellow,
                    _ => Color.Gray
                };
                
                Log($"⚠️ THREAT DETECTED [{e.Level}]", color);
                Log($"   Type: {e.Type}", color);
                Log($"   Description: {e.Description}", color);
                if (!string.IsNullOrEmpty(e.Content))
                    Log($"   Content: {e.Content}", color);
                Log($"   Source: {e.Source}", color);
                
                UpdateStats();
            }));
        };
        
        _protectionManager.OnStatisticsUpdated += (sender, e) =>
        {
            _keyCount = e.TotalKeysPressed;
            _mouseCount = e.TotalMouseClicks;
            BeginInvoke(new Action(() =>
            {
                UpdateStats();
                Log($"📊 Stats Update - Keys: {e.TotalKeysPressed}, Mouse: {e.TotalMouseClicks}, Threats: {e.ThreatsDetected}", Color.Gray);
            }));
        };
        
        _protectionManager.OnActivity += (sender, e) =>
        {
            BeginInvoke(new Action(() =>
            {
                if (e.Data is ActivityType actType)
                {
                    if (actType != ActivityType.KeyPress && actType != ActivityType.MouseClick)
                    {
                        Log($"📌 Activity: {e.Data}", Color.LightBlue);
                    }
                }
            }));
        };
    }
    
    private void StartProtection(object sender, EventArgs e)
    {
        try
        {
            Log("🔄 Starting protection manager...", Color.Yellow);
            _protectionManager.Start(_config);
            
            _startProtectionBtn.Enabled = false;
            _stopProtectionBtn.Enabled = true;
            _statusLabel.Text = "🟢 Protection: ACTIVE";
            _statusLabel.ForeColor = Color.Green;
            
            Log("✅ Protection started successfully!", Color.Green);
            Log("   - Input Monitoring: " + (_config.EnableInputMonitoring ? "ON" : "OFF"), Color.Green);
            Log("   - URL Checking: " + (_config.CheckUrls ? "ON" : "OFF"), Color.Green);
            Log("   - Content Filter: " + (_config.BlockInappropriateContent ? "ON" : "OFF"), Color.Green);
            Log("   - Audio Monitor: " + (_config.EnableAudioMonitoring ? "ON" : "OFF"), Color.Green);
            Log("   - Screenshot Block: " + (_config.BlockScreenshots ? "ON" : "OFF"), Color.Green);
            
            Log("💡 Try typing or clicking around to test detection!", Color.Cyan);
        }
        catch (Exception ex)
        {
            Log($"❌ Failed to start protection: {ex.Message}", Color.Red);
        }
    }
    
    private void StopProtection(object sender, EventArgs e)
    {
        try
        {
            Log("🔄 Stopping protection manager...", Color.Yellow);
            _protectionManager.Stop();
            
            _startProtectionBtn.Enabled = true;
            _stopProtectionBtn.Enabled = false;
            _statusLabel.Text = "⚪ Protection: STOPPED";
            _statusLabel.ForeColor = Color.Gray;
            
            Log("⏹ Protection stopped", Color.Orange);
        }
        catch (Exception ex)
        {
            Log($"❌ Error stopping protection: {ex.Message}", Color.Red);
        }
    }
    
    private void TestBadWords(object sender, EventArgs e)
    {
        var text = _testInput.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            Log("❌ Please enter text to test", Color.Red);
            return;
        }
        
        Log($"🔍 Testing bad words detection for: \"{text}\"", Color.Cyan);
        
        try
        {
            var result = _badWordsDetector.Analyze(text);
            
            if (result.IsClean)
            {
                Log("✅ Content is CLEAN - No bad words detected", Color.Green);
            }
            else
            {
                Log($"⚠️ BAD CONTENT DETECTED!", Color.Red);
                Log($"   Found words: {string.Join(", ", result.FoundWords)}", Color.Red);
                Log($"   Severity: {result.Severity}", Color.Orange);
                Log($"   Summary: {result.GetSummary()}", Color.Orange);
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Error in bad words detection: {ex.Message}", Color.Red);
        }
    }
    
    private async void TestUrlCheck(object sender, EventArgs e)
    {
        var url = _testInput.Text;
        if (string.IsNullOrWhiteSpace(url))
        {
            Log("❌ Please enter a URL to test", Color.Red);
            return;
        }
        
        Log($"🌐 Testing URL safety for: \"{url}\"", Color.Cyan);
        
        try
        {
            var result = await _urlChecker.CheckUrlAsync(url);
            
            if (result.IsSafe)
            {
                Log("✅ URL is SAFE", Color.Green);
            }
            else
            {
                Log($"⚠️ UNSAFE URL DETECTED!", Color.Red);
                Log($"   Reason: {result.Reason}", Color.Red);
                if (result.Categories?.Count > 0)
                    Log($"   Categories: {string.Join(", ", result.Categories)}", Color.Orange);
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Error in URL checking: {ex.Message}", Color.Red);
        }
    }
    
    private void TestAudioMonitor(object sender, EventArgs e)
    {
        Log("🎤 Testing Audio Monitor...", Color.Cyan);
        
        var audioMonitor = new AudioMonitor();
        
        audioMonitor.OnDetection += (s, args) =>
        {
            BeginInvoke(new Action(() =>
            {
                Log($"🎤 Audio Detection Event:", Color.Yellow);
                Log($"   Text: {args.TranscribedText}", Color.Yellow);
                Log($"   Clean: {args.DetectionResult.IsClean}", Color.Yellow);
            }));
        };
        
        Log("📢 Audio monitor configured (FFmpeg required for actual capture)", Color.Blue);
        Log("   Note: Audio capture requires FFmpeg installed", Color.Gray);
        
        // Simulate audio detection
        Task.Delay(1000).ContinueWith(_ =>
        {
            BeginInvoke(new Action(() =>
            {
                Log("🔊 Simulating audio detection...", Color.Cyan);
                Log("   Would capture 30-second audio chunks", Color.Gray);
                Log("   Would transcribe using speech-to-text", Color.Gray);
                Log("   Would analyze for inappropriate content", Color.Gray);
            }));
        });
    }
    
    private void UpdateStats()
    {
        _statsLabel.Text = $"Keys: {_keyCount} | Mouse: {_mouseCount} | Threats: {_threatCount}";
    }
    
    private void Log(string message, Color color)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Log(message, color)));
            return;
        }
        
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logBox.SelectionColor = Color.DarkGray;
        _logBox.AppendText($"[{timestamp}] ");
        _logBox.SelectionColor = color;
        _logBox.AppendText(message + Environment.NewLine);
        _logBox.ScrollToCaret();
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _protectionManager?.Dispose();
        base.OnFormClosed(e);
    }
    
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TestProgram());
    }
}

// Extension class for DetectionResult
public static class DetectionResultExtensions
{
    public static string GetSummary(this DetectionResult result)
    {
        if (result.IsClean)
            return "Content is safe";
        
        var totalIssues = result.FoundWords.Count + 
                         result.DetectedPhrases.Count + 
                         result.DetectedPatterns.Count;
        
        return $"Found {totalIssues} inappropriate items - Severity: {result.Severity}";
    }
}
