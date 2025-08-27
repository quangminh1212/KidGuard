using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Detection;
using ChildGuard.Core.Models;

namespace ChildGuard.Hooking;

public class EnhancedHookManager : IDisposable
{
    private readonly BadWordsDetector _badWordsDetector;
    private readonly UrlSafetyChecker _urlChecker;
    private readonly StringBuilder _keyBuffer;
    private readonly List<string> _recentUrls;
    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;
    private LowLevelKeyboardProc _keyboardProc;
    private LowLevelMouseProc _mouseProc;
    private bool _isRunning;
    
    public event EventHandler<ContentDetectionEventArgs> OnContentDetected;
    public event EventHandler<UrlDetectionEventArgs> OnUrlDetected;
    public event EventHandler<ChildGuard.Core.Models.ActivityEvent> OnActivity;
    
    public EnhancedHookManager()
    {
        _badWordsDetector = new BadWordsDetector();
        _urlChecker = new UrlSafetyChecker();
        _keyBuffer = new StringBuilder(1000);
        _recentUrls = new List<string>();
    }
    
    public void Start(AppConfig config)
    {
        if (_isRunning) return;
        
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
        
        _isRunning = true;
    }
    
    public void Stop()
    {
        if (!_isRunning) return;
        
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
        
        _isRunning = false;
    }
    
    private IntPtr HookKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            
            // Build text buffer
            if (IsTextKey(key))
            {
                var keyChar = GetCharFromKey(key);
                _keyBuffer.Append(keyChar);
                
                // Check for space or enter to analyze words
                if (key == Keys.Space || key == Keys.Enter)
                {
                    AnalyzeTypedText();
                }
            }
            
            // Check for Ctrl+V (paste)
            if (key == Keys.V && (Control.ModifierKeys & Keys.Control) != 0)
            {
                Task.Run(async () => await CheckClipboardForUrl());
            }
            
            // Buffer management
            if (_keyBuffer.Length > 500)
            {
                AnalyzeTypedText();
                _keyBuffer.Clear();
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
                // Get clicked element text if possible
                Task.Run(async () =>
                {
                    var url = GetUrlUnderCursor();
                    if (!string.IsNullOrEmpty(url))
                    {
                        await CheckUrl(url);
                    }
                });
            }
        }
        
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }
    
    private void AnalyzeTypedText()
    {
        if (_keyBuffer.Length == 0) return;
        
        var text = _keyBuffer.ToString();
        var result = _badWordsDetector.Analyze(text);
        
        if (!result.IsClean)
        {
            OnContentDetected?.Invoke(this, new ContentDetectionEventArgs
            {
                Timestamp = DateTime.Now,
                Content = text,
                DetectionResult = result,
                Source = "Keyboard"
            });
        }
        
        _keyBuffer.Clear();
    }
    
    private async Task CheckUrl(string url)
    {
        // Avoid checking same URL multiple times
        if (_recentUrls.Contains(url))
            return;
            
        _recentUrls.Add(url);
        if (_recentUrls.Count > 100)
            _recentUrls.RemoveAt(0);
        
        var result = await _urlChecker.CheckUrlAsync(url);
        
        if (!result.IsSafe)
        {
            OnUrlDetected?.Invoke(this, new UrlDetectionEventArgs
            {
                Timestamp = DateTime.Now,
                Url = url,
                CheckResult = result
            });
        }
    }
    
    private async Task CheckClipboardForUrl()
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (IsUrl(text))
                {
                    await CheckUrl(text);
                }
            }
        }
        catch { }
    }
    
    private string GetUrlUnderCursor()
    {
        // This would use UI Automation or accessibility APIs
        // to get the URL/link under the cursor
        // Simplified implementation
        try
        {
            // Check if we're in a browser
            var activeWindow = GetForegroundWindow();
            var className = GetWindowClassName(activeWindow);
            
            if (className.Contains("Chrome") || className.Contains("Firefox") || 
                className.Contains("Edge") || className.Contains("IEFrame"))
            {
                // In production, use UI Automation to get URL
                // For now, return empty
                return string.Empty;
            }
        }
        catch { }
        
        return string.Empty;
    }
    
    private bool IsUrl(string text)
    {
        return text.StartsWith("http://") || 
               text.StartsWith("https://") ||
               text.Contains("www.") ||
               text.Contains(".com") ||
               text.Contains(".org") ||
               text.Contains(".net");
    }
    
    private bool IsTextKey(Keys key)
    {
        return (key >= Keys.A && key <= Keys.Z) ||
               (key >= Keys.D0 && key <= Keys.D9) ||
               key == Keys.Space ||
               key == Keys.Enter ||
               key == Keys.OemPeriod ||
               key == Keys.OemComma;
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
            return (char)('0' + (key - Keys.D0));
            
        if (key == Keys.Space) return ' ';
        if (key == Keys.OemPeriod) return '.';
        if (key == Keys.OemComma) return ',';
        
        return '\0';
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

public class ContentDetectionEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
    public DetectionResult DetectionResult { get; set; }
    public string Source { get; set; }
}

public class UrlDetectionEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public string Url { get; set; }
    public UrlCheckResult CheckResult { get; set; }
}
