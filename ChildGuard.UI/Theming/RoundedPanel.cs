using System.Drawing.Drawing2D;

namespace ChildGuard.UI.Theming;

public class RoundedPanel : Panel
{
    public int CornerRadius { get; set; } = 8;
    public Color BorderColor { get; set; } = Color.FromArgb(220,220,220);
    public int BorderThickness { get; set; } = 1;
    public bool Dark { get; set; } = false;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = ClientRectangle;
        rect.Width -= 1; rect.Height -= 1;
        using var path = GetRoundedRect(rect, CornerRadius);
        using var pen = new Pen(Dark ? Color.FromArgb(70,70,70) : BorderColor, BorderThickness);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        var rect = ClientRectangle;
        rect.Width -= 1; rect.Height -= 1;
        using var path = GetRoundedRect(rect, CornerRadius);
        this.Region = new Region(path);
    }

    private static GraphicsPath GetRoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.StartFigure();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

