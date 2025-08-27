using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Modern toast notification control với animation
    /// </summary>
    public class ModernToast : Control
    {
        public enum ToastType
        {
            Info,
            Success,
            Warning,
            Error,
            Threat
        }
        
        private string _title = string.Empty;
        private string _message = string.Empty;
        private ToastType _type = ToastType.Info;
        private System.Windows.Forms.Timer _animationTimer;
        private System.Windows.Forms.Timer _autoHideTimer;
        private float _animationProgress = 0f;
        private bool _isAnimatingIn = true;
        private bool _isClosing = false;
        private int _autoHideDuration = 5000; // 5 seconds
        
        // Events
        public event EventHandler? Closed;
        public event EventHandler? Clicked;
        
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }
        
        public string Message
        {
            get => _message;
            set { _message = value; Invalidate(); }
        }
        
        public ToastType Type
        {
            get => _type;
            set { _type = value; Invalidate(); }
        }
        
        public int AutoHideDuration
        {
            get => _autoHideDuration;
            set { _autoHideDuration = value; RestartAutoHideTimer(); }
        }
        
        public ModernToast()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);
            
            Size = new Size(350, 100);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            
            // Setup animation timer
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16; // ~60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
            
            // Setup auto-hide timer
            _autoHideTimer = new System.Windows.Forms.Timer();
            _autoHideTimer.Interval = _autoHideDuration;
            _autoHideTimer.Tick += AutoHideTimer_Tick;
            _autoHideTimer.Start();
        }
        
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_isAnimatingIn)
            {
                _animationProgress += 0.08f;
                if (_animationProgress >= 1f)
                {
                    _animationProgress = 1f;
                    _isAnimatingIn = false;
                }
            }
            else if (_isClosing)
            {
                _animationProgress -= 0.08f;
                if (_animationProgress <= 0f)
                {
                    _animationProgress = 0f;
                    _animationTimer.Stop();
                    _autoHideTimer.Stop();
                    Closed?.Invoke(this, EventArgs.Empty);
                    if (Parent != null)
                    {
                        Parent.Controls.Remove(this);
                    }
                    Dispose();
                }
            }
            
            Invalidate();
        }
        
        private void AutoHideTimer_Tick(object? sender, EventArgs e)
        {
            Close();
        }
        
        public void Show(string title, string message, ToastType type = ToastType.Info, int duration = 5000)
        {
            _title = title;
            _message = message;
            _type = type;
            _autoHideDuration = duration;
            
            RestartAutoHideTimer();
            _isAnimatingIn = true;
            _isClosing = false;
            _animationProgress = 0f;
            _animationTimer.Start();
            
            Invalidate();
        }
        
        public void Close()
        {
            if (!_isClosing)
            {
                _isClosing = true;
                _isAnimatingIn = false;
                _autoHideTimer.Stop();
            }
        }
        
        private void RestartAutoHideTimer()
        {
            if (_autoHideTimer != null)
            {
                _autoHideTimer.Stop();
                _autoHideTimer.Interval = _autoHideDuration;
                _autoHideTimer.Start();
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Calculate animated position and opacity
            var opacity = (int)(255 * _animationProgress);
            var yOffset = (int)((1f - _animationProgress) * 20);
            var rect = new Rectangle(0, yOffset, Width - 1, Height - 1 - yOffset);
            
            // Get colors based on type
            Color backColor, borderColor, iconColor;
            string icon;
            GetColorsForType(_type, out backColor, out borderColor, out iconColor, out icon);
            
            // Draw shadow
            if (_animationProgress > 0.5f)
            {
                using (var shadowBrush = new SolidBrush(Color.FromArgb((int)(30 * _animationProgress), 0, 0, 0)))
                {
                    g.FillRectangle(shadowBrush, new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height));
                }
            }
            
            // Draw background
            using (var path = CreateRoundedRectangle(rect, 8))
            using (var brush = new SolidBrush(Color.FromArgb(opacity, backColor)))
            {
                g.FillPath(brush, path);
            }
            
            // Draw border
            using (var path = CreateRoundedRectangle(rect, 8))
            using (var pen = new Pen(Color.FromArgb(opacity, borderColor), 1))
            {
                g.DrawPath(pen, path);
            }
            
            // Draw icon
            var iconRect = new Rectangle(rect.X + 15, rect.Y + rect.Height / 2 - 15, 30, 30);
            using (var iconFont = new Font("Segoe MDL2 Assets", 16))
            using (var iconBrush = new SolidBrush(Color.FromArgb(opacity, iconColor)))
            {
                var iconSize = g.MeasureString(icon, iconFont);
                var iconX = iconRect.X + (iconRect.Width - iconSize.Width) / 2;
                var iconY = iconRect.Y + (iconRect.Height - iconSize.Height) / 2;
                g.DrawString(icon, iconFont, iconBrush, iconX, iconY);
            }
            
            // Draw close button
            var closeRect = new Rectangle(rect.Right - 30, rect.Y + 10, 20, 20);
            using (var closeFont = new Font("Segoe MDL2 Assets", 10))
            using (var closeBrush = new SolidBrush(Color.FromArgb(opacity / 2, ColorScheme.Modern.TextSecondary)))
            {
                g.DrawString("\uE711", closeFont, closeBrush, closeRect.X + 2, closeRect.Y + 2);
            }
            
            // Draw title
            var textX = iconRect.Right + 10;
            var textWidth = closeRect.Left - textX - 5;
            using (var titleFont = new Font("Segoe UI Semibold", 11))
            using (var titleBrush = new SolidBrush(Color.FromArgb(opacity, ColorScheme.Modern.TextPrimary)))
            {
                var titleRect = new Rectangle(textX, rect.Y + 15, textWidth, 20);
                g.DrawString(_title, titleFont, titleBrush, titleRect, 
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
            }
            
            // Draw message
            using (var messageFont = new Font("Segoe UI", 9))
            using (var messageBrush = new SolidBrush(Color.FromArgb(opacity * 3 / 4, ColorScheme.Modern.TextSecondary)))
            {
                var messageRect = new Rectangle(textX, rect.Y + 38, textWidth, rect.Height - 48);
                g.DrawString(_message, messageFont, messageBrush, messageRect, 
                    new StringFormat 
                    { 
                        Trimming = StringTrimming.EllipsisWord,
                        LineAlignment = StringAlignment.Near
                    });
            }
            
            // Draw progress bar
            if (_autoHideTimer.Enabled && _animationProgress >= 1f && !_isClosing)
            {
                var elapsed = _autoHideDuration - (_autoHideTimer.Interval);
                var progress = 1f - ((float)elapsed / _autoHideDuration);
                var progressRect = new Rectangle(rect.X, rect.Bottom - 3, (int)(rect.Width * progress), 3);
                using (var progressBrush = new SolidBrush(Color.FromArgb(opacity / 2, borderColor)))
                {
                    g.FillRectangle(progressBrush, progressRect);
                }
            }
        }
        
        private void GetColorsForType(ToastType type, out Color backColor, out Color borderColor, out Color iconColor, out string icon)
        {
            switch (type)
            {
                case ToastType.Success:
                    backColor = Color.FromArgb(245, 255, 245);
                    borderColor = Color.FromArgb(76, 175, 80);
                    iconColor = Color.FromArgb(76, 175, 80);
                    icon = "\uE73E"; // Checkmark
                    break;
                    
                case ToastType.Warning:
                    backColor = Color.FromArgb(255, 253, 245);
                    borderColor = Color.FromArgb(255, 152, 0);
                    iconColor = Color.FromArgb(255, 152, 0);
                    icon = "\uE7BA"; // Warning
                    break;
                    
                case ToastType.Error:
                    backColor = Color.FromArgb(255, 245, 245);
                    borderColor = Color.FromArgb(244, 67, 54);
                    iconColor = Color.FromArgb(244, 67, 54);
                    icon = "\uE783"; // Error
                    break;
                    
                case ToastType.Threat:
                    backColor = Color.FromArgb(255, 240, 240);
                    borderColor = Color.FromArgb(200, 0, 0);
                    iconColor = Color.FromArgb(200, 0, 0);
                    icon = "\uE897"; // Shield alert
                    break;
                    
                default: // Info
                    backColor = Color.FromArgb(245, 250, 255);
                    borderColor = ColorScheme.Modern.Primary;
                    iconColor = ColorScheme.Modern.Primary;
                    icon = "\uE946"; // Info
                    break;
            }
        }
        
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
        
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Clicked?.Invoke(this, e);
            
            // Check if click was on close button
            var closeRect = new Rectangle(Width - 30, 10, 20, 20);
            var mousePos = PointToClient(Cursor.Position);
            if (closeRect.Contains(mousePos))
            {
                Close();
            }
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            // Pause auto-hide when mouse is over
            _autoHideTimer.Stop();
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            // Resume auto-hide when mouse leaves
            if (!_isClosing && _animationProgress >= 1f)
            {
                RestartAutoHideTimer();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Dispose();
                _autoHideTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
