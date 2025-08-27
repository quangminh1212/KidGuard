using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using ChildGuard.Core.Audio;
using ChildGuard.Core.Detection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestAudioMonitor;

public class AudioMonitorTestForm : Form
{
    private EnhancedAudioMonitor _monitor;
    private TextBox _logBox;
    private TextBox _badWordsInput;
    private Button _startBtn;
    private Button _stopBtn;
    private Button _addWordsBtn;
    private Button _testFFmpegBtn;
    private Label _statusLabel;
    private Label _audioLevelLabel;
    private ProgressBar _audioLevelBar;
    private ListBox _detectedList;
    private CheckBox _useSpeechRecognition;
    private CheckBox _useFFmpeg;
    private Timer _updateTimer;
    
    public AudioMonitorTestForm()
    {
        InitializeUI();
        InitializeMonitor();
    }
    
    private void InitializeUI()
    {
        Text = "Audio Monitor Test - ChildGuard";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterScreen;
        
        // Main panel
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        // Control panel
        var controlPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 200
        };
        
        // Status label
        _statusLabel = new Label
        {
            Text = "Status: Ready",
            Location = new Point(10, 10),
            Size = new Size(500, 20),
            ForeColor = Color.Blue
        };
        
        // Audio level
        _audioLevelLabel = new Label
        {
            Text = "Audio Level: 0%",
            Location = new Point(10, 35),
            Size = new Size(150, 20)
        };
        
        _audioLevelBar = new ProgressBar
        {
            Location = new Point(170, 35),
            Size = new Size(300, 20),
            Maximum = 100
        };
        
        // Checkboxes
        _useSpeechRecognition = new CheckBox
        {
            Text = "Use Windows Speech Recognition",
            Location = new Point(10, 65),
            Size = new Size(250, 25),
            Checked = true
        };
        
        _useFFmpeg = new CheckBox
        {
            Text = "Use FFmpeg Capture",
            Location = new Point(270, 65),
            Size = new Size(200, 25),
            Checked = false
        };
        
        // Buttons
        _startBtn = new Button
        {
            Text = "Start Monitoring",
            Location = new Point(10, 95),
            Size = new Size(120, 30),
            BackColor = Color.LightGreen
        };
        _startBtn.Click += async (s, e) => await StartMonitoring();
        
        _stopBtn = new Button
        {
            Text = "Stop Monitoring",
            Location = new Point(140, 95),
            Size = new Size(120, 30),
            BackColor = Color.LightCoral,
            Enabled = false
        };
        _stopBtn.Click += (s, e) => StopMonitoring();
        
        _testFFmpegBtn = new Button
        {
            Text = "Test FFmpeg",
            Location = new Point(270, 95),
            Size = new Size(120, 30),
            BackColor = Color.LightBlue
        };
        _testFFmpegBtn.Click += (s, e) => TestFFmpeg();
        
        // Bad words input
        var badWordsLabel = new Label
        {
            Text = "Add bad words (comma separated):",
            Location = new Point(10, 135),
            Size = new Size(250, 20)
        };
        
        _badWordsInput = new TextBox
        {
            Location = new Point(10, 155),
            Size = new Size(350, 25),
            Text = "damn, hell, stupid, idiot, shut up"
        };
        
        _addWordsBtn = new Button
        {
            Text = "Add Words",
            Location = new Point(370, 153),
            Size = new Size(100, 27)
        };
        _addWordsBtn.Click += (s, e) => AddBadWords();
        
        controlPanel.Controls.AddRange(new Control[] {
            _statusLabel, _audioLevelLabel, _audioLevelBar,
            _useSpeechRecognition, _useFFmpeg,
            _startBtn, _stopBtn, _testFFmpegBtn,
            badWordsLabel, _badWordsInput, _addWordsBtn
        });
        
        // Log panel
        var logPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };
        
        var logLabel = new Label
        {
            Text = "Activity Log:",
            Dock = DockStyle.Top,
            Height = 20
        };
        
        _logBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9),
            ReadOnly = true
        };
        
        // Detected phrases panel
        var detectedPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 300,
            Padding = new Padding(10, 0, 0, 0)
        };
        
        var detectedLabel = new Label
        {
            Text = "Detected Bad Words:",
            Dock = DockStyle.Top,
            Height = 20
        };
        
        _detectedList = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9)
        };
        
        detectedPanel.Controls.Add(_detectedList);
        detectedPanel.Controls.Add(detectedLabel);
        
        logPanel.Controls.Add(_logBox);
        logPanel.Controls.Add(logLabel);
        
        mainPanel.Controls.Add(logPanel);
        mainPanel.Controls.Add(detectedPanel);
        mainPanel.Controls.Add(controlPanel);
        
        Controls.Add(mainPanel);
        
        // Update timer
        _updateTimer = new Timer { Interval = 1000 };
        _updateTimer.Tick += (s, e) => UpdateDetectedList();
    }
    
    private void InitializeMonitor()
    {
        _monitor = new EnhancedAudioMonitor();
        
        // Subscribe to events
        _monitor.OnStatusChanged += (s, status) =>
        {
            Invoke(() =>
            {
                LogMessage($"[STATUS] {status}");
                _statusLabel.Text = $"Status: {status}";
            });
        };
        
        _monitor.OnSpeechDetected += (s, e) =>
        {
            Invoke(() =>
            {
                LogMessage($"[SPEECH] {e.Text} (Confidence: {e.Confidence:F2})");
            });
        };
        
        _monitor.OnBadWordDetected += (s, e) =>
        {
            Invoke(() =>
            {
                LogMessage($"[ALERT] Bad words detected: {string.Join(", ", e.DetectionResult.DetectedWords)}");
                LogMessage($"  Text: {e.TranscribedText}");
                LogMessage($"  Timestamp: {e.Timestamp:HH:mm:ss}");
                
                // Show alert
                MessageBox.Show(
                    $"Inappropriate content detected!\n\n" +
                    $"Text: {e.TranscribedText}\n" +
                    $"Words: {string.Join(", ", e.DetectionResult.DetectedWords)}\n" +
                    $"Time: {e.Timestamp:HH:mm:ss}",
                    "ChildGuard Alert",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            });
        };
        
        _monitor.OnAudioLevelChanged += (s, e) =>
        {
            Invoke(() =>
            {
                int level = (int)(e.Level * 100);
                _audioLevelBar.Value = Math.Min(level, 100);
                _audioLevelLabel.Text = $"Audio Level: {level}%";
            });
        };
        
        // Add default bad words
        AddDefaultBadWords();
    }
    
    private void AddDefaultBadWords()
    {
        var defaultWords = new[]
        {
            // English bad words
            "damn", "hell", "stupid", "idiot", "shut up", "hate",
            
            // Vietnamese bad words (for testing)
            "đồ ngu", "im đi", "chết tiệt", "khốn nạn"
        };
        
        _monitor.SetCustomBadWords(defaultWords);
        LogMessage($"Added {defaultWords.Length} default bad words");
    }
    
    private async Task StartMonitoring()
    {
        try
        {
            _startBtn.Enabled = false;
            _stopBtn.Enabled = true;
            
            _monitor.UseWindowsSpeechRecognition = _useSpeechRecognition.Checked;
            _monitor.UseFFmpegCapture = _useFFmpeg.Checked;
            
            LogMessage("Starting audio monitoring...");
            await _monitor.StartMonitoringAsync();
            
            _updateTimer.Start();
            LogMessage("Audio monitoring started successfully!");
        }
        catch (Exception ex)
        {
            LogMessage($"Error starting monitor: {ex.Message}");
            _startBtn.Enabled = true;
            _stopBtn.Enabled = false;
        }
    }
    
    private void StopMonitoring()
    {
        _monitor.Stop();
        _updateTimer.Stop();
        
        _startBtn.Enabled = true;
        _stopBtn.Enabled = false;
        
        LogMessage("Audio monitoring stopped");
    }
    
    private void AddBadWords()
    {
        var words = _badWordsInput.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            var trimmedWords = Array.ConvertAll(words, w => w.Trim());
            _monitor.SetCustomBadWords(trimmedWords);
            LogMessage($"Added {trimmedWords.Length} custom bad words: {string.Join(", ", trimmedWords)}");
            _badWordsInput.Clear();
        }
    }
    
    private void TestFFmpeg()
    {
        LogMessage("Testing FFmpeg...");
        
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            if (output.Contains("ffmpeg version"))
            {
                LogMessage("FFmpeg found and working!");
                LogMessage($"Version: {output.Substring(0, Math.Min(100, output.Length))}...");
            }
            else
            {
                LogMessage("FFmpeg not found or not working properly");
            }
        }
        catch (Exception ex)
        {
            LogMessage($"FFmpeg test failed: {ex.Message}");
            LogMessage("Please install FFmpeg and add it to PATH");
        }
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
    
    private void LogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logBox.AppendText($"[{timestamp}] {message}\r\n");
        _logBox.SelectionStart = _logBox.Text.Length;
        _logBox.ScrollToCaret();
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
        _updateTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        try
        {
            // Use modern UI by default, pass --classic for old UI
            bool useModernUI = !args.Contains("--classic");
            
            if (useModernUI)
            {
                // Use the modern Material Design UI
                Application.Run(new ModernAudioMonitorForm());
            }
            else
            {
                // Use the classic UI
                Application.Run(new AudioMonitorTestForm());
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error starting application:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
