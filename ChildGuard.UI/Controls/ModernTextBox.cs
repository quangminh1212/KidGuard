using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// TextBox hiện đại theo phong cách Windows 11 và Facebook
    /// </summary>
    public class ModernTextBox : UserControl
    {
        private TextBox textBox;
        private Label placeholderLabel;
        private Label titleLabel;
        private Panel underlinePanel;
        private Timer animationTimer;
        private bool isFocused = false;
        private int animationStep = 0;
        private string placeholderText = "Enter text...";
        private string titleText = "";
        private Color focusColor = ColorScheme.Modern.Primary;
        private int cornerRadius = 8;
        private bool showUnderline = true;
        private bool showBorder = false;

        [Category("Modern Style")]
        [Description("Placeholder text")]
        public string PlaceholderText
        {
            get => placeholderText;
            set
            {
                placeholderText = value;
                placeholderLabel.Text = value;
                UpdatePlaceholderVisibility();
            }
        }

        [Category("Modern Style")]
        [Description("Title text")]
        public string Title
        {
            get => titleText;
            set
            {
                titleText = value;
                titleLabel.Text = value;
                titleLabel.Visible = !string.IsNullOrEmpty(value);
                UpdateLayout();
            }
        }

        [Category("Modern Style")]
        [Description("Hiển thị underline")]
        public bool ShowUnderline
        {
            get => showUnderline;
            set
            {
                showUnderline = value;
                underlinePanel.Visible = value;
                UpdateLayout();
            }
        }

        [Category("Modern Style")]
        [Description("Hiển thị border")]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
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

        [Category("Modern Style")]
        [Description("Màu focus")]
        public Color FocusColor
        {
            get => focusColor;
            set
            {
                focusColor = value;
                Invalidate();
            }
        }

        public override string Text
        {
            get => textBox.Text;
            set
            {
                textBox.Text = value;
                UpdatePlaceholderVisibility();
            }
        }

        public bool UseSystemPasswordChar
        {
            get => textBox.UseSystemPasswordChar;
            set => textBox.UseSystemPasswordChar = value;
        }

        public bool Multiline
        {
            get => textBox.Multiline;
            set
            {
                textBox.Multiline = value;
                UpdateLayout();
            }
        }

        public ScrollBars ScrollBars
        {
            get => textBox.ScrollBars;
            set => textBox.ScrollBars = value;
        }

        public ModernTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = ColorScheme.Modern.Surface;
            Size = new Size(250, 48);
            Font = new Font("Segoe UI", 10f);

            InitializeComponents();
            UpdateLayout();

            // Animation timer
            animationTimer = new Timer();
            animationTimer.Interval = 20;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void InitializeComponents()
        {
            // Title Label
            titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = ColorScheme.Modern.TextSecondary,
                BackColor = Color.Transparent,
                Text = titleText,
                Visible = false
            };
            Controls.Add(titleLabel);

            // TextBox
            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                ForeColor = ColorScheme.Modern.TextPrimary,
                BackColor = BackColor
            };
            textBox.TextChanged += TextBox_TextChanged;
            textBox.Enter += TextBox_Enter;
            textBox.Leave += TextBox_Leave;
            Controls.Add(textBox);

            // Placeholder Label
            placeholderLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10f),
                ForeColor = ColorScheme.Modern.TextDisabled,
                BackColor = Color.Transparent,
                Text = placeholderText,
                Cursor = Cursors.IBeam
            };
            placeholderLabel.Click += (s, e) => textBox.Focus();
            Controls.Add(placeholderLabel);

            // Underline Panel
            underlinePanel = new Panel
            {
                Height = 2,
                BackColor = ColorScheme.Modern.Border,
                Visible = showUnderline
            };
            Controls.Add(underlinePanel);
        }

        private void UpdateLayout()
        {
            int padding = 12;
            int titleHeight = titleLabel.Visible ? titleLabel.Height + 4 : 0;

            // Title
            if (titleLabel.Visible)
            {
                titleLabel.Location = new Point(padding, 4);
            }

            // TextBox
            int textBoxY = titleHeight + padding;
            int textBoxHeight = Height - textBoxY - padding - (showUnderline ? 2 : 0);
            
            textBox.Location = new Point(padding, textBoxY);
            textBox.Size = new Size(Width - padding * 2, textBoxHeight);

            // Placeholder
            placeholderLabel.Location = new Point(padding + 2, textBoxY + 2);

            // Underline
            if (showUnderline)
            {
                underlinePanel.Location = new Point(0, Height - 2);
                underlinePanel.Width = Width;
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            placeholderLabel.Visible = string.IsNullOrEmpty(textBox.Text) && !isFocused;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (SolidBrush bgBrush = new SolidBrush(BackColor))
            {
                if (cornerRadius > 0)
                {
                    using (GraphicsPath path = CreateRoundedRectangle(ClientRectangle, cornerRadius))
                    {
                        g.FillPath(bgBrush, path);
                    }
                }
                else
                {
                    g.FillRectangle(bgBrush, ClientRectangle);
                }
            }

            // Draw border
            if (showBorder || isFocused)
            {
                Color borderColor = isFocused ? focusColor : ColorScheme.Modern.Border;
                int borderWidth = isFocused ? 2 : 1;
                
                using (Pen borderPen = new Pen(borderColor, borderWidth))
                {
                    Rectangle borderRect = new Rectangle(
                        borderWidth / 2, 
                        borderWidth / 2, 
                        Width - borderWidth, 
                        Height - borderWidth);

                    if (cornerRadius > 0)
                    {
                        using (GraphicsPath path = CreateRoundedRectangle(borderRect, cornerRadius))
                        {
                            g.DrawPath(borderPen, path);
                        }
                    }
                    else
                    {
                        g.DrawRectangle(borderPen, borderRect);
                    }
                }
            }

            // Draw focus underline animation
            if (showUnderline && animationStep > 0)
            {
                int underlineWidth = (int)(Width * animationStep / 10f);
                int underlineX = (Width - underlineWidth) / 2;
                
                using (SolidBrush underlineBrush = new SolidBrush(focusColor))
                {
                    g.FillRectangle(underlineBrush, underlineX, Height - 2, underlineWidth, 2);
                }
            }
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

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdatePlaceholderVisibility();
            OnTextChanged(e);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            isFocused = true;
            animationStep = 0;
            animationTimer.Start();
            UpdatePlaceholderVisibility();
            
            if (showUnderline)
            {
                underlinePanel.BackColor = focusColor;
            }
            
            Invalidate();
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            isFocused = false;
            UpdatePlaceholderVisibility();
            
            if (showUnderline)
            {
                underlinePanel.BackColor = ColorScheme.Modern.Border;
            }
            
            Invalidate();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationStep++;
            
            if (animationStep >= 10)
            {
                animationTimer.Stop();
            }
            
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
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
