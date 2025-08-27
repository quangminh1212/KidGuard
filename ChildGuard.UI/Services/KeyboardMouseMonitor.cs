using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;
using ChildGuard.Core.Services;
using Timer = System.Threading.Timer;

namespace ChildGuard.UI.Services
{
    /// <summary>
    /// Service giám sát keyboard và mouse sử dụng global hooks
    /// </summary>
    public class KeyboardMouseMonitor : IDisposable
    {
        private IKeyboardMouseEvents? _globalHook;
        private readonly StringBuilder _keyBuffer;
        private readonly Queue<string> _recentWords;
        private DateTime _lastKeystrokeTime;
        private Timer? _flushTimer;
        private readonly object _bufferLock = new object();
        private bool _isMonitoring;
        private readonly ServiceManager _serviceManager;
        
        // Configuration
        private const int MaxBufferSize = 1000;
        private const int MaxWordHistory = 50;
        private const int FlushIntervalMs = 5000;
        private const int InactivityThresholdMs = 3000;

        // Events
        public event EventHandler<KeystrokeEventArgs>? KeystrokeDetected;
        public event EventHandler<MouseActivityEventArgs>? MouseActivityDetected;
        public event EventHandler<string>? BufferTextChanged;
        
        public KeyboardMouseMonitor()
        {
            _serviceManager = ServiceManager.Instance;
            _keyBuffer = new StringBuilder();
            _recentWords = new Queue<string>();
            _lastKeystrokeTime = DateTime.UtcNow;
            _isMonitoring = false;
        }

        public bool IsMonitoring => _isMonitoring;
        
        public void StartMonitoring()
        {
            if (_isMonitoring) return;
            
            try
            {
                // Subscribe to global hooks
                _globalHook = Hook.GlobalEvents();
                
                // Keyboard events
                _globalHook.KeyPress += OnKeyPress;
                _globalHook.KeyDown += OnKeyDown;
                
                // Mouse events (optional - can be intensive)
                _globalHook.MouseClick += OnMouseClick;
                _globalHook.MouseWheel += OnMouseWheel;
                
                // Start flush timer
                _flushTimer = new Timer(OnFlushTimer, null, FlushIntervalMs, FlushIntervalMs);
                
                _isMonitoring = true;
                
                // Log start event
                _serviceManager.EventDispatcher?.PublishAsync(new SystemEvent
                {
                    Message = "Keyboard and mouse monitoring started",
                    Level = EventLevel.Info
                });
            }
            catch (Exception ex)
            {
                _serviceManager.EventDispatcher?.PublishAsync(new SystemEvent
                {
                    Message = $"Failed to start keyboard/mouse monitoring: {ex.Message}",
                    Level = EventLevel.Error
                });
                throw;
            }
        }
        
        public void StopMonitoring()
        {
            if (!_isMonitoring) return;
            
            try
            {
                // Unsubscribe events
                if (_globalHook != null)
                {
                    _globalHook.KeyPress -= OnKeyPress;
                    _globalHook.KeyDown -= OnKeyDown;
                    _globalHook.MouseClick -= OnMouseClick;
                    _globalHook.MouseWheel -= OnMouseWheel;
                    _globalHook.Dispose();
                    _globalHook = null;
                }
                
                // Stop timer
                _flushTimer?.Dispose();
                _flushTimer = null;
                
                // Flush remaining buffer
                FlushBuffer(true);
                
                _isMonitoring = false;
                
                // Log stop event
                _serviceManager.EventDispatcher?.PublishAsync(new SystemEvent
                {
                    Message = "Keyboard and mouse monitoring stopped",
                    Level = EventLevel.Info
                });
            }
            catch (Exception ex)
            {
                _serviceManager.EventDispatcher?.PublishAsync(new SystemEvent
                {
                    Message = $"Error stopping keyboard/mouse monitoring: {ex.Message}",
                    Level = EventLevel.Warning
                });
            }
        }
        
        private void OnKeyPress(object? sender, KeyPressEventArgs e)
        {
            lock (_bufferLock)
            {
                _lastKeystrokeTime = DateTime.UtcNow;
                
                // Add character to buffer
                _keyBuffer.Append(e.KeyChar);
                
                // Check for word boundaries (space, enter, punctuation)
                if (char.IsWhiteSpace(e.KeyChar) || char.IsPunctuation(e.KeyChar))
                {
                    ProcessWordBoundary();
                }
                
                // Prevent buffer overflow
                if (_keyBuffer.Length > MaxBufferSize)
                {
                    _keyBuffer.Remove(0, _keyBuffer.Length - MaxBufferSize);
                }
                
                // Notify buffer change
                BufferTextChanged?.Invoke(this, _keyBuffer.ToString());
            }
        }
        
        private void OnKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            lock (_bufferLock)
            {
                _lastKeystrokeTime = DateTime.UtcNow;
                
                // Handle special keys
                switch (e.KeyCode)
                {
                    case Keys.Back:
                        if (_keyBuffer.Length > 0)
                            _keyBuffer.Length--;
                        break;
                        
                    case Keys.Delete:
                        // Can't track cursor position in global context
                        break;
                        
                    case Keys.Enter:
                        ProcessWordBoundary();
                        _keyBuffer.AppendLine();
                        CheckForPatterns();
                        break;
                        
                    case Keys.Tab:
                        _keyBuffer.Append('\t');
                        ProcessWordBoundary();
                        break;
                }
                
                // Detect Ctrl+C, Ctrl+V (clipboard operations)
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            LogKeystrokeEvent("Clipboard Copy", EventLevel.Info);
                            break;
                        case Keys.V:
                            LogKeystrokeEvent("Clipboard Paste", EventLevel.Info);
                            // Could read clipboard content if needed
                            break;
                    }
                }
            }
        }
        
        private void OnMouseClick(object? sender, MouseEventArgs e)
        {
            // Log significant mouse events only
            MouseActivityDetected?.Invoke(this, new MouseActivityEventArgs
            {
                Action = "Click",
                Button = e.Button.ToString(),
                Location = e.Location,
                Timestamp = DateTime.UtcNow
            });
            
            // Consider click as potential word boundary
            lock (_bufferLock)
            {
                ProcessWordBoundary();
            }
        }
        
        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            // Log scroll activity (can be noisy, consider throttling)
            MouseActivityDetected?.Invoke(this, new MouseActivityEventArgs
            {
                Action = "Scroll",
                ScrollDelta = e.Delta,
                Location = e.Location,
                Timestamp = DateTime.UtcNow
            });
        }
        
        private void ProcessWordBoundary()
        {
            // Extract last word from buffer
            var text = _keyBuffer.ToString();
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length > 0)
            {
                var lastWord = words.Last();
                if (!string.IsNullOrWhiteSpace(lastWord))
                {
                    _recentWords.Enqueue(lastWord);
                    
                    // Maintain word history size
                    while (_recentWords.Count > MaxWordHistory)
                    {
                        _recentWords.Dequeue();
                    }
                    
                    // Check for URL patterns
                    CheckForUrlPattern(lastWord);
                }
            }
        }
        
        private void CheckForPatterns()
        {
            var text = _keyBuffer.ToString();
            
            // Check for URLs
            var urlPatterns = new[] { "http://", "https://", "www.", ".com", ".org", ".net" };
            foreach (var pattern in urlPatterns)
            {
                if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    var urls = ExtractUrls(text);
                    foreach (var url in urls)
                    {
                        _serviceManager.EventDispatcher?.PublishAsync(new UrlVisitedEvent
                        {
                            Url = url,
                            Source = "Keyboard Input",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
            
            // Check for email patterns
            if (text.Contains('@') && text.Contains('.'))
            {
                var emails = ExtractEmails(text);
                foreach (var email in emails)
                {
                    LogKeystrokeEvent($"Email entered: {MaskEmail(email)}", EventLevel.Info);
                }
            }
        }
        
        private void CheckForUrlPattern(string word)
        {
            if (word.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                word.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                word.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
                (word.Contains('.') && (word.EndsWith(".com") || word.EndsWith(".org") || 
                 word.EndsWith(".net") || word.EndsWith(".edu"))))
            {
                _serviceManager.EventDispatcher?.PublishAsync(new UrlVisitedEvent
                {
                    Url = word,
                    Source = "Keyboard Input",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        
        private List<string> ExtractUrls(string text)
        {
            var urls = new List<string>();
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (Uri.TryCreate(word, UriKind.Absolute, out var uri))
                {
                    urls.Add(uri.ToString());
                }
                else if (!word.StartsWith("http", StringComparison.OrdinalIgnoreCase) && word.Contains('.'))
                {
                    // Try adding http:// prefix
                    if (Uri.TryCreate($"http://{word}", UriKind.Absolute, out uri))
                    {
                        urls.Add(word); // Store original
                    }
                }
            }
            
            return urls.Distinct().ToList();
        }
        
        private List<string> ExtractEmails(string text)
        {
            var emails = new List<string>();
            var pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var matches = System.Text.RegularExpressions.Regex.Matches(text, pattern);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                emails.Add(match.Value);
            }
            
            return emails;
        }
        
        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            
            var username = parts[0];
            if (username.Length > 3)
            {
                username = username.Substring(0, 2) + new string('*', username.Length - 3) + username.Last();
            }
            
            return $"{username}@{parts[1]}";
        }
        
        private void OnFlushTimer(object? state)
        {
            lock (_bufferLock)
            {
                // Check for inactivity
                var inactivityMs = (DateTime.UtcNow - _lastKeystrokeTime).TotalMilliseconds;
                if (inactivityMs > InactivityThresholdMs && _keyBuffer.Length > 0)
                {
                    FlushBuffer(false);
                }
            }
        }
        
        private void FlushBuffer(bool force)
        {
            if (!force && _keyBuffer.Length < 10) return; // Don't flush small buffers unless forced
            
            var text = _keyBuffer.ToString().Trim();
            if (string.IsNullOrEmpty(text)) return;
            
            // Create keystroke event
            var keystrokeEvent = new KeystrokeEvent
            {
                Text = text,
                WordCount = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length,
                CharacterCount = text.Length,
                Timestamp = DateTime.UtcNow
            };
            
            // Publish event
            _serviceManager.EventDispatcher?.PublishAsync(keystrokeEvent);
            
            // Raise local event
            KeystrokeDetected?.Invoke(this, new KeystrokeEventArgs
            {
                Text = text,
                Timestamp = DateTime.UtcNow
            });
            
            // Clear buffer
            _keyBuffer.Clear();
        }
        
        private void LogKeystrokeEvent(string message, EventLevel level)
        {
            _serviceManager.EventDispatcher?.PublishAsync(new KeystrokeEvent
            {
                Text = message,
                WordCount = 0,
                CharacterCount = 0,
                Timestamp = DateTime.UtcNow
            });
        }
        
        public string GetCurrentBuffer()
        {
            lock (_bufferLock)
            {
                return _keyBuffer.ToString();
            }
        }
        
        public string[] GetRecentWords()
        {
            lock (_bufferLock)
            {
                return _recentWords.ToArray();
            }
        }
        
        public void ClearBuffer()
        {
            lock (_bufferLock)
            {
                _keyBuffer.Clear();
                _recentWords.Clear();
            }
        }
        
        public void Dispose()
        {
            StopMonitoring();
        }
    }
    
    // Event argument classes
    public class KeystrokeEventArgs : EventArgs
    {
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
    
    public class MouseActivityEventArgs : EventArgs
    {
        public string Action { get; set; } = string.Empty;
        public string? Button { get; set; }
        public System.Drawing.Point Location { get; set; }
        public int ScrollDelta { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
