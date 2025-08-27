namespace ChildGuard.UI.Theming;

public static class GlyphIcons
{
    // Common Segoe glyphs (MDL2 Assets) - fallback codes
    public const char Settings = '\uE713';
    public const char Reports = '\uE9D2'; // BarChart4
    public const char Policy = '\uE70F';  // Edit
    public const char Keyboard = '\uE92E';
    public const char Mouse = '\uE962';
    public const char Info = '\uE946';    // Info

    public static Bitmap Render(char glyph, int size, Color color)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var font = ResolveIconFont(size);
        using var brush = new SolidBrush(color);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size, size), sf);
        return bmp;
    }

    private static Font ResolveIconFont(int size)
    {
        // Prefer Segoe Fluent Icons (Win11), fallback to Segoe MDL2 Assets
        try { return new Font("Segoe Fluent Icons", size - 2, FontStyle.Regular, GraphicsUnit.Pixel); } catch { }
        try { return new Font("Segoe MDL2 Assets", size - 2, FontStyle.Regular, GraphicsUnit.Pixel); } catch { }
        return new Font(SystemFonts.DefaultFont.FontFamily, size - 2, FontStyle.Regular, GraphicsUnit.Pixel);
    }
}

