using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ChildGuard.UI.Theming;

public class ToggleSwitch : CheckBox
{
    [Browsable(true)] public Color OnBackColor { get; set; } = Color.FromArgb(0, 120, 215);
    [Browsable(true)] public Color OffBackColor { get; set; } = Color.FromArgb(200, 200, 200);
    [Browsable(true)] public Color OnToggleColor { get; set; } = Color.White;
    [Browsable(true)] public Color OffToggleColor { get; set; } = Color.White;
    [Browsable(true)] public int ToggleSize { get; set; } = 20;

    public ToggleSwitch()
    {
        MinimumSize = new Size(40, 22);
        AutoSize = false;
        Padding = new Padding(0);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);
        int toggleSize = ToggleSize;
        int trackHeight = Math.Max(22, toggleSize + 4);
        int trackWidth = Math.Max(40, toggleSize * 2 + 8);
        var rect = new Rectangle(0, (Height - trackHeight)/2, trackWidth, trackHeight);

        using var path = GetRounded(rect, rect.Height/2);
        using var backBrush = new SolidBrush(Checked ? OnBackColor : OffBackColor);
        e.Graphics.FillPath(backBrush, path);

        int pad = 2;
        int x = Checked ? rect.Right - toggleSize - pad : rect.Left + pad;
        int y = rect.Top + (rect.Height - toggleSize)/2;
        var toggleRect = new Rectangle(x, y, toggleSize, toggleSize);
        using var toggleBrush = new SolidBrush(Checked ? OnToggleColor : OffToggleColor);
        e.Graphics.FillEllipse(toggleBrush, toggleRect);
    }

    private static GraphicsPath GetRounded(Rectangle r, int radius)
    {
        var p = new GraphicsPath();
        int d = radius * 2;
        p.StartFigure();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}
