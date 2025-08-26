namespace ChildGuard.UI.Theming;

public class AccentColorTable : ProfessionalColorTable
{
    private readonly Color _accent;
    public AccentColorTable(Color accent)
    {
        _accent = accent;
        UseSystemColors = false;
    }

    public override Color MenuItemSelected => Lighten(_accent, 0.20);
    public override Color MenuItemBorder => _accent;
    public override Color MenuItemSelectedGradientBegin => Lighten(_accent, 0.15);
    public override Color MenuItemSelectedGradientEnd => Lighten(_accent, 0.05);
    public override Color ToolStripGradientBegin => Color.White;
    public override Color ToolStripGradientMiddle => Color.White;
    public override Color ToolStripGradientEnd => Color.White;
    public override Color ImageMarginGradientBegin => Color.White;
    public override Color ImageMarginGradientEnd => Color.White;

    private static Color Lighten(Color c, double amount)
    {
        int r = (int)Math.Min(255, c.R + 255 * amount);
        int g = (int)Math.Min(255, c.G + 255 * amount);
        int b = (int)Math.Min(255, c.B + 255 * amount);
        return Color.FromArgb(c.A, r, g, b);
    }
}

