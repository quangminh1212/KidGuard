using System.Runtime.InteropServices;

namespace ChildGuard.UI.Theming;

public static class ThemeHelper
{
    // Try to detect system light/dark preference
    public static bool IsSystemDark()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value is int i) return i == 0; // 0 means dark
            }
        }
        catch { }
        return false;
    }

    // Try to get Windows accent color via DWM; fallback to SystemColors.Highlight
    public static Color GetAccentColor()
    {
        try
        {
            if (DwmGetColorizationColor(out uint color, out _))
            {
                // color is ARGB
                byte a = (byte)((color >> 24) & 0xFF);
                byte r = (byte)((color >> 16) & 0xFF);
                byte g = (byte)((color >> 8) & 0xFF);
                byte b = (byte)(color & 0xFF);
                if (a == 0) a = 255;
                return Color.FromArgb(a, r, g, b);
            }
        }
        catch { }
        return SystemColors.Highlight;
    }

    // Optional: try to enable Windows 11 Mica backdrop if available
    public static void TryEnableMica(Form form)
    {
        try
        {
            // Windows 11 build 22000+
            Version v = Environment.OSVersion.Version;
            if (v.Major < 10 || (v.Major == 10 && v.Build < 22000)) return;

            int DWMWA_SYSTEMBACKDROP_TYPE = 38; // Only available on Windows 11; ignored elsewhere
            int DWM_SBT_MAINWINDOW = 2;         // Mica-like backdrop
            DwmSetWindowAttribute(form.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref DWM_SBT_MAINWINDOW, sizeof(int));
        }
        catch { }
    }

    [DllImport("dwmapi.dll")]
    private static extern bool DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
}

