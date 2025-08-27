namespace ChildGuard.UI.Theming;

public static class ModernStyle
{
public static void Apply(Form form, ThemeMode mode)
    {
        bool dark = mode switch { ThemeMode.Dark => true, ThemeMode.Light => false, _ => ThemeHelper.IsSystemDark() };
        try { form.Font = new Font("Segoe UI Variable", 10f); } catch { try { form.Font = new Font("Segoe UI", 10f); } catch { } }
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.White;
        form.ForeColor = dark ? Color.FromArgb(230, 230, 230) : SystemColors.ControlText;
        form.MinimumSize = new Size(Math.Max(320, form.Width), Math.Max(200, form.Height));

        // Menu styling
        foreach (Control c in form.Controls)
        {
            if (c is MenuStrip ms)
            {
                var accent = ThemeHelper.GetAccentColor();
                ms.RenderMode = ToolStripRenderMode.Professional;
                ms.Renderer = new ToolStripProfessionalRenderer(new AccentColorTable(accent));
                ms.BackColor = dark ? Color.FromArgb(32,32,32) : Color.White;
                ms.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
            }
            // Improve buffering for layout panels to reduce flicker
            if (c is TableLayoutPanel or FlowLayoutPanel)
            {
                EnableDoubleBuffer(c);
                c.BackColor = form.BackColor;
                c.ForeColor = form.ForeColor;
            }
        }

        ApplyRecursive(form, dark);

        // Optional: try Windows 11 backdrop
        ThemeHelper.TryEnableMica(form);
        // Set icons for top menu if present
        foreach (Control c in form.Controls)
        {
            if (c is MenuStrip ms)
            {
                try
                {
                    var accent = ThemeHelper.GetAccentColor();
                    foreach (ToolStripItem it in ms.Items)
                    {
                        if (it is ToolStripMenuItem mi)
                        {
                            char g = mi.Text.Contains("Settings", StringComparison.OrdinalIgnoreCase) ? GlyphIcons.Settings :
                                     mi.Text.Contains("Reports", StringComparison.OrdinalIgnoreCase) ? GlyphIcons.Reports :
                                     mi.Text.Contains("Policy", StringComparison.OrdinalIgnoreCase) ? GlyphIcons.Policy :
                                     mi.Text.Contains("Help", StringComparison.OrdinalIgnoreCase) ? GlyphIcons.Info :
                                     mi.Text.Contains("About", StringComparison.OrdinalIgnoreCase) ? GlyphIcons.Info : '\0';
                            if (g != '\0') mi.Image = GlyphIcons.Render(g, 16, accent);
                            mi.ImageScaling = ToolStripItemImageScaling.None;
                        }
                    }
                }
                catch { }
            }
        }

    }

    public static void MakePrimary(Button b, bool dark)
    {
        var accent = ThemeHelper.GetAccentColor();
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = accent;
        b.ForeColor = Color.White;
        b.Padding = new Padding(10, 6, 10, 6);
        b.MouseEnter += (_, __) => b.BackColor = ControlPaint.Light(accent);
        b.MouseLeave += (_, __) => b.BackColor = accent;
    }

    public static void MakeSecondary(Button b, bool dark)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = dark ? Color.FromArgb(70,70,70) : Color.FromArgb(200,200,200);
        b.BackColor = dark ? Color.FromArgb(36,36,36) : Color.White;
        b.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
        b.Padding = new Padding(10, 6, 10, 6);
    }

    private static void ApplyRecursive(Control parent, bool dark)
    {
        foreach (Control c in parent.Controls)
        {
            switch (c)
            {
                case Button b:
                    b.FlatStyle = FlatStyle.System;
                    b.Height = Math.Max(32, Math.Max(30, b.Height));
                    b.Margin = new Padding(6);
                    b.UseVisualStyleBackColor = true;
                    b.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
                    break;
                case CheckBox cb:
                    cb.AutoSize = true;
                    cb.Margin = new Padding(4);
                    cb.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
                    break;
                case Label l:
                    l.Margin = new Padding(4);
                    l.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
                    break;
                case TextBox tb:
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.BackColor = dark ? Color.FromArgb(45,45,45) : Color.White;
                    tb.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    break;
                case ComboBox cbx:
                    cbx.FlatStyle = FlatStyle.System;
                    cbx.BackColor = dark ? Color.FromArgb(45,45,45) : Color.White;
                    cbx.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    break;
                case DateTimePicker dtp:
                    dtp.CalendarForeColor = dark ? Color.White : SystemColors.ControlText;
                    dtp.CalendarMonthBackground = dark ? Color.FromArgb(45,45,45) : Color.White;
                    dtp.CalendarTitleBackColor = ThemeHelper.GetAccentColor();
                    dtp.CalendarTitleForeColor = Color.White;
                    dtp.BackColor = dark ? Color.FromArgb(45,45,45) : Color.White;
                    dtp.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    break;
                case NumericUpDown nud:
                    nud.BackColor = dark ? Color.FromArgb(45,45,45) : Color.White;
                    nud.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    break;
                case GroupBox gb:
                    gb.ForeColor = dark ? Color.FromArgb(235,235,235) : SystemColors.ControlText;
                    gb.BackColor = dark ? Color.FromArgb(30,30,30) : Color.White;
                    break;
                case DataGridView dgv:
                    StyleGrid(dgv, dark);
                    break;
                case TableLayoutPanel tlp:
                    tlp.BackColor = dark ? Color.FromArgb(30,30,30) : Color.White;
                    tlp.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    EnableDoubleBuffer(tlp);
                    break;
                case FlowLayoutPanel flp:
                    flp.BackColor = dark ? Color.FromArgb(30,30,30) : Color.White;
                    flp.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    EnableDoubleBuffer(flp);
                    break;
                case Panel pnl:
                    pnl.BackColor = dark ? Color.FromArgb(30,30,30) : Color.White;
                    pnl.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
                    EnableDoubleBuffer(pnl);
                    break;
            }
            if (c.HasChildren) ApplyRecursive(c, dark);
        }
    }

    private static void StyleGrid(DataGridView grid, bool dark)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = dark ? Color.FromArgb(30,30,30) : Color.White;
        grid.EnableHeadersVisualStyles = false;
        var accent = ThemeHelper.GetAccentColor();
        grid.ColumnHeadersDefaultCellStyle.BackColor = dark ? Color.FromArgb(45,45,45) : Color.FromArgb(240, 240, 240);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
        grid.ColumnHeadersHeight = 34;
        grid.RowHeadersVisible = false;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = dark ? Color.FromArgb(60,60,60) : Color.FromArgb(230, 230, 230);
        grid.AlternatingRowsDefaultCellStyle.BackColor = dark ? Color.FromArgb(36,36,36) : Color.FromArgb(248, 248, 248);
        grid.DefaultCellStyle.BackColor = dark ? Color.FromArgb(30,30,30) : Color.White;
        grid.DefaultCellStyle.ForeColor = dark ? Color.FromArgb(230,230,230) : SystemColors.ControlText;
        grid.DefaultCellStyle.SelectionBackColor = ControlPaint.Light(accent);
        grid.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
    }

    private static void EnableDoubleBuffer(Control ctrl)
    {
        try
        {
            ctrl.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(ctrl, true, null);
        }
        catch { }
    }
}

