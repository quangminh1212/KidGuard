using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Detection;
using ChildGuard.Core.Models;
using ChildGuard.Core.Audio;

namespace ChildGuard.Hooking;

/// <summary>
/// Advanced protection manager that combines keyboard/mouse hooks with audio monitoring
/// and real-time threat detection
/// </summary>
public class AdvancedProtectionManager : IDisposable
{
    private readonly BadWordsDetector _badWordsDetector;
    private readonly UrlSafetyChecker _urlChecker;
    private readonly AudioMonitor _audioMonitor;
    private readonly StringBuilder _keyBuffer;
    private readonly List<string> _recentUrls;
    private readonly Queue<string> _recentTexts;
    private readonly System.Threading.Timer _periodicAnalysisTimer;
    
    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;
    private LowLevelKeyboardProc _keyboardProc;
    private LowLevelMouseProc _mouseProc;
    private bool _isRunning;
    private AppConfig _config;
    
    // Statistics
    private int _totalKeysPressed;
    private int _totalMouseClicks;
    private int _threatsDetected;
    private int _urlsChecked;
    
    public event EventHandler<ThreatDetectedEventArgs> OnThreatDetected;
    public event EventHandler<ChildGuard.Core.Models.ActivityEvent> OnActivity;
    public event EventHandler<StatisticsUpdatedEventArgs> OnStatisticsUpdated;
    
    public AdvancedProtectionManager()
    {
        _badWordsDetector = new BadWordsDetector();
        _urlChecker = new UrlSafetyChecker();
        _audioMonitor = new AudioMonitor();
        _keyBuffer = new StringBuilder(1000);
        _recentUrls = new List<string>();
        _recentTexts = new Queue<string>();
        
        // Setup audio monitoring events
        _audioMonitor.OnSpeechDetected += HandleSpeechDetected;
        _audioMonitor.OnLoudNoiseDetected += HandleLoudNoise;
        
        // Periodic analysis every 30 seconds
        _periodicAnalysisTimer = new Timer(PeriodicAnalysis, null, 
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }
    
    public void Start(AppConfig config)
    {
        if (_isRunning) return;
        
        _config = config;
        
        // Start keyboard and mouse hooks
        _keyboardProc = HookKeyboard;
        _mouseProc = HookMouse;
        
        using (var curProcess = Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, 
                GetModuleHandle(curModule.ModuleName), 0);
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, 
                GetModuleHandle(curModule.ModuleName), 0);
        }
        
        // Start audio monitoring if enabled
        if (_config.EnableAudioMonitoring)
        {
            _audioMonitor.Start();
        }
        
        _isRunning = true;
        
        LogActivity("Protection started", ActivityType.SystemEvent);
    }
    
    public void Stop()
    {
        if (!_isRunning) return;
        
        // Stop hooks
        if (_keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }
        
        if (_mouseHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
        
        // Stop audio monitoring
        _audioMonitor.Stop();
        
        _isRunning = false;
        
        LogActivity("Protection stopped", ActivityType.SystemEvent);
    }
    
    private IntPtr HookKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            _totalKeysPressed++;
            
            int vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            
            // Build text buffer for analysis
            if (IsTextKey(key))
            {
                var keyChar = GetCharFromKey(key);
                if (keyChar != '\0')
                {
                    _keyBuffer.Append(keyChar);
                }
                
                // Analyze on word boundaries
                if (key == Keys.Space || key == Keys.Enter || key == Keys.Tab)
                {
                    AnalyzeTypedText();
                }
            }
            
            // Special key combinations
            HandleSpecialKeys(key);
            
            // Buffer overflow protection
            if (_keyBuffer.Length > 500)
            {
                AnalyzeTypedText();
                _keyBuffer.Remove(0, 250); // Keep last half
            }
            
            // Update statistics periodically
            if (_totalKeysPressed % 100 == 0)
            {
                UpdateStatistics();
            }
        }
        
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }
    
    private IntPtr HookMouse(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)
            {
                _totalMouseClicks++;
                
                // Check for URL clicks
                Task.Run(async () =>
                {
                    var url = await GetClickedUrl();
                    if (!string.IsNullOrEmpty(url))
                    {
                        await CheckUrl(url);
                    }
                });
                
                // Update statistics periodically
                if (_totalMouseClicks % 50 == 0)
                {
                    UpdateStatistics();
                }
            }
        }
        
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }
    
    private void HandleSpecialKeys(Keys key)
    {
        var ctrl = (Control.ModifierKeys & Keys.Control) != 0;
        var alt = (Control.ModifierKeys & Keys.Alt) != 0;
        
        // Ctrl+V - Check clipboard
        if (ctrl && key == Keys.V)
        {
            Task.Run(CheckClipboard);
        }
        
        // Alt+Tab - Window switch
        if (alt && key == Keys.Tab)
        {
            LogActivity("Window switch detected", ActivityType.WindowSwitch);
        }
        
        // Print Screen - Screenshot attempt
        if (key == Keys.PrintScreen)
        {
            LogActivity("Screenshot attempted", ActivityType.Screenshot);
            
            if (_config.BlockScreenshots)
            {
                // In production, would block the screenshot
                RaiseThreat(ThreatType.Screenshot, "Screenshot attempt detected", 
                    ThreatLevel.Low);
            }
        }
    }
    
    private void AnalyzeTypedText()
    {
        if (_keyBuffer.Length == 0) return;
        
        var text = _keyBuffer.ToString().Trim();
        if (string.IsNullOrWhiteSpace(text)) 
        {
            _keyBuffer.Clear();
            return;
        }
        
        // Store recent text for context
        _recentTexts.Enqueue(text);
        if (_recentTexts.Count > 20)
            _recentTexts.Dequeue();
        
        // Check for bad words
        var result = _badWordsDetector.Analyze(text);
        
        if (!result.IsClean)
        {
            RaiseThreat(ThreatType.InappropriateContent, 
                $"Inappropriate content typed: {result.FoundWords.Count} bad words detected",
                result.Severity == DetectionSeverity.High ? ThreatLevel.High : ThreatLevel.Medium,
                text);
        }
        
        // Check for URLs in text
        ExtractAndCheckUrls(text);
        
        _keyBuffer.Clear();
    }
    
    private void ExtractAndCheckUrls(string text)
    {
        // Simple URL extraction
        var words = text.Split(' ', '\t', '\n');
        foreach (var word in words)
        {
            if (IsUrl(word))
            {
                Task.Run(() => CheckUrl(word));
            }
        }
    }
    
    private async Task CheckUrl(string url)
    {
        // Avoid duplicate checks
        lock (_recentUrls)
        {
            if (_recentUrls.Contains(url))
                return;
                
            _recentUrls.Add(url);
            if (_recentUrls.Count > 100)
                _recentUrls.RemoveAt(0);
        }
        
        _urlsChecked++;
        
        try
        {
            var result = await _urlChecker.CheckUrlAsync(url);
            
            if (!result.IsSafe)
            {
                RaiseThreat(ThreatType.UnsafeUrl,
                    $"Unsafe URL detected: {url}",
                    result.ThreatLevel == UrlThreatLevel.High ? 
                        ThreatLevel.High : ThreatLevel.Medium,
                    url);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"URL check failed: {ex.Message}");
        }
    }
    
    private async Task CheckClipboard()
    {
        try
        {
            await Task.Run(() =>
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    
                    // Check for URLs
                    if (IsUrl(text))
                    {
                        _ = CheckUrl(text);
                    }
                    
                    // Check for inappropriate content
                    var result = _badWordsDetector.Analyze(text);
                    if (!result.IsClean)
                    {
                        RaiseThreat(ThreatType.InappropriateContent,
                            "Inappropriate content in clipboard",
                            ThreatLevel.Low, text);
                    }
                }
            });
        }
        catch { }
    }
    
    private async Task<string> GetClickedUrl()
    {
        // This would use UI Automation in production
        // For now, check if we're in a browser
        try
        {
            var activeWindow = GetForegroundWindow();
            var className = GetWindowClassName(activeWindow);
            
            if (IsBrowserWindow(className))
            {
                // In production, extract URL from browser
                // For demo, return empty
                return string.Empty;
            }
        }
        catch { }
        
        return string.Empty;
    }
    
    private void HandleSpeechDetected(object sender, SpeechDetectedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Text))
        {
            var result = _badWordsDetector.Analyze(e.Text);
            
            if (!result.IsClean)
            {
                RaiseThreat(ThreatType.InappropriateSpeech,
                    $"Inappropriate speech detected: {result.FoundWords.Count} bad words",
                    result.Severity == DetectionSeverity.High ? 
                        ThreatLevel.High : ThreatLevel.Medium,
                    e.Text);
            }
        }
    }
    
    private void HandleLoudNoise(object sender, AudioLevelEventArgs e)
    {
        if (e.Level > 0.9)
        {
            LogActivity($"Very loud noise detected: {e.Level:P0}", 
                ActivityType.AudioEvent);
        }
    }
    
    private void PeriodicAnalysis(object state)
    {
        // Analyze recent activity patterns
        if (_recentTexts.Count > 10)
        {
            var allText = string.Join(" ", _recentTexts);
            var result = _badWordsDetector.Analyze(allText);
            
            if (!result.IsClean && result.FoundWords.Count > 5)
            {
                RaiseThreat(ThreatType.PatternDetected,
                    "Pattern of inappropriate content detected",
                    ThreatLevel.High);
            }
        }
        
        UpdateStatistics();
    }
    
    private void RaiseThreat(ThreatType type, string description, 
        ThreatLevel level, string content = null)
    {
        _threatsDetected++;
        
        var args = new ThreatDetectedEventArgs
        {
            Timestamp = DateTime.Now,
            Type = type,
            Description = description,
            Level = level,
            Content = content,
            Source = DetermineSource(type)
        };
        
        OnThreatDetected?.Invoke(this, args);
        
        LogActivity($"Threat detected: {type} - {description}", 
            ActivityType.ThreatDetected);
    }
    
    private string DetermineSource(ThreatType type)
    {
        return type switch
        {
            ThreatType.InappropriateContent => "Keyboard",
            ThreatType.UnsafeUrl => "Web",
            ThreatType.InappropriateSpeech => "Microphone",
            ThreatType.Screenshot => "System",
            _ => "Unknown"
        };
    }
    
    private void LogActivity(string description, ActivityType type)
    {
        OnActivity?.Invoke(this, new ChildGuard.Core.Models.ActivityEvent
        {
            Timestamp = DateTime.Now,
            Description = description,
            Data = type
        });
    }
    
    private void UpdateStatistics()
    {
        OnStatisticsUpdated?.Invoke(this, new StatisticsUpdatedEventArgs
        {
            TotalKeysPressed = _totalKeysPressed,
            TotalMouseClicks = _totalMouseClicks,
            ThreatsDetected = _threatsDetected,
            UrlsChecked = _urlsChecked
        });
    }
    
    private bool IsUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        
        return text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
               text.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
               (text.Contains(".") && (
                   text.Contains(".com") || text.Contains(".org") || 
                   text.Contains(".net") || text.Contains(".edu") ||
                   text.Contains(".gov") || text.Contains(".io")));
    }
    
    private bool IsBrowserWindow(string className)
    {
        return className.Contains("Chrome") || 
               className.Contains("Firefox") ||
               className.Contains("Edge") || 
               className.Contains("Opera") ||
               className.Contains("Safari") ||
               className.Contains("IEFrame");
    }
    
    private bool IsTextKey(Keys key)
    {
        return (key >= Keys.A && key <= Keys.Z) ||
               (key >= Keys.D0 && key <= Keys.D9) ||
               (key >= Keys.NumPad0 && key <= Keys.NumPad9) ||
               key == Keys.Space || key == Keys.Enter || key == Keys.Tab ||
               key == Keys.OemPeriod || key == Keys.OemComma ||
               key == Keys.OemQuestion || key == Keys.OemSemicolon ||
               key == Keys.OemQuotes || key == Keys.OemMinus ||
               key == Keys.OemPlus;
    }
    
    private char GetCharFromKey(Keys key)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
            bool caps = Control.IsKeyLocked(Keys.CapsLock);
            bool upper = shift ^ caps;
            return (char)(upper ? key : key + 32);
        }
        
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                // Shift + number keys
                char[] shiftChars = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
                return shiftChars[key - Keys.D0];
            }
            return (char)('0' + (key - Keys.D0));
        }
        
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            return (char)('0' + (key - Keys.NumPad0));
            
        switch (key)
        {
            case Keys.Space: return ' ';
            case Keys.Enter: return '\n';
            case Keys.Tab: return '\t';
            case Keys.OemPeriod: return '.';
            case Keys.OemComma: return ',';
            case Keys.OemQuestion: return '?';
            case Keys.OemSemicolon: return ';';
            case Keys.OemQuotes: return '"';
            case Keys.OemMinus: return '-';
            case Keys.OemPlus: return '+';
            default: return '\0';
        }
    }
    
    private string GetWindowClassName(IntPtr hWnd)
    {
        var className = new StringBuilder(256);
        GetClassName(hWnd, className, className.Capacity);
        return className.ToString();
    }
    
    public void Dispose()
    {
        Stop();
        _periodicAnalysisTimer?.Dispose();
        _audioMonitor?.Dispose();
    }
    
    // Windows API declarations
    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
}

// Event arguments classes
public class ThreatDetectedEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public ThreatType Type { get; set; }
    public string Description { get; set; }
    public ThreatLevel Level { get; set; }
    public string Content { get; set; }
    public string Source { get; set; }
}

public class StatisticsUpdatedEventArgs : EventArgs
{
    public int TotalKeysPressed { get; set; }
    public int TotalMouseClicks { get; set; }
    public int ThreatsDetected { get; set; }
    public int UrlsChecked { get; set; }
}

// Enumerations
public enum ThreatType
{
    InappropriateContent,
    UnsafeUrl,
    InappropriateSpeech,
    Screenshot,
    PatternDetected,
    SuspiciousActivity
}

public enum ThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum ActivityType
{
    KeyPress,
    MouseClick,
    WindowSwitch,
    Screenshot,
    AudioEvent,
    ThreatDetected,
    SystemEvent
}
