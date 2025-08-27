using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Localization;

namespace ChildGuard.UI.Theming;

public static class SimpleModernLayout
{
    public static void ApplyToForm(Form1 form, bool isDark)
    {
        form.SuspendLayout();
        
        // Clear existing dynamic controls (keep menu and hidden controls)
        var toRemove = new List<Control>();
        foreach (Control c in form.Controls)
        {
            if (c.Name == "mainPanel" || c.Name == "contentPanel")
                toRemove.Add(c);
        }
        foreach (var c in toRemove)
            form.Controls.Remove(c);
        
        // Setup colors
        var bgColor = isDark ? Color.FromArgb(28, 28, 28) : Color.FromArgb(248, 249, 250);
        var cardBg = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
        var textPrimary = isDark ? Color.FromArgb(255, 255, 255) : Color.FromArgb(32, 33, 36);
        var textSecondary = isDark ? Color.FromArgb(154, 160, 166) : Color.FromArgb(95, 99, 104);
        var borderColor = isDark ? Color.FromArgb(48, 49, 51) : Color.FromArgb(218, 220, 224);
        var accentColor = Color.FromArgb(26, 115, 232);
        
        form.BackColor = bgColor;
        form.ForeColor = textPrimary;
        
        // Main container
        // Get menustrip height
        var menuStrip = form.Controls.OfType<MenuStrip>().FirstOrDefault();
        var menuHeight = menuStrip?.Height ?? 24;
        
        var container = new Panel
        {
            Name = "mainPanel",
            Location = new Point(0, menuHeight),
            Size = new Size(form.ClientSize.Width, form.ClientSize.Height - menuHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = bgColor,
            AutoScroll = true
        };
        
        // Content wrapper with proper padding
        var content = new Panel
        {
            Location = new Point(48, 32),
            Size = new Size(container.Width - 96, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.Transparent
        };
        
        // Title Section
        var titleLabel = new Label
        {
            Text = "Giám sát hệ thống",
            Font = new Font("Segoe UI", 24, FontStyle.Regular),
            ForeColor = textPrimary,
            Location = new Point(0, 0),
            AutoSize = true
        };
        content.Controls.Add(titleLabel);
        
        var subtitleLabel = new Label
        {
            Text = "Theo dõi hoạt động bàn phím và chuột",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = textSecondary,
            Location = new Point(0, 40),
            AutoSize = true
        };
        content.Controls.Add(subtitleLabel);
        
        // Stats Cards Container
        var statsContainer = new Panel
        {
            Location = new Point(0, 90),
            Size = new Size(600, 120),
            BackColor = Color.Transparent
        };
        
        // Keyboard Stats Card
        var keyboardCard = CreateStatsCard(
            "Bàn phím",
            "0",
            "lượt nhấn",
            Color.FromArgb(66, 133, 244),
            cardBg,
            textPrimary,
            textSecondary,
            isDark
        );
        keyboardCard.Location = new Point(0, 0);
        keyboardCard.Name = "keyboardCard";
        statsContainer.Controls.Add(keyboardCard);
        
        // Mouse Stats Card
        var mouseCard = CreateStatsCard(
            "Chuột",
            "0",
            "sự kiện",
            Color.FromArgb(52, 168, 83),
            cardBg,
            textPrimary,
            textSecondary,
            isDark
        );
        mouseCard.Location = new Point(300, 0);
        mouseCard.Name = "mouseCard";
        statsContainer.Controls.Add(mouseCard);
        
        content.Controls.Add(statsContainer);
        
        // Controls Section
        var controlsLabel = new Label
        {
            Text = "Điều khiển",
            Font = new Font("Segoe UI", 14, FontStyle.Regular),
            ForeColor = textPrimary,
            Location = new Point(0, 240),
            AutoSize = true
        };
        content.Controls.Add(controlsLabel);
        
        var controlsPanel = new Panel
        {
            Location = new Point(0, 275),
            Size = new Size(600, 80),
            BackColor = cardBg
        };
        
        // Add border
        controlsPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(borderColor, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, controlsPanel.Width - 1, controlsPanel.Height - 1);
        };
        
        // Modern Checkbox
        var monitoringCheck = new CheckBox
        {
            Text = "Bật theo dõi bàn phím/chuột",
            Font = new Font("Segoe UI", 10),
            ForeColor = textPrimary,
            Location = new Point(24, 28),
            AutoSize = true,
            Name = "monitoringCheck"
        };
        controlsPanel.Controls.Add(monitoringCheck);
        
        // Start Button
        var startBtn = CreateModernButton(
            "Bắt đầu",
            accentColor,
            Color.White,
            new Point(380, 20),
            new Size(90, 40)
        );
        startBtn.Name = "startBtn";
        controlsPanel.Controls.Add(startBtn);
        
        // Stop Button  
        var stopBtn = CreateModernButton(
            "Dừng",
            isDark ? Color.FromArgb(95, 99, 104) : Color.FromArgb(218, 220, 224),
            isDark ? Color.White : Color.FromArgb(32, 33, 36),
            new Point(480, 20),
            new Size(90, 40)
        );
        stopBtn.Name = "stopBtn";
        controlsPanel.Controls.Add(stopBtn);
        
        content.Controls.Add(controlsPanel);
        
        // Add all to form
        container.Controls.Add(content);
        form.Controls.Add(container);
        container.BringToFront();
        if (menuStrip != null)
            menuStrip.BringToFront();
            
        form.ResumeLayout();
    }
    
    private static Panel CreateStatsCard(string title, string value, string unit, Color accentColor, 
        Color bgColor, Color textPrimary, Color textSecondary, bool isDark)
    {
        var card = new Panel
        {
            Size = new Size(280, 120),
            BackColor = bgColor
        };
        
        // Card border and shadow
        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Shadow for light theme
            if (!isDark)
            {
                using var shadowBrush = new SolidBrush(Color.FromArgb(8, Color.Black));
                g.FillRectangle(shadowBrush, 1, 1, card.Width, card.Height);
            }
            
            // Border
            using var borderPen = new Pen(isDark ? Color.FromArgb(48, 49, 51) : Color.FromArgb(218, 220, 224), 1);
            g.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
            
            // Top accent line
            using var accentBrush = new SolidBrush(accentColor);
            g.FillRectangle(accentBrush, 0, 0, card.Width, 3);
        };
        
        // Icon circle
        var iconPanel = new Panel
        {
            Size = new Size(48, 48),
            Location = new Point(20, 36),
            BackColor = Color.Transparent
        };
        iconPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(20, accentColor));
            g.FillEllipse(brush, 0, 0, 47, 47);
            
            // Draw simple icon
            using var iconBrush = new SolidBrush(accentColor);
            var iconFont = new Font("Segoe UI", 20);
            var icon = title.Contains("Bàn") ? "⌨" : "🖱";
            g.DrawString(icon, iconFont, iconBrush, 12, 8);
        };
        card.Controls.Add(iconPanel);
        
        // Title
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10),
            ForeColor = textSecondary,
            Location = new Point(80, 25),
            AutoSize = true
        };
        card.Controls.Add(titleLabel);
        
        // Value
        var valueLabel = new Label
        {
            Text = value,
            Name = title.Contains("Bàn") ? "keyValue" : "mouseValue",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = textPrimary,
            Location = new Point(80, 45),
            AutoSize = true
        };
        card.Controls.Add(valueLabel);
        
        // Unit
        var unitLabel = new Label
        {
            Text = unit,
            Font = new Font("Segoe UI", 9),
            ForeColor = textSecondary,
            Location = new Point(80, 80),
            AutoSize = true
        };
        card.Controls.Add(unitLabel);
        
        return card;
    }
    
    private static Button CreateModernButton(string text, Color bgColor, Color fgColor, Point location, Size size)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 10),
            Location = location,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = bgColor,
            ForeColor = fgColor,
            Cursor = Cursors.Hand
        };
        
        btn.FlatAppearance.BorderSize = 0;
        
        // Hover effect
        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = ControlPaint.Light(bgColor, 0.1f);
        };
        
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = bgColor;
        };
        
        return btn;
    }
}
