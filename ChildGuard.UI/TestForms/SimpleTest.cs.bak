using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Detection;

namespace ChildGuard.UI;

public class SimpleTestProgram : Form
{
    private readonly BadWordsDetector _badWordsDetector;
    private readonly UrlSafetyChecker _urlChecker;
    private RichTextBox _logBox;
    private TextBox _testInput;
    private Button _testBadWordsBtn;
    private Button _testUrlBtn;
    
    public SimpleTestProgram()
    {
        _badWordsDetector = new BadWordsDetector();
        _urlChecker = new UrlSafetyChecker();
        
        InitializeComponent();
        Log("🚀 ChildGuard Protection Test Started", Color.Green);
        Log("📋 Testing bad words detection and URL safety check", Color.Blue);
        Log("", Color.White);
    }
    
    private void InitializeComponent()
    {
        this.Text = "ChildGuard Protection Test";
        this.Size = new Size(900, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        
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
        var controlPanel = new Panel { Dock = DockStyle.Fill };
        
        var title = new Label
        {
            Text = "Protection Feature Tests",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(10, 10),
            Size = new Size(300, 30)
        };
        controlPanel.Controls.Add(title);
        
        var inputLabel = new Label
        {
            Text = "Test Input:",
            Location = new Point(10, 50),
            Size = new Size(100, 20)
        };
        controlPanel.Controls.Add(inputLabel);
        
        _testInput = new TextBox
        {
            Location = new Point(10, 75),
            Size = new Size(310, 25),
            Font = new Font("Segoe UI", 10)
        };
        controlPanel.Controls.Add(_testInput);
        
        _testBadWordsBtn = new Button
        {
            Text = "🔍 Test Bad Words Detection",
            Location = new Point(10, 110),
            Size = new Size(310, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.LightBlue
        };
        _testBadWordsBtn.Click += TestBadWords;
        controlPanel.Controls.Add(_testBadWordsBtn);
        
        _testUrlBtn = new Button
        {
            Text = "🌐 Test URL Safety Check",
            Location = new Point(10, 150),
            Size = new Size(310, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.LightGreen
        };
        _testUrlBtn.Click += TestUrlCheck;
        controlPanel.Controls.Add(_testUrlBtn);
        
        // Quick test buttons
        var quickTestsLabel = new Label
        {
            Text = "Quick Tests:",
            Location = new Point(10, 200),
            Size = new Size(100, 20),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        controlPanel.Controls.Add(quickTestsLabel);
        
        var quickTests = new[]
        {
            ("Test English bad words", "This contains violence and drugs"),
            ("Test Vietnamese", "Nội dung có bạo lực và ma túy"),
            ("Test pattern", "I want to hack and cheat"),
            ("Test safe content", "This is safe content"),
            ("Test phishing URL", "http://phishing-site.fake"),
            ("Test safe URL", "https://www.google.com"),
            ("Test gambling URL", "http://casino-gambling.com")
        };
        
        int yPos = 225;
        foreach (var (label, text) in quickTests)
        {
            var btn = new Button
            {
                Text = label,
                Location = new Point(10, yPos),
                Size = new Size(310, 28),
                FlatStyle = FlatStyle.Flat,
                Tag = text
            };
            btn.Click += (s, e) =>
            {
                var button = (Button)s;
                _testInput.Text = button.Tag.ToString();
                
                // Auto-detect if it's a URL or text
                if (_testInput.Text.StartsWith("http"))
                    TestUrlCheck(s, e);
                else
                    TestBadWords(s, e);
            };
            controlPanel.Controls.Add(btn);
            yPos += 32;
        }
        
        // Clear button
        var clearBtn = new Button
        {
            Text = "Clear Log",
            Location = new Point(10, yPos + 10),
            Size = new Size(310, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.LightGray
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
    
    private void TestBadWords(object sender, EventArgs e)
    {
        var text = _testInput.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            Log("❌ Please enter text to test", Color.Red);
            return;
        }
        
        Log($"🔍 Testing: \"{text}\"", Color.Cyan);
        Log("─────────────────────────────", Color.Gray);
        
        try
        {
            var result = _badWordsDetector.Analyze(text);
            
            if (result.IsClean)
            {
                Log("✅ Result: CLEAN", Color.Green);
                Log("   No inappropriate content detected", Color.Green);
            }
            else
            {
                Log("⚠️ Result: INAPPROPRIATE CONTENT DETECTED", Color.Red);
                
                if (result.FoundWords.Count > 0)
                {
                    Log($"   Bad words found: {string.Join(", ", result.FoundWords)}", Color.Orange);
                }
                
                if (result.DetectedPhrases.Count > 0)
                {
                    Log($"   Bad phrases found: {string.Join(", ", result.DetectedPhrases)}", Color.Orange);
                }
                
                if (result.DetectedPatterns.Count > 0)
                {
                    Log($"   Pattern matches: {string.Join(", ", result.DetectedPatterns)}", Color.Orange);
                }
                
                Log($"   Severity: {result.Severity}", Color.Yellow);
                
                var totalViolations = result.FoundWords.Count + 
                                    result.DetectedPhrases.Count + 
                                    result.DetectedPatterns.Count;
                Log($"   Total violations: {totalViolations}", Color.Yellow);
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Error: {ex.Message}", Color.Red);
        }
        
        Log("", Color.White);
    }
    
    private async void TestUrlCheck(object sender, EventArgs e)
    {
        var url = _testInput.Text;
        if (string.IsNullOrWhiteSpace(url))
        {
            Log("❌ Please enter a URL to test", Color.Red);
            return;
        }
        
        Log($"🌐 Testing URL: \"{url}\"", Color.Cyan);
        Log("─────────────────────────────", Color.Gray);
        
        try
        {
            var result = await _urlChecker.CheckUrlAsync(url);
            
            if (result.IsSafe)
            {
                Log("✅ Result: SAFE URL", Color.Green);
                Log("   This URL appears to be safe", Color.Green);
            }
            else
            {
                Log("⚠️ Result: UNSAFE URL DETECTED", Color.Red);
                Log($"   Reason: {result.Reason ?? "Potentially harmful"}", Color.Orange);
                
                if (result.Categories?.Count > 0)
                {
                    Log($"   Categories: {string.Join(", ", result.Categories)}", Color.Orange);
                }
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Error: {ex.Message}", Color.Red);
        }
        
        Log("", Color.White);
    }
    
    private void Log(string message, Color color)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Log(message, color)));
            return;
        }
        
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        
        if (!string.IsNullOrEmpty(message))
        {
            _logBox.SelectionColor = Color.DarkGray;
            _logBox.AppendText($"[{timestamp}] ");
        }
        
        _logBox.SelectionColor = color;
        _logBox.AppendText(message + Environment.NewLine);
        _logBox.ScrollToCaret();
    }
    
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var testProgram = new SimpleTestProgram();
        
        // Run some automatic tests on startup
        testProgram.Log("🧪 Running automatic tests...", Color.Magenta);
        testProgram.Log("", Color.White);
        
        // Test 1: English bad words
        testProgram._testInput.Text = "violence and drugs";
        testProgram.TestBadWords(null, null);
        
        // Test 2: Vietnamese bad words
        testProgram._testInput.Text = "bạo lực và ma túy";
        testProgram.TestBadWords(null, null);
        
        // Test 3: Safe content
        testProgram._testInput.Text = "This is safe educational content";
        testProgram.TestBadWords(null, null);
        
        // Test 4: URL checking
        Task.Run(async () =>
        {
            await Task.Delay(100);
            testProgram.BeginInvoke(new Action(() =>
            {
                testProgram._testInput.Text = "http://phishing-site.fake";
                testProgram.TestUrlCheck(null, null);
            }));
        });
        
        Application.Run(testProgram);
    }
}
