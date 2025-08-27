using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;
using ChildGuard.UI.Controls;

namespace ChildGuard.UI.Services
{
    /// <summary>
    /// Event args for toast notification activation
    /// </summary>
    public class ToastNotificationActivatedEventArgs : EventArgs
    {
        public string Argument { get; set; } = string.Empty;
        public Dictionary<string, string> UserInput { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Service quản lý thông báo: Toast, System Tray, và Windows Notifications
    /// </summary>
    public class NotificationService : IEventSource, IDisposable
    {
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;
        private Form? _mainForm;
        private readonly Queue<ModernToast> _toastQueue;
        private readonly object _lockObject = new object();
        private bool _isSilentMode = false;
        private DateTime? _silentUntil;
        private int _maxToasts = 3;
        
        // Event source implementation
        public string SourceName => "NotificationService";
        public IEventDispatcher? Dispatcher { get; set; }
        
        // Events
        public event EventHandler<NotificationEventArgs>? NotificationShown;
        public event EventHandler? TrayIconClicked;
        public event EventHandler<string>? TrayMenuItemClicked;
        
        public bool IsSilentMode 
        { 
            get => _isSilentMode || (_silentUntil.HasValue && _silentUntil.Value > DateTime.Now);
            set => _isSilentMode = value;
        }
        
        public NotificationService()
        {
            _toastQueue = new Queue<ModernToast>();
        }
        
        /// <summary>
        /// Khởi tạo service với form chính
        /// </summary>
        public void Initialize(Form mainForm)
        {
            _mainForm = mainForm;
            SetupSystemTray();
            
            // Register for Windows 10 toast notifications
            // Note: ToastNotificationManagerCompat requires additional setup
            // For now, we'll use system tray and in-app toasts
        }
        
        /// <summary>
        /// Thiết lập System Tray
        /// </summary>
        private void SetupSystemTray()
        {
            // Create tray menu
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Mở ChildGuard", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "open"));
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Dashboard", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "dashboard"));
            _trayMenu.Items.Add("Monitoring", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "monitoring"));
            _trayMenu.Items.Add("Reports", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "reports"));
            _trayMenu.Items.Add("Settings", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "settings"));
            _trayMenu.Items.Add(new ToolStripSeparator());
            
            var silentModeItem = new ToolStripMenuItem("Chế độ im lặng");
            silentModeItem.CheckOnClick = true;
            silentModeItem.Click += (s, e) => 
            {
                IsSilentMode = silentModeItem.Checked;
                ShowInfo("Chế độ im lặng", IsSilentMode ? "Đã bật chế độ im lặng" : "Đã tắt chế độ im lặng");
            };
            _trayMenu.Items.Add(silentModeItem);
            
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Thoát", null, (s, e) => TrayMenuItemClicked?.Invoke(this, "exit"));
            
            // Create tray icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = GetAppIcon();
            _trayIcon.Text = "ChildGuard - Bảo vệ trẻ em";
            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.Visible = true;
            
            _trayIcon.Click += (s, e) =>
            {
                if (e is MouseEventArgs me && me.Button == MouseButtons.Left)
                {
                    TrayIconClicked?.Invoke(this, EventArgs.Empty);
                }
            };
        }
        
        /// <summary>
        /// Hiển thị thông báo thông tin
        /// </summary>
        public void ShowInfo(string title, string message, Dictionary<string, object>? metadata = null)
        {
            if (IsSilentMode) return;
            
            ShowNotification(title, message, NotificationType.Info, metadata);
        }
        
        /// <summary>
        /// Hiển thị cảnh báo
        /// </summary>
        public void ShowWarning(string title, string message, Dictionary<string, object>? metadata = null)
        {
            if (IsSilentMode) return;
            
            ShowNotification(title, message, NotificationType.Warning, metadata);
        }
        
        /// <summary>
        /// Hiển thị lỗi
        /// </summary>
        public void ShowError(string title, string message, Dictionary<string, object>? metadata = null)
        {
            ShowNotification(title, message, NotificationType.Error, metadata);
        }
        
        /// <summary>
        /// Hiển thị thông báo threat
        /// </summary>
        public void ShowThreat(string title, string message, Dictionary<string, object>? metadata = null)
        {
            // Always show threat notifications even in silent mode
            ShowNotification(title, message, NotificationType.Threat, metadata, true);
        }
        
        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public void ShowSuccess(string title, string message, Dictionary<string, object>? metadata = null)
        {
            if (IsSilentMode) return;
            
            ShowNotification(title, message, NotificationType.Success, metadata);
        }
        
        /// <summary>
        /// Core notification method
        /// </summary>
        private void ShowNotification(string title, string message, NotificationType type, 
            Dictionary<string, object>? metadata = null, bool forceShow = false)
        {
            if (!forceShow && IsSilentMode) return;
            
            // Show Windows 10 toast if available
            try
            {
                ShowWindowsToast(title, message, type, metadata);
            }
            catch
            {
                // Fallback to other notification methods
            }
            
            // Show system tray balloon
            ShowTrayBalloon(title, message, type);
            
            // Show in-app toast
            ShowInAppToast(title, message, type);
            
            // Fire event
            NotificationShown?.Invoke(this, new NotificationEventArgs
            {
                Title = title,
                Message = message,
                Type = type,
                Metadata = metadata,
                Timestamp = DateTime.Now
            });
            
            // Publish to event bus
            if (Dispatcher != null)
            {
                var severity = type switch
                {
                    NotificationType.Threat => EventSeverity.Critical,
                    NotificationType.Error => EventSeverity.High,
                    NotificationType.Warning => EventSeverity.Medium,
                    _ => EventSeverity.Info
                };
                
                var notificationEvent = new NotificationEvent(title, message, severity, metadata);
                Dispatcher.PublishAsync(notificationEvent);
            }
        }
        
        /// <summary>
        /// Show Windows 10 toast notification
        /// </summary>
        private void ShowWindowsToast(string title, string message, NotificationType type, 
            Dictionary<string, object>? metadata)
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);
            
            // Add action buttons based on type
            if (type == NotificationType.Threat)
            {
                builder.AddButton(new ToastButton()
                    .SetContent("Xem chi tiết")
                    .AddArgument("action", "view")
                    .AddArgument("type", "threat"));
                
                builder.AddButton(new ToastButton()
                    .SetContent("Bỏ qua")
                    .AddArgument("action", "dismiss"));
            }
            
            // Set audio based on type
            switch (type)
            {
                case NotificationType.Threat:
                case NotificationType.Error:
                    builder.AddAudio(new ToastAudio() { Silent = false });
                    break;
                case NotificationType.Warning:
                    builder.AddAudio(new ToastAudio() { Silent = false });
                    break;
                default:
                    builder.AddAudio(new ToastAudio() { Silent = true });
                    break;
            }
            
            // Show toast notification
            // Note: Requires Windows app manifest configuration
            try
            {
                var toastContent = builder.GetToastContent();
                var xml = toastContent.GetContent();
                // For now, we'll rely on system tray notifications
            }
            catch
            {
                // Toast notifications not available
            }
        }
        
        /// <summary>
        /// Show system tray balloon
        /// </summary>
        private void ShowTrayBalloon(string title, string message, NotificationType type)
        {
            if (_trayIcon == null) return;
            
            var icon = type switch
            {
                NotificationType.Error => ToolTipIcon.Error,
                NotificationType.Warning => ToolTipIcon.Warning,
                NotificationType.Threat => ToolTipIcon.Warning,
                _ => ToolTipIcon.Info
            };
            
            _trayIcon.ShowBalloonTip(5000, title, message, icon);
        }
        
        /// <summary>
        /// Show system tray notification (public method)
        /// </summary>
        public void ShowTrayNotification(string title, string message, NotificationType type)
        {
            ShowTrayBalloon(title, message, type);
        }
        
        /// <summary>
        /// Show in-app toast
        /// </summary>
        private void ShowInAppToast(string title, string message, NotificationType type)
        {
            if (_mainForm == null || _mainForm.IsDisposed) return;
            
            _mainForm.BeginInvoke(new Action(() =>
            {
                lock (_lockObject)
                {
                    // Remove old toasts if too many
                    while (_toastQueue.Count >= _maxToasts)
                    {
                        var oldToast = _toastQueue.Dequeue();
                        oldToast.Close();
                    }
                    
                    // Create new toast
                    var toast = new ModernToast();
                    toast.Show(title, message, ConvertToToastType(type));
                    
                    // Position toast
                    var yPos = 20;
                    foreach (var existingToast in _toastQueue)
                    {
                        yPos += existingToast.Height + 10;
                    }
                    
                    toast.Location = new Point(_mainForm.Width - toast.Width - 20, yPos);
                    toast.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    
                    // Add to form
                    _mainForm.Controls.Add(toast);
                    toast.BringToFront();
                    
                    // Track toast
                    _toastQueue.Enqueue(toast);
                    
                    // Remove from queue when closed
                    toast.Closed += (s, e) =>
                    {
                        lock (_lockObject)
                        {
                            if (_toastQueue.Contains(toast))
                            {
                                var toasts = _toastQueue.ToArray();
                                _toastQueue.Clear();
                                foreach (var t in toasts.Where(t => t != toast))
                                {
                                    _toastQueue.Enqueue(t);
                                }
                                
                                // Reposition remaining toasts
                                RepositionToasts();
                            }
                        }
                    };
                }
            }));
        }
        
        private void RepositionToasts()
        {
            var yPos = 20;
            foreach (var toast in _toastQueue)
            {
                toast.Location = new Point(toast.Location.X, yPos);
                yPos += toast.Height + 10;
            }
        }
        
        private ModernToast.ToastType ConvertToToastType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => ModernToast.ToastType.Success,
                NotificationType.Warning => ModernToast.ToastType.Warning,
                NotificationType.Error => ModernToast.ToastType.Error,
                NotificationType.Threat => ModernToast.ToastType.Threat,
                _ => ModernToast.ToastType.Info
            };
        }
        
        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgs e)
        {
            // Handle toast activation  
            var args = ToastArguments.Parse(e.Argument);
            
            if (args.TryGetValue("action", out var action))
            {
                _mainForm?.BeginInvoke(new Action(() =>
                {
                    TrayMenuItemClicked?.Invoke(this, action);
                }));
            }
        }
        
        /// <summary>
        /// Set silent mode for duration
        /// </summary>
        public void SetSilentMode(TimeSpan duration)
        {
            _silentUntil = DateTime.Now.Add(duration);
            ShowInfo("Chế độ im lặng", $"Đã bật chế độ im lặng trong {duration.TotalMinutes} phút");
        }
        
        /// <summary>
        /// Clear all toasts
        /// </summary>
        public void ClearAllToasts()
        {
            lock (_lockObject)
            {
                while (_toastQueue.Count > 0)
                {
                    var toast = _toastQueue.Dequeue();
                    toast.Close();
                }
            }
        }
        
        private Icon GetAppIcon()
        {
            try
            {
                // Try to load app icon from resources or file
                return SystemIcons.Shield;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }
        
        public void Dispose()
        {
            ClearAllToasts();
            _trayIcon?.Dispose();
            _trayMenu?.Dispose();
        }
    }
    
    /// <summary>
    /// Notification event arguments
    /// </summary>
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Notification types
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Threat
    }
}
