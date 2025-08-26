namespace ChildGuard.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        bool openSettings = false, openReports = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--open", StringComparison.OrdinalIgnoreCase) && i+1 < args.Length)
            {
                var v = args[i+1].ToLowerInvariant();
                if (v == "settings") openSettings = true;
                if (v == "reports") openReports = true;
                i++;
            }
        }
        var f = new Form1();
        f.Shown += (s, e) =>
        {
            if (openSettings)
            {
                try { new SettingsForm().Show(f); } catch { }
            }
            if (openReports)
            {
                try { new ReportsForm().Show(f); } catch { }
            }
        };
        Application.Run(f);
    }    
}
