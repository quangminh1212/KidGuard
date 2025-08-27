using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Button hiện đại theo phong cách Windows 11 và Facebook
    /// </summary>
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        private System.Windows.Forms.Timer animationTimer;
        private int animationStep = 0;
        private ButtonStyle buttonStyle = ButtonStyle.Primary;
        private int cornerRadius = 8;

        public enum ButtonStyle
        {
            Primary,
            Secondary,
            Success,
            Danger,
            Warning,
            Ghost,
            Flat
        }

        [Category("Modern Style")]
        [Description("Kiểu button")]
        public ButtonStyle Style
        {
            get => buttonStyle;
            set
            {
                buttonStyle = value;
                UpdateAppearance();
                Invalidate();
            }
        }

        [Category("Modern Style")]
        [Description("Độ bo góc")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, Math.Min(value, Height / 2));
                Invalidate();
            }
        }

        public ModernButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            Size = new Size(120, 40);
            Cursor = Cursors.Hand;

            // Animation timer
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;
            animationTimer.Tick += AnimationTimer_Tick;

            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            switch (buttonStyle)
            {
                case ButtonStyle.Primary:
                    BackColor = ColorScheme.Modern.Primary;
                    ForeColor = Color.White;
                    break;
                case ButtonStyle.Secondary:
                    BackColor = ColorScheme.Modern.SurfaceVariant;
                    ForeColor = ColorScheme.Modern.TextPrimary;
                    break;
                case ButtonStyle.Success:
                    BackColor = ColorScheme.Modern.Success;
                    ForeColor = Color.White;
                    break;
                case ButtonStyle.Danger:
                    BackColor = ColorScheme.Modern.Error;
                    ForeColor = Color.White;
                    break;
                case ButtonStyle.Warning:
                    BackColor = ColorScheme.Modern.Warning;
                    ForeColor = ColorScheme.Modern.TextPrimary;
                    break;
                case ButtonStyle.Ghost:
                    BackColor = Color.Transparent;
                    ForeColor = ColorScheme.Modern.Primary;
                    break;
                case ButtonStyle.Flat:
                    BackColor = Color.Transparent;
                    ForeColor = ColorScheme.Modern.TextSecondary;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            
            // Tạo path với góc bo tròn
            using (GraphicsPath path = CreateRoundedRectangle(rect, cornerRadius))
            {
                // Vẽ nền
                Color bgColor = GetBackgroundColor();
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Vẽ shadow cho Primary và Success buttons
                if (buttonStyle == ButtonStyle.Primary || buttonStyle == ButtonStyle.Success)
                {
                    DrawShadow(g, path);
                }

                // Vẽ border cho Ghost và Flat styles khi hover
                if ((buttonStyle == ButtonStyle.Ghost || buttonStyle == ButtonStyle.Flat) && isHovered)
                {
                    using (Pen borderPen = new Pen(ColorScheme.Modern.Border, 1))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                // Vẽ ripple effect khi click
                if (isPressed && animationStep > 0)
                {
                    DrawRippleEffect(g, path);
                }

                // Vẽ text
                DrawButtonText(g);
            }
        }

        private void DrawShadow(Graphics g, GraphicsPath path)
        {
            if (!Enabled || buttonStyle == ButtonStyle.Ghost || buttonStyle == ButtonStyle.Flat)
                return;

            Rectangle shadowRect = new Rectangle(2, 2, Width - 4, Height - 4);
            using (GraphicsPath shadowPath = CreateRoundedRectangle(shadowRect, cornerRadius))
            {
                using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = ColorScheme.Modern.ShadowMedium;
                    shadowBrush.SurroundColors = new[] { Color.Transparent };
                    shadowBrush.FocusScales = new PointF(0.8f, 0.8f);
                    g.FillPath(shadowBrush, shadowPath);
                }
            }
        }

        private void DrawRippleEffect(Graphics g, GraphicsPath path)
        {
            int maxRadius = Math.Max(Width, Height);
            int currentRadius = (int)(maxRadius * animationStep / 10f);
            
            Point center = new Point(Width / 2, Height / 2);
            
            if (currentRadius > 0)
            {
                using (GraphicsPath ripplePath = new GraphicsPath())
                {
                    ripplePath.AddEllipse(center.X - currentRadius, center.Y - currentRadius, 
                                         currentRadius * 2, currentRadius * 2);
                    
                    Region clipRegion = new Region(path);
                    g.SetClip(clipRegion, CombineMode.Intersect);
                    
                    int alpha = Math.Max(0, 60 - animationStep * 6);
                    using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    {
                        g.FillPath(rippleBrush, ripplePath);
                    }
                    
                    g.ResetClip();
                    clipRegion.Dispose();
                }
            }
        }

        private void DrawButtonText(Graphics g)
        {
            Color textColor = GetTextColor();
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            
            Rectangle textRect = new Rectangle(0, 0, Width, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
        }

        private Color GetBackgroundColor()
        {
            if (!Enabled)
            {
                return ColorScheme.Modern.SurfaceVariant;
            }

            Color baseColor = BackColor;
            
            if (isPressed)
            {
                return ColorScheme.Darken(baseColor, 0.1f);
            }
            else if (isHovered)
            {
                return ColorScheme.Lighten(baseColor, 0.05f);
            }
            
            return baseColor;
        }

        private Color GetTextColor()
        {
            if (!Enabled)
            {
                return ColorScheme.Modern.TextDisabled;
            }
            
            return ForeColor;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddLine(rect.X + radius, rect.Y, rect.Right - radius * 2, rect.Y);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom - radius * 2);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddLine(rect.Right - radius * 2, rect.Bottom, rect.X + radius, rect.Bottom);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.AddLine(rect.X, rect.Bottom - radius * 2, rect.X, rect.Y + radius);
            path.CloseFigure();

            return path;
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                animationStep = 0;
                animationTimer.Start();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isPressed = false;
            Invalidate();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationStep++;
            
            if (animationStep >= 10)
            {
                animationTimer.Stop();
                animationStep = 0;
            }
            
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
